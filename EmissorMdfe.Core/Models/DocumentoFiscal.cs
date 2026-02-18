namespace EmissorMdfe.Core.Models;

public class DocumentoFiscal
{
    public string Chave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Peso { get; set; }
    public long IbgeDescarga { get; set; }
    public string MunicipioDescarga { get; set; } = string.Empty;
}