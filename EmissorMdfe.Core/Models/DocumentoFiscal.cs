namespace EmissorMdfe.Core.Models;

public class DocumentoFiscal
{
    public string Chave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Peso { get; set; }

    // DADOS DE DESCARGA (DESTINO)
    public long IbgeDescarga { get; set; }
    public string MunicipioDescarga { get; set; } = string.Empty;
    public string UfDescarga { get; set; } = string.Empty; // <--- NOVO

    // DADOS DE CARREGAMENTO (ORIGEM - EMITENTE DA NFE)
    public long IbgeCarregamento { get; set; }  // <--- NOVO
    public string MunicipioCarregamento { get; set; } = string.Empty; // <--- NOVO
    public string UfCarregamento { get; set; } = string.Empty; // <--- NOVO
}