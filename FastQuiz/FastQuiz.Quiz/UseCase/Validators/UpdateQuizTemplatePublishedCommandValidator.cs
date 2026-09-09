using FastQuiz.Quiz.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Validators;

public class UpdateQuizTemplatePublishedCommandValidator
    : AbstractValidator<UpdateQuizTemplatePublishedCommand>
{
    public UpdateQuizTemplatePublishedCommandValidator()
    {
        RuleFor(x => x.IsPublished).NotNull().WithMessage("发布状态不可为空");
    }
}
