using FastQuiz.Quiz.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Validators;

public class UpdateQuizTemplateCommandValidator : AbstractValidator<UpdateQuizTemplateCommand>
{
    public UpdateQuizTemplateCommandValidator()
    {
        // 名称验证
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("名称不能为空")
            .NotNull()
            .WithMessage("名称不能为null")
            .MaximumLength(100)
            .WithMessage("名称长度不能超过100个字符")
            .MinimumLength(2)
            .WithMessage("名称长度至少为2个字符");

        RuleFor(x => x.Description).MaximumLength(500).WithMessage("描述长度不能超过500个字符");

        RuleFor(x => x.Remark).MaximumLength(500).WithMessage("备注长度不能超过500个字符");

        RuleFor(x => x.AttemptLimit)
            .GreaterThanOrEqualTo(1)
            .WithMessage("尝试次数至少为1次")
            .LessThanOrEqualTo(100)
            .WithMessage("尝试次数最多为100次");

        RuleFor(x => x.IsUnlimitedTime).NotNull().WithMessage("IsUnlimitedTime不能为null");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 480)
            .WithMessage("考试时长必须在1-480分钟之间")
            .NotNull()
            .WithMessage("DurationMinutes不能为null");

        // 不限时模式下，时长应该为0或忽略
        When(
            x => !x.IsUnlimitedTime,
            () =>
            {
                RuleFor(x => x.BeginTime)
                    .NotNull()
                    .WithMessage("开始时间不能为null")
                    .GreaterThanOrEqualTo(DateTimeOffset.Now)
                    .WithMessage("开始时间不能早于当前时间");

                // 结束时间验证
                RuleFor(x => x.EndTime)
                    .NotNull()
                    .WithMessage("结束时间不能为null")
                    .GreaterThan(x => x.BeginTime)
                    .WithMessage("结束时间必须晚于开始时间");
            }
        );

        // 整体时间范围验证
        //RuleFor(x => x).Must(x => x.EndTime > x.BeginTime).WithMessage("结束时间必须晚于开始时间");

        // 考试时长不能超过开始到结束的时间范围
        When(
            x => !x.IsUnlimitedTime && x.DurationMinutes > 0,
            () =>
            {
                RuleFor(x => x)
                    .Must(x => x.BeginTime.AddMinutes(x.DurationMinutes) <= x.EndTime)
                    .WithMessage("考试时长不能超过开始时间到结束时间的范围");
            }
        );
    }
}
