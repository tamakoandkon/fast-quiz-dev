using FastQuiz.Question.UseCase.Query;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Validators;

public class GetAllQuestionGroupQueryValidator : AbstractValidator<GetAllQuestionGroupQuery>
{
    public GetAllQuestionGroupQueryValidator()
    {
        RuleFor(x => x.Cursor)
            .MaximumLength(128)
            .WithMessage("Cursor长度不能超过128个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Cursor));

        RuleFor(x => x.Count)
            .NotNull()
            .WithMessage("Count不能为null")
            .InclusiveBetween(1, 200)
            .WithMessage("每页数量必须在1-200之间");

        RuleFor(x => x.IsPrevious).NotNull().WithMessage("IsPrevious不能为null");

        RuleFor(x => x.NamePrefix)
            .MaximumLength(200)
            .WithMessage("名称前缀长度不能超过200个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.NamePrefix));
    }
}
