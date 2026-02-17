using EmissorMdfe.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace EmissorMdfe.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<ConfiguracaoApp> Configuracoes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<Condutor> Condutores { get; set; }

    public string DbPath { get; }

    public AppDbContext()
    {
        // Procura a pasta segura de dados do utilizador (AppData no Windows / .local no Linux)
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        // Cria uma pasta com o nome do nosso sistema
        var appFolder = Path.Join(path, "EmissorMdfeCore");
        Directory.CreateDirectory(appFolder);

        // Define o caminho final da base de dados
        DbPath = Path.Join(appFolder, "emissor_mdfe.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}