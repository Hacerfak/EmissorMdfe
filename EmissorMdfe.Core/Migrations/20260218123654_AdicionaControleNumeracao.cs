using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmissorMdfe.Core.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaControleNumeracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Ambiente",
                table: "Configuracoes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "TipoEmitente",
                table: "Configuracoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UltimoNumeroMdfe",
                table: "Configuracoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "UltimoNumeroMdfe",
                table: "Configuracoes");

            migrationBuilder.AlterColumn<string>(
                name: "Ambiente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
