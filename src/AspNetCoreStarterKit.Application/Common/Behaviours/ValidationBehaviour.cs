// AspNetCoreStarterKit.Application/Common/Behaviours/ValidationBehaviour.cs
using FluentValidation;
using MediatR;
using ValidationException = AspNetCoreStarterKit.Application.Common.Exceptions.ValidationException;  // ← Add alias

namespace AspNetCoreStarterKit.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);  

        return await next();
    }
}