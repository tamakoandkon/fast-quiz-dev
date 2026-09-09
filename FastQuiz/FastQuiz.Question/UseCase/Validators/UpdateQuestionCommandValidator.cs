using FastQuiz.Question.UseCase.Command;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Validators;

public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("题目内容不能为空")
            .NotNull()
            .WithMessage("题目内容不能为null")
            .MaximumLength(1000)
            .WithMessage("题目内容长度不能超过1000个字符")
            .MinimumLength(5)
            .WithMessage("题目内容长度至少为5个字符");

        RuleFor(x => x.Level)
            .InclusiveBetween(1, 100)
            .WithMessage("难度等级必须在1-100之间")
            .NotNull()
            .WithMessage("难度等级不能为null");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("题目类型无效")
            .NotNull()
            .WithMessage("题目类型不能为null");

        RuleFor(x => x.Answers)
            .NotNull()
            .WithMessage("答案列表不能为null")
            .NotEmpty()
            .WithMessage("至少需要一个答案")
            .ForEach(answer =>
            {
                answer.SetValidator(new CreateQuestionAnswerCommandValidator());
            });

        RuleFor(x => x.Remark).MaximumLength(500).WithMessage("备注长度不能超过500个字符");
    }
}
