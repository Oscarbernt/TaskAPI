using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TaskHub.API.Infrastructure.Middleware;

namespace TaskHub.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoException_CallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (HttpContext _) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var environment = Mock.Of<IHostEnvironment>();
        var middleware = new GlobalExceptionHandlerMiddleware(next, logger, environment);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_ReturnsBadRequestWithValidationProblemDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext _) =>
        {
            throw new ArgumentException("Title is required.", "title");
        };

        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var environment = Mock.Of<IHostEnvironment>();
        var middleware = new GlobalExceptionHandlerMiddleware(next, logger, environment);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.NotEmpty(responseBody);

        // Deserialize as JsonDocument to inspect actual structure
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("title", out var titleProp), $"Response: {responseBody}");
        Assert.Equal("One or more validation errors occurred.", titleProp.GetString());

        Assert.True(root.TryGetProperty("status", out var statusProp));
        Assert.Equal(400, statusProp.GetInt32());

            Assert.True(root.TryGetProperty("errors", out var errorsProp), $"Response: {responseBody}");
            Assert.True(errorsProp.TryGetProperty("Title", out var titleErrorProp), $"Errors: {errorsProp}");
            var errors = titleErrorProp.EnumerateArray().Select(e => e.GetString()).ToArray();
            Assert.Contains(errors, e => e != null && e.Contains("Title is required"));
        }

    [Fact]
    public async Task InvokeAsync_ArgumentExceptionWithDueDate_MapsParameterCorrectly()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext _) =>
        {
            throw new ArgumentException("DueDate must be in the future.", "dueDate");
        };

        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var environment = Mock.Of<IHostEnvironment>();
        var middleware = new GlobalExceptionHandlerMiddleware(next, logger, environment);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("errors", out var errorsProp));
        Assert.True(errorsProp.TryGetProperty("DueDate", out _));
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext _) =>
        {
            throw new InvalidOperationException("Something went wrong");
        };

        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        var middleware = new GlobalExceptionHandlerMiddleware(next, logger, mockEnvironment.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problemDetails);
        Assert.Equal("An error occurred while processing your request.", problemDetails.Title);
        Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        Assert.NotNull(problemDetails.Extensions);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task InvokeAsync_GenericExceptionInDevelopment_IncludesDetailedError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext _) =>
        {
            throw new InvalidOperationException("Detailed error message");
        };

        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        var middleware = new GlobalExceptionHandlerMiddleware(next, logger, mockEnvironment.Object);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(problemDetails);
        Assert.NotNull(problemDetails.Detail);
        Assert.Contains("Detailed error message", problemDetails.Detail);
    }

    [Fact]
    public async Task InvokeAsync_NullContext_ThrowsArgumentNullException()
    {
        var logger = Mock.Of<ILogger<GlobalExceptionHandlerMiddleware>>();
        var environment = Mock.Of<IHostEnvironment>();
        var middleware = new GlobalExceptionHandlerMiddleware(
            (HttpContext _) => Task.CompletedTask,
            logger,
            environment);

        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
    }
}
