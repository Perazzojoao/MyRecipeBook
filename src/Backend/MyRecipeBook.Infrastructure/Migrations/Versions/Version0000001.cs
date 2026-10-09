using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_USERS, "Creating Users table")]
public class Version0000001 : ForwardOnlyMigration {
  public override void Up() {
    Create.Table("Users")
      .WithColumn("Id").AsGuid().PrimaryKey().NotNullable()
      .WithColumn("Name").AsString(250).NotNullable()
      .WithColumn("Email").AsString(250).NotNullable().Unique()
      .WithColumn("Password").AsString(2000).NotNullable()
      .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
      .WithColumn("DeletedAt").AsDateTime().Nullable().WithDefaultValue(null);
  }
}
