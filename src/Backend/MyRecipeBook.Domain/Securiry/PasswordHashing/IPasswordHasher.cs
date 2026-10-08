namespace MyRecipeBook.Domain.Securiry.PasswordHashing;

public interface IPasswordHasher {
  string HashPassword(string password);
  bool VerifyPassword(string password, string passwordHash);
}
