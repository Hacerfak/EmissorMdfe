using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmissorMdfe.Core.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaConfiguracaoEmitente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UltimoCnpjEmitente",
                table: "Configuracoes",
                newName: "UfEmitente");

            migrationBuilder.RenameColumn(
                name: "ChaveApiNuvemFiscal",
                table: "Configuracoes",
                newName: "SenhaCertificado");

            migrationBuilder.AddColumn<string>(
                name: "BairroEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CaminhoCertificado",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CepEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CidadeEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CnpjEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IeEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogradouroEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocialEmitente",
                table: "Configuracoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BairroEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "CaminhoCertificado",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "CepEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "CidadeEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "CnpjEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "IeEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "LogradouroEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "NumeroEmitente",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "RazaoSocialEmitente",
                table: "Configuracoes");

            migrationBuilder.RenameColumn(
                name: "UfEmitente",
                table: "Configuracoes",
                newName: "UltimoCnpjEmitente");

            migrationBuilder.RenameColumn(
                name: "SenhaCertificado",
                table: "Configuracoes",
                newName: "ChaveApiNuvemFiscal");
        }
    }
}
