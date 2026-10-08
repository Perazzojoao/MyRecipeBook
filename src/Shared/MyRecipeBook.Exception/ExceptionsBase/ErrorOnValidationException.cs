namespace MyRecipeBook.Exception.ExceptionsBase;

public class ErrorOnValidationException : MyRecipeBookExcetion {
  private readonly List<string> _errors;

  public ErrorOnValidationException(List<string> errorMessages) {
    _errors = errorMessages;
  }

  public List<string> GetErrorMessages() => _errors;
}
