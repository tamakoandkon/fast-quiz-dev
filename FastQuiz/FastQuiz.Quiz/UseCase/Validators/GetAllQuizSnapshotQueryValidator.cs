using FastQuiz.Quiz.UseCase.Queries;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Validators;

public class GetAllQuizSnapshotQueryValidator : AbstractValidator<GetAllQuizSnapshotQuery>
{
    public GetAllQuizSnapshotQueryValidator()
    {
        RuleFor(x => x.Cursor)
            .MaximumLength(128)
            .WithMessage("Cursor长度不能超过128个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Cursor));

        RuleFor(x => x.Count).InclusiveBetween(1, 200).WithMessage("每页数量必须在1-200之间");
    }
}
