namespace EmissorMdfe.Core.Models;

public class ConfiguracaoApp
{
    public int Id { get; set; }
    public string ChaveApiNuvemFiscal { get; set; } = string.Empty;
    public string Ambiente { get; set; } = "homologacao"; // homologacao ou producao
    public string UltimoCnpjEmitente { get; set; } = string.Empty;
}