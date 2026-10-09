using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;

internal sealed class UsersRepository(MyRecipeBookDbContext dbContext) : IUserWriteOnlyRepository, IUserReadOnlyRepository {
  private readonly MyRecipeBookDbContext _dbContext = dbContext;

  public async Task Add(User user) => await _dbContext.Users.AddAsync(user);

  public async Task<bool> ExistActiveUserWithEmail(string email) => await _dbContext.Users.AnyAsync(user => user.Active && user.Email.Equals(email));
}
