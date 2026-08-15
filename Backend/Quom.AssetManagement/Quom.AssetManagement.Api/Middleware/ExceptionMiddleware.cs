using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace Quom.AssetManagement.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                // SQL Server utiliza estos códigos para violaciones de unicidad
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "Ya existe un registro con alguno de los datos únicos proporcionados.");
            }
            catch (SqlException ex) when (ex.Number >= 50100 && ex.Number <= 50105)
            {
                var statusCode = ex.Number switch
                {
                    50101 => HttpStatusCode.NotFound,   // Activo no existe
                    50103 => HttpStatusCode.NotFound,   // Colaborador no existe
                    50102 => HttpStatusCode.Conflict,   // Activo no disponible
                    50105 => HttpStatusCode.Conflict,   // Ya existe asignación activa

                    // Usuario inactivo o empleado inactivo
                    _ => HttpStatusCode.BadRequest
                };

                await WriteResponseAsync(
                    context,
                    statusCode,
                    ex.Message);
            }
            catch (SqlException ex) when (ex.Number >= 50200 && ex.Number <= 50203)
            {
                var statusCode = ex.Number switch
                {
                    50201 => HttpStatusCode.NotFound,   // Activo no existe
                    50202 => HttpStatusCode.Conflict,   // Activo no está asignado
                    50203 => HttpStatusCode.Conflict,   // No existe asignación activa
                    _ => HttpStatusCode.BadRequest
                };

                await WriteResponseAsync(
                    context,
                    statusCode,
                    ex.Message);
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50024)
            {
                var statusCode = ex.Number switch
                {
                    50010 or 50011 => HttpStatusCode.Conflict,
                    50012 or 50013 => HttpStatusCode.BadRequest,

                    50020 => HttpStatusCode.NotFound,
                    50021 or 50022 => HttpStatusCode.Conflict,
                    50023 or 50024 => HttpStatusCode.BadRequest,

                    _ => HttpStatusCode.BadRequest
                };

                await WriteResponseAsync(
                    context,
                    statusCode,
                    ex.Message);
            }
            catch (SqlException ex) when (ex.Number == 50030)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
            catch (SqlException ex) when (ex.Number == 50001)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.Conflict,
                    ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error al ejecutar una operación en SQL Server.");

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Ocurrió un error al procesar la operación.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteResponseAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado.");

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Ocurrió un error inesperado.");
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = (int)statusCode,
                message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}