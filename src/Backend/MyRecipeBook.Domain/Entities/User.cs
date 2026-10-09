namespace MyRecipeBook.Domain.Entities;

public class User {
  public Guid Id { get; set; } = Guid.CreateVersion7();
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;

  public bool Active { get; set; } = true;
  public DateTime? DeletedAt { get; set; } = null;
}
