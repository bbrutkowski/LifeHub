using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LifeHub.Infrastructure.Persistence;

#nullable disable

namespace LifeHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260927120000_AddUserProfileAndActivation")]
public partial class AddUserProfileAndActivation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AvatarUrl", table: "Users", type: "nvarchar(512)", maxLength: 512, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "Timezone", table: "Users", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "City", table: "Users", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "Currency", table: "Users", type: "nvarchar(3)", maxLength: 3, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "DateFormat", table: "Users", type: "nvarchar(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "WeekStartsOn", table: "Users", type: "nvarchar(10)", maxLength: 10, nullable: true);
        migrationBuilder.AddColumn<bool>(
            name: "IsActive", table: "Users", type: "bit", nullable: false, defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AvatarUrl", table: "Users");
        migrationBuilder.DropColumn(name: "Timezone", table: "Users");
        migrationBuilder.DropColumn(name: "City", table: "Users");
        migrationBuilder.DropColumn(name: "Currency", table: "Users");
        migrationBuilder.DropColumn(name: "DateFormat", table: "Users");
        migrationBuilder.DropColumn(name: "WeekStartsOn", table: "Users");
        migrationBuilder.DropColumn(name: "IsActive", table: "Users");
    }
}
