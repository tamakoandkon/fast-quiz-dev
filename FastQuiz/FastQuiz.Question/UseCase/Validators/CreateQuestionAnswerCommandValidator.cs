using FastQuiz.Question.UseCase.Command;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Validators;

public class CreateQuestionAnswerCommandValidator : AbstractValidator<CreateQuestionAnswerCommand>
{
    public CreateQuestionAnswerCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("答案内容不能为空")
            .NotNull()
            .WithMessage("答案内容不能为null")
            .MaximumLength(500)
            .WithMessage("答案内容长度不能超过500个字符");

        RuleFor(x => x.IsCorrect).NotNull().WithMessage("IsCorrect不能为null");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("排序值不能为负数")
            .NotNull()
            .WithMessage("排序值不能为null");

        RuleFor(x => x.MatchStrategy)
            .IsInEnum()
            .WithMessage("匹配策略类型无效")
            .NotNull()
            .WithMessage("匹配策略不能为null");
    }
}
