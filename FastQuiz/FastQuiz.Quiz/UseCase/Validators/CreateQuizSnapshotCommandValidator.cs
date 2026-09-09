using FastQuiz.Quiz.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Validators;

public class CreateQuizSnapshotCommandValidator : AbstractValidator<CreateQuizSnapshotCommand>
{
    public CreateQuizSnapshotCommandValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThanOrEqualTo(1)
            .WithMessage("生成试卷样本数量不能少于1")
            .LessThanOrEqualTo(100)
            .WithMessage("生成试卷样本数量不能多于100");
    }
}
