using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEncaixe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Encaixe",
                table: "Consultas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoEncaixe",
                table: "Consultas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Consultas_EncaixeTemMotivo",
                table: "Consultas",
                sql: "NOT \"Encaixe\" OR \"MotivoEncaixe\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Consultas_EncaixeTemMotivo",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Encaixe",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "MotivoEncaixe",
                table: "Consultas");
        }
    }
}
