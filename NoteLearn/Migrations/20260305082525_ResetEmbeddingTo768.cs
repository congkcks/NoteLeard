using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace NoteLearn.Migrations
{
    /// <inheritdoc />
    public partial class ResetEmbeddingTo768 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "users");

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding",
                table: "content_chunks",
                type: "vector(768)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Vector>(
                name: "embedding",
                table: "content_chunks",
                type: "vector",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(768)");
        }
    }
}
