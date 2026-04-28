using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pro_exam.Migrations
{
    /// <inheritdoc />
    public partial class AddExamExtraRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamExtraRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamExtraRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamExtraRooms_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamExtraRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamExtraRooms_ExamId",
                table: "ExamExtraRooms",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamExtraRooms_RoomId",
                table: "ExamExtraRooms",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamExtraRooms");
        }
    }
}
