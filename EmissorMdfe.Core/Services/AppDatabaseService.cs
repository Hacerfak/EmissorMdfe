using EmissorMdfe.Core.Data;
using EmissorMdfe.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmissorMdfe.Core.Services;

public class AppDatabaseService
{
    // =====================================
    // VEÍCULOS
    // =====================================
    public async Task<List<Veiculo>> GetVeiculosAsync()
    {
        using var db = new AppDbContext();
        return await db.Veiculos.ToListAsync();
    }

    public async Task SalvarVeiculoAsync(Veiculo veiculo)
    {
        using var db = new AppDbContext();
        if (veiculo.Id == 0)
            db.Veiculos.Add(veiculo); // Novo
        else
            db.Veiculos.Update(veiculo); // Edição

        await db.SaveChangesAsync();
    }

    public async Task ExcluirVeiculoAsync(Veiculo veiculo)
    {
        using var db = new AppDbContext();
        db.Veiculos.Remove(veiculo);
        await db.SaveChangesAsync();
    }

    // =====================================
    // CONDUTORES
    // =====================================
    public async Task<List<Condutor>> GetCondutoresAsync()
    {
        using var db = new AppDbContext();
        return await db.Condutores.ToListAsync();
    }

    public async Task SalvarCondutorAsync(Condutor condutor)
    {
        using var db = new AppDbContext();
        if (condutor.Id == 0)
            db.Condutores.Add(condutor);
        else
            db.Condutores.Update(condutor);

        await db.SaveChangesAsync();
    }

    public async Task ExcluirCondutorAsync(Condutor condutor)
    {
        using var db = new AppDbContext();
        db.Condutores.Remove(condutor);
        await db.SaveChangesAsync();
    }

    // =====================================
    // CONFIGURAÇÕES GERAIS E EMITENTE
    // =====================================
    public async Task<ConfiguracaoApp?> GetConfiguracaoAsync()
    {
        using var db = new AppDbContext();
        return await db.Configuracoes.FirstOrDefaultAsync();
    }

    public async Task SalvarConfiguracaoAsync(ConfiguracaoApp config)
    {
        using var db = new AppDbContext();
        if (config.Id == 0)
            db.Configuracoes.Add(config);
        else
            db.Configuracoes.Update(config);

        await db.SaveChangesAsync();
    }
}