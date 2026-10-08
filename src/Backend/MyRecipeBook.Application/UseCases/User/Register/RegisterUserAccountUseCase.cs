using Mapster;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Securiry.PasswordHashing;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.User.Register;

public class RegisterUserAccountUseCase : IRegisterUserAccountUseCase {
  private readonly IPasswordHasher _passwordHasher;

  public RegisterUserAccountUseCase(IPasswordHasher passwordHasher) {
    _passwordHasher = passwordHasher;
  }

  public void Execute(RequestRegisterUserAccountJson request) {
    ValidateAndThrowOnFailures(request);

    var user = request.Adapt<Domain.Entities.User>();
    user.Password = _passwordHasher.HashPassword(user.Password);
  }

  private void ValidateAndThrowOnFailures(RequestRegisterUserAccountJson request) {
    var validator = new RegisterUserAccountValidator();
    var result = validator.Validate(request);
    if (!result.IsValid) {
      var errorMessages = result.Errors.Select(error => error.ErrorMessage).ToList();
      throw new ErrorOnValidationException(errorMessages);
    }
  }
}
