using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FranchiseAgregator.Migrations
{
    /// <inheritdoc />
    public partial class _123123123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Feedbacks");

            migrationBuilder.AddColumn<int>(
                name: "FeedbackStatusId",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FeedbackStatus",
                columns: table => new
                {
                    FeedbackStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackStatus", x => x.FeedbackStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_FeedbackStatusId",
                table: "Feedbacks",
                column: "FeedbackStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_FeedbackStatus_FeedbackStatusId",
                table: "Feedbacks",
                column: "FeedbackStatusId",
                principalTable: "FeedbackStatus",
                principalColumn: "FeedbackStatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_FeedbackStatus_FeedbackStatusId",
                table: "Feedbacks");

            migrationBuilder.DropTable(
                name: "FeedbackStatus");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_FeedbackStatusId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "FeedbackStatusId",
                table: "Feedbacks");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Feedbacks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
