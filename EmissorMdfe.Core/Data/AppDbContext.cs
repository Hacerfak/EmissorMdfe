using EmissorMdfe.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EmissorMdfe.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<ConfiguracaoApp> Configuracoes { get; set; }
    // No futuro adicionamos: public DbSet<Veiculo> Veiculos { get; set; }

    public string DbPath { get; }

    public AppDbContext()
    {
        // Guarda a base de dados na pasta local da aplicação
        DbPath = Path.Join(System.AppDomain.CurrentDomain.BaseDirectory, "emissor_mdfe.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}