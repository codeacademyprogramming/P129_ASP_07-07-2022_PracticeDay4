using Microsoft.EntityFrameworkCore.Migrations;

namespace P129Allup.Migrations
{
    public partial class UpdatedBasketsTablerelatedUserTabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Baskets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_AppUserId",
                table: "Baskets",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Baskets_AspNetUsers_AppUserId",
                table: "Baskets",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Baskets_AspNetUsers_AppUserId",
                table: "Baskets");

            migrationBuilder.DropIndex(
                name: "IX_Baskets_AppUserId",
                table: "Baskets");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Baskets");
        }
    }
}
