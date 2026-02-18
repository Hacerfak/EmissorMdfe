using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmissorMdfe.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMocksAdicionaCamposReais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Veiculos",
                newName: "UfLicenciamento");

            migrationBuilder.RenameColumn(
                name: "Rntrc",
                table: "Veiculos",
                newName: "Renavam");

            migrationBuilder.AddColumn<int>(
                name: "CapacidadeKG",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Veiculos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TaraKG",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoCarroceria",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoRodado",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "CodigoIbgeCidade",
                table: "Configuracoes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacidadeKG",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "TaraKG",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "TipoCarroceria",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "TipoRodado",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "CodigoIbgeCidade",
                table: "Configuracoes");

            migrationBuilder.RenameColumn(
                name: "UfLicenciamento",
                table: "Veiculos",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "Renavam",
                table: "Veiculos",
                newName: "Rntrc");
        }
    }
}
