namespace EmissorMdfe.Core.Models;

public class Veiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
    public string Rntrc { get; set; } = string.Empty;

    // NOVOS CAMPOS OBRIGATÓRIOS DO MDF-E:
    public int TaraKG { get; set; } = 0;
    public int CapacidadeKG { get; set; } = 0;
    public string Renavam { get; set; } = string.Empty;
    public string UfLicenciamento { get; set; } = string.Empty;

    // 01-Truck, 02-Toco, 03-Cavalo Mecânico, etc...
    public int TipoRodado { get; set; } = 01;

    // 00-Não aplicável, 01-Aberta, 02-Fechada/Baú, etc...
    public int TipoCarroceria { get; set; } = 00;
}