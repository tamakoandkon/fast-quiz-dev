using FastQuiz.Question.UseCase.Command;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Validators;

public class CreateQuestionGroupCommandValidator : AbstractValidator<CreateQuestionGroupCommand>
{
    public CreateQuestionGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("题目组名称不能为空")
            .MaximumLength(100)
            .WithMessage("题目组名称长度不能超过100个字符");

        RuleFor(x => x.Remark).MaximumLength(500).WithMessage("备注长度不能超过500个字符");
    }
}
