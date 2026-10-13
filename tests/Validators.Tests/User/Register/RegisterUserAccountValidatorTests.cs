using System.Diagnostics.CodeAnalysis;
using CommonTestUtilities.Requests;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Exception;
using Shouldly;

namespace Validators.Tests.User.Register;

public class RegisterUserAccountValidatorTests {

  [Fact]
  public void Success() {
    // AAA
    // Arrange
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    var validator = new RegisterUserAccountValidator();

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.ShouldBeTrue();
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("            ")]
  [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "<Pending>")]
  public void Validadte_ShouldHaveError_WhenNameIsEmpty(string name) {
    // AAA
    // Arrange
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    request.Name = name;

    var validator = new RegisterUserAccountValidator();

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldSatisfyAllConditions(erros => {
      erros.Count.ShouldBe(1);
      erros.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_NAME_REQUIRED));
    });
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("            ")]
  [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "<Pending>")]
  public void Validadte_ShouldHaveError_WhenEmailIsEmpty(string email) {
    // AAA
    // Arrange
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    request.Email = email;

    var validator = new RegisterUserAccountValidator();

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldSatisfyAllConditions(erros => {
      erros.Count.ShouldBe(1);
      erros.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_EMAIL_REQUIRED));
    });
  }

  [Fact]
  public void Validadte_ShouldHaveError_WhenPasswordIsEmpty() {
    // AAA
    // Arrange
    var request = RequestRegisterUserAccountJsonBuilder.Build();
    request.Password = string.Empty;

    var validator = new RegisterUserAccountValidator();

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.ShouldBeFalse();
    result.Errors.ShouldSatisfyAllConditions(erros => {
      erros.Count.ShouldBe(1);
      erros.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_PASSWORD_REQUIRED));
    });
  }
}
