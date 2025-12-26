using System.Text.Json;
using Timecrax.Api.Domain.Exceptions;

namespace Timecrax.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            // Regras de domínio → 400
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = ex.Message,
                field = ex.Field
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        catch (Exception ex)
        {
            // Erros inesperados → 500
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = "Unexpected error.",
                traceId = context.TraceIdentifier
            };

            // Em DEV, é útil logar a exceção completa
            // (não devolva detalhes ao client)
            Console.Error.WriteLine(ex);

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
