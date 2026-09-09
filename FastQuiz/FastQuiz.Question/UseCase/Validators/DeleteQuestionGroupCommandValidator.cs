using FastQuiz.Question.UseCase.Command;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Validators;

public class DeleteQuestionGroupCommandValidator : AbstractValidator<DeleteQuestionGroupCommand>
{
    public DeleteQuestionGroupCommandValidator()
    {
        RuleFor(x => x.GroupIds)
            .NotNull()
            .WithMessage("GroupIds不能为null")
            .NotEmpty()
            .WithMessage("GroupIds不能为空集合")
            .Must(ids => ids.Count <= 500)
            .WithMessage("单次删除最多500个题目组")
            .ChildRules(ids =>
            {
                ids.RuleForEach(x => x)
                    .NotEmpty()
                    .WithMessage("GroupId不能为空")
                    .MaximumLength(128)
                    .WithMessage("GroupId长度不能超过128个字符");
            });
    }
}
