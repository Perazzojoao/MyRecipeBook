using FluentValidation.Results;
using Mapster;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Securiry.PasswordHashing;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using MyRecipeBook.Infrastructure.DataAccess.Repositories;

namespace MyRecipeBook.Application.UseCases.User.Register;

public class RegisterUserAccountUseCase : IRegisterUserAccountUseCase {
  private readonly IPasswordHasher _passwordHasher;
  private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;
  private readonly IUserReadOnlyRepository _userReadOnlyRepository;
  private readonly IUnitOfWork _unitOfWork;

  public RegisterUserAccountUseCase(IPasswordHasher passwordHasher, IUserWriteOnlyRepository userWriteOnlyRepository, IUserReadOnlyRepository userReadOnlyRepository, IUnitOfWork unitOfWork) {
    _passwordHasher = passwordHasher;
    _userWriteOnlyRepository = userWriteOnlyRepository;
    _userReadOnlyRepository = userReadOnlyRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<ResponseRegisterUserJson> Execute(RequestRegisterUserAccountJson request) {
    await ValidateAndThrowOnFailures(request);

    var user = request.Adapt<Domain.Entities.User>();
    user.Password = _passwordHasher.HashPassword(user.Password);
    await _userWriteOnlyRepository.Add(user);
    await _unitOfWork.Commit();

    return new ResponseRegisterUserJson {
      Name = user.Name,
      Tokens = new ResponseTokensJson()
    };
  }

  private async Task ValidateAndThrowOnFailures(RequestRegisterUserAccountJson request) {
    var validator = new RegisterUserAccountValidator();
    var result = validator.Validate(request);
    var emailExists = await _userReadOnlyRepository.ExistActiveUserWithEmail(request.Email);
    if (emailExists)
      result.Errors.Add(new ValidationFailure(string.Empty, ResourceMessagesException.VALIDATION_EMAIL_ALREADY_EXISTS));

    if (!result.IsValid) {
      var errorMessages = result.Errors.Select(error => error.ErrorMessage).ToList();
      throw new ErrorOnValidationException(errorMessages);
    }
  }
}
