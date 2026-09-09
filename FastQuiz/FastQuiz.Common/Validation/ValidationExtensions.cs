using FastQuiz.Common.Exceptions;
using FluentValidation;

namespace FastQuiz.Common.Validation;

public static class ValidationExtensions
{
    public static void ValidateAndThrowArgumentException<T>(
        this IValidator<T> validator,
        T instance
    )
    {
        var result = validator.Validate(instance);

        if (!result.IsValid)
        {
            var ex = new ValidationException(result.Errors);
            throw new EntityArgumentException(ex.Message, ex);
        }
    }
}
