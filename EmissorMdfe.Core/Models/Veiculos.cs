namespace EmissorMdfe.Core.Models;

public class Veiculo
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Rntrc { get; set; } = string.Empty;

    // Para simplificar, vamos usar uma string ("Tração" ou "Reboque")
    public string Tipo { get; set; } = "Tração";
}