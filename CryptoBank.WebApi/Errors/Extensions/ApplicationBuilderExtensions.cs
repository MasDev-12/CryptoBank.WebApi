using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using CryptoBank.WebApi.Errors.Exceptions;

namespace CryptoBank.WebApi.Errors.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
                var exception = exceptionHandlerPathFeature.Error;

                switch (exception)
                {
                    case ValidationErrorsException validationErrorsException:
                        {

                            var validationProblemDetails = HandleProblemDetails("Validation failed"
                                , "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400"
                                , 400, validationErrorsException.Message, context);

                            validationProblemDetails.Extensions["errors"] = validationErrorsException.Errors
                                .Select(x => new ErrorData(x.Field, x.Message, x.Code));

                            await context.Response.WriteAsync(JsonSerializer.Serialize(validationProblemDetails));
                            break;
                        }
                    case LogicConflictException logicConflictException:
                        {
                            var logicConflictExceptionProblemDetails = HandleProblemDetails("Logic conflict"
                                , "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422"
                                , 422, logicConflictException.Message, context);

                            logicConflictExceptionProblemDetails.Extensions["code"] = logicConflictException.Code;

                            await context.Response.WriteAsync(JsonSerializer.Serialize(logicConflictExceptionProblemDetails));
                            break;
                        }
                    case OperationCanceledException:
                        {
                            var operationCanceledProblemDetails = HandleProblemDetails("Timeout"
                                , "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/504"
                                , 504, "Request timed out", context);

                            await context.Response.WriteAsync(JsonSerializer.Serialize(operationCanceledProblemDetails));
                            break;
                        }
                    default:
                        {
                            var internalErrorProblemDetails = HandleProblemDetails("Internal server error"
                                , "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500"
                                , 500, "Interval server error has occured", context);

                            await context.Response.WriteAsync(JsonSerializer.Serialize(internalErrorProblemDetails));
                            break;
                        }
                    }
                });
            });
        return app;
    }

    private static ProblemDetails HandleProblemDetails(string title, string type, int status, string message, HttpContext httpContext)
    {
        var validatorProblemDetails = new ProblemDetails
        {
            Title = title,
            Type = type,
            Detail = message,
            Status = status,
        };
        validatorProblemDetails.Extensions.Add("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = status;

        return validatorProblemDetails;
    }
}

internal record ErrorData(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("code")] string Code);
