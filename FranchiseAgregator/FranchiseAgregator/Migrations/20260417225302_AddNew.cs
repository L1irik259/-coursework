using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FranchiseAgregator.Migrations
{
    /// <inheritdoc />
    public partial class AddNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActivate",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "News",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "News",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NewsStatusId",
                table: "News",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NewsStatuses",
                columns: table => new
                {
                    NewsStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsStatuses", x => x.NewsStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_News_NewsStatusId",
                table: "News",
                column: "NewsStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_News_NewsStatuses_NewsStatusId",
                table: "News",
                column: "NewsStatusId",
                principalTable: "NewsStatuses",
                principalColumn: "NewsStatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_NewsStatuses_NewsStatusId",
                table: "News");

            migrationBuilder.DropTable(
                name: "NewsStatuses");

            migrationBuilder.DropIndex(
                name: "IX_News_NewsStatusId",
                table: "News");

            migrationBuilder.DropColumn(
                name: "NewsStatusId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "News",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivate",
                table: "News",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
