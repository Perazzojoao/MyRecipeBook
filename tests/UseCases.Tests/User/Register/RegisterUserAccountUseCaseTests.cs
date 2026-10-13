using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using Shouldly;
using Xunit.Abstractions;

namespace UseCases.Tests.User.Register;

public class RegisterUserAccountUseCaseTests {
  [Fact]
  public async Task Success() {
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    var useCase = CreateUseCase();

    var result = await useCase.Execute(request);

    result.ShouldNotBeNull();
    result.Tokens.ShouldNotBeNull();
    result.Name.ShouldBe(request.Name);
    result.Tokens.AccessToken.ShouldBeNullOrEmpty();
  }

  [Fact]
  public async Task Validate_ShouldThrowException_WhenNameIsEmpty() {
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    request.Name = string.Empty;

    var useCase = CreateUseCase();

    var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();
    exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages => {
      errorMessages.Count.ShouldBe(1);
      errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_NAME_REQUIRED);
    });
  }

  [Fact]
  public async Task Validate_ShouldThrowException_WhenEmailAlreadyExists() {
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    var useCase = CreateUseCase(request.Email);

    var exception = await useCase.Execute(request).ShouldThrowAsync<ErrorOnValidationException>();
    exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages => {
      errorMessages.Count.ShouldBe(1);
      errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_EMAIL_ALREADY_EXISTS);
    });
  }

  private RegisterUserAccountUseCase CreateUseCase(string? emailThatAlreadyExists = null) {
    var passwordHash = new IPasswordHasherBuilder().Build();
    var userWriteOnlyRepository = IUserWriteOnlyRepositoryBuilder.Build();
    var unitOfWork = IUnitOfWorkBuilder.Build();
    var userReadOnlyRepositoryBuilder = new IUserReadOnlyRepositoryBuilder();
    if (emailThatAlreadyExists.IsNotEmpty())
      userReadOnlyRepositoryBuilder.ExistActiveUserWithEmail(emailThatAlreadyExists);

    return new RegisterUserAccountUseCase(passwordHash, userWriteOnlyRepository, userReadOnlyRepositoryBuilder.Build(), unitOfWork);
  }
}
