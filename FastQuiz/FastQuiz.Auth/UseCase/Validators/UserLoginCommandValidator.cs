using FastQuiz.Auth.UseCase.Commands;
using FluentValidation;

namespace FastQuiz.Auth.UseCase.Validators;

public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("用户名不能为空")
            .MaximumLength(50)
            .WithMessage("用户名长度不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("密码不能为空")
            .MaximumLength(20)
            .WithMessage("密码长度不能超过20个字符");
    }
}
