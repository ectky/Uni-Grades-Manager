using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialInstructorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
    table: "Users",
    columns: ["Id", "Email", "Name", "Password", "StudentId", "Type"],
    values: [1, "instructor@uni.bg", "Instructor", "pnSu5Xs8V0Fv2LcxL+lgwwtTE2S7lcws4RzeWdpny4clU2CD3/r7uleWt+5tT0dX", null, 1]);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
