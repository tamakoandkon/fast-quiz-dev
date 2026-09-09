using FastQuiz.Auth.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Auth.UseCase.Validators;

public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("用户名不能为空")
            .Length(3, 20)
            .WithMessage("用户名长度必须在3-20个字符之间")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("用户名只能包含字母、数字和下划线");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("密码不能为空")
            .MinimumLength(6)
            .WithMessage("密码长度至少为6位")
            .MaximumLength(20)
            .WithMessage("密码长度不能超过20个字符");

        When(
            x => !string.IsNullOrWhiteSpace(x.Email),
            () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress()
                    .WithMessage("邮箱格式不正确")
                    .MaximumLength(100)
                    .WithMessage("邮箱长度不能超过100个字符");
            }
        );
    }
}
