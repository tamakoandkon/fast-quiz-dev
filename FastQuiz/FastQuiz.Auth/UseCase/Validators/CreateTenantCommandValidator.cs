using FastQuiz.Auth.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Auth.UseCase.Validators;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("租户名称不能为空")
            .MaximumLength(50)
            .WithMessage("租户名称长度不能超过50个字符");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("租户详情长度不能超过200个字符");

        RuleFor(x => x.AdminUsername)
            .NotEmpty()
            .WithMessage("管理员用户名不能为空")
            .Length(3, 20)
            .WithMessage("管理员用户名长度必须在3-20个字符之间")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("管理员用户名只能包含字母、数字和下划线");

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .WithMessage("管理员密码不能为空")
            .MinimumLength(6)
            .WithMessage("管理员密码长度至少为6位")
            .MaximumLength(20)
            .WithMessage("管理员密码长度不能超过20个字符");

        When(
            x => !string.IsNullOrWhiteSpace(x.AdminEmail),
            () =>
            {
                RuleFor(x => x.AdminEmail)
                    .EmailAddress()
                    .WithMessage("管理员邮箱格式不正确")
                    .MaximumLength(100)
                    .WithMessage("管理员邮箱长度不能超过100个字符");
            }
        );
    }
}
