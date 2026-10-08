using FluentValidation;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.User.Register;

public class RegisterUserAccountValidator : AbstractValidator<RequestRegisterUserAccountJson> {
  public RegisterUserAccountValidator() {
    RuleFor(user => user.Name)
      .NotEmpty().WithMessage(ResourceMessagesException.NAME_REQUIRED)
      .MaximumLength(100).WithMessage(ResourceMessagesException.NAME_TOO_LONG);

    RuleFor(user => user.Email)
      .NotEmpty().WithMessage(ResourceMessagesException.EMAIL_REQUIRED);

    RuleFor(user => user.Password)
      .NotEmpty().WithMessage(ResourceMessagesException.PASSWORD_REQUIRED)
      .MinimumLength(6).WithMessage(ResourceMessagesException.PASSWORD_TOO_SHORT);

    When(user => user.Email.IsNotEmpty(), () => {
      RuleFor(user => user.Email)
        .EmailAddress().WithMessage(ResourceMessagesException.EMAIL_INVALID);
    });
  }

}
