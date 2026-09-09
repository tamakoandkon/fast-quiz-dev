using FastQuiz.Quiz.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Validators;

public class UpdateQuestionSelectionRuleCommandValidator
    : AbstractValidator<UpdateQuestionSelectionRuleCommand>
{
    public UpdateQuestionSelectionRuleCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("GroupId不能为空")
            .NotNull()
            .WithMessage("GroupId不能为null")
            .MaximumLength(128)
            .WithMessage("GroupId长度不能超过128个字符");

        RuleFor(x => x.QuestionType).IsInEnum().WithMessage("题目类型无效");

        // Level 验证
        RuleFor(x => x.Level)
            .InclusiveBetween(0, 5)
            .WithMessage("难度等级必须在0-5之间，0表示全部");

        // Count 验证（抽取数量）
        RuleFor(x => x.Count).GreaterThanOrEqualTo(1).WithMessage("抽取数量必须大于1");

        // Score 验证（每题分数）
        RuleFor(x => x.Score).GreaterThanOrEqualTo(1).WithMessage("每题分数必须大于1");
    }
}
