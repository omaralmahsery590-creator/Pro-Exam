using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pro_exam.Migrations
{
    /// <inheritdoc />
    public partial class DoctorCourseM2M : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Doctors_DoctorId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DoctorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "DoctorCourses",
                columns: table => new
                {
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorCourses", x => new { x.DoctorId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_DoctorCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorCourses_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorCourses_CourseId",
                table: "DoctorCourses",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorCourses");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DoctorId",
                table: "Courses",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Doctors_DoctorId",
                table: "Courses",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
