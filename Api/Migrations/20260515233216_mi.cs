using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class mi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Laboratodio_Id",
                table: "Peticiones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaboratoriosId",
                table: "Peticiones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Peticiones_LaboratoriosId",
                table: "Peticiones",
                column: "LaboratoriosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Peticiones_Laboratorios_LaboratoriosId",
                table: "Peticiones",
                column: "LaboratoriosId",
                principalTable: "Laboratorios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peticiones_Laboratorios_LaboratoriosId",
                table: "Peticiones");

            migrationBuilder.DropIndex(
                name: "IX_Peticiones_LaboratoriosId",
                table: "Peticiones");

            migrationBuilder.DropColumn(
                name: "Laboratodio_Id",
                table: "Peticiones");

            migrationBuilder.DropColumn(
                name: "LaboratoriosId",
                table: "Peticiones");
        }
    }
}
