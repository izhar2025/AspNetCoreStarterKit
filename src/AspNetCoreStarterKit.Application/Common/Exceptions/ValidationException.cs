using FluentValidation.Results;

namespace AspNetCoreStarterKit.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; } = new();

    public ValidationException(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures.Select(f => f.ErrorMessage).ToList();
    }
}