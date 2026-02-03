using FluentValidation;
using MediatR;

namespace ATMChallenge.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior que intercepta todas las requests de MediatR y ejecuta validaciones de FluentValidation
    /// antes de que lleguen al handler correspondiente.
    /// Implementa el principio de Separation of Concerns y Single Responsibility.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Si no hay validadores registrados para este request, continuar al siguiente paso del pipeline
            if (!_validators.Any())
            {
                return await next();
            }

            // Crear contexto de validación
            var context = new ValidationContext<TRequest>(request);

            // Ejecutar todas las validaciones en paralelo
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // Recolectar todos los errores
            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            // Si hay errores, lanzar excepción de validación
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }

            // Si todo es válido, continuar al handler
            return await next();
        }
    }
}
