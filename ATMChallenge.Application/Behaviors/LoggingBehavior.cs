using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ATMChallenge.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior que registra información de performance y logs de todas las requests de MediatR.
    /// Implementa Cross-Cutting Concerns y el principio de Open/Closed.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogInformation(
                "Ejecutando {RequestName} - {@Request}",
                requestName,
                request
            );

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "Completado {RequestName} en {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds
                );

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "Error ejecutando {RequestName} después de {ElapsedMilliseconds}ms - Error: {ErrorMessage}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message
                );

                throw;
            }
        }
    }
}
