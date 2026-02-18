namespace EmissorMdfe.Core.Models;

public class ConfiguracaoApp
{
    public int Id { get; set; }

    // Configurações do Sistema
    public string Ambiente { get; set; } = "Homologacao"; // Homologacao ou Producao

    // Certificado Digital
    public string CaminhoCertificado { get; set; } = string.Empty;
    public string SenhaCertificado { get; set; } = string.Empty;

    // Dados do Emitente (Que virão automaticamente da SEFAZ)
    public string CnpjEmitente { get; set; } = string.Empty;
    public string IeEmitente { get; set; } = string.Empty;
    public string RazaoSocialEmitente { get; set; } = string.Empty;
    public string UfEmitente { get; set; } = string.Empty;
    public string CidadeEmitente { get; set; } = string.Empty;
    public string LogradouroEmitente { get; set; } = string.Empty;
    public string NumeroEmitente { get; set; } = string.Empty;
    public string BairroEmitente { get; set; } = string.Empty;
    public string CepEmitente { get; set; } = string.Empty;
}