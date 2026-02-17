using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmissorMdfe.Core.Data;
using EmissorMdfe.Core.Models;
using EmissorMdfe.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmissorMdfe.UI.ViewModels;

public enum Page
{
    Dashboard, Emissao, Veiculos, Condutores, Historico
}

public partial class MainWindowViewModel : ViewModelBase
{
    // Serviço do Banco de Dados
    private readonly AppDatabaseService _dbService;

    // ==========================================
    // LISTAS DE DADOS (BINDING PARA A UI)
    // ==========================================

    // ==========================================
    // MODAIS (FORMULÁRIOS DE CADASTRO)
    // ==========================================

    [ObservableProperty] private bool _isModalVeiculoAberto;
    [ObservableProperty] private Veiculo _novoVeiculo = new();

    [ObservableProperty] private bool _isModalCondutorAberto;
    [ObservableProperty] private Condutor _novoCondutor = new();

    // -- Comandos Veículo --
    [RelayCommand]
    private void AbrirModalVeiculo()
    {
        NovoVeiculo = new Veiculo { Tipo = "Veículo de Tração" }; // Limpa o formulário e define o padrão
        IsModalVeiculoAberto = true;
    }

    [RelayCommand] private void FecharModalVeiculo() => IsModalVeiculoAberto = false;

    [RelayCommand]
    private async Task SalvarVeiculoAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoVeiculo.Placa)) return; // Validação simples

        await _dbService.SalvarVeiculoAsync(NovoVeiculo);
        Veiculos.Add(NovoVeiculo); // Atualiza a lista na UI instantaneamente

        IsModalVeiculoAberto = false; // Fecha o modal
    }

    // -- Comandos Condutor --
    [RelayCommand]
    private void AbrirModalCondutor()
    {
        NovoCondutor = new Condutor(); // Limpa o formulário
        IsModalCondutorAberto = true;
    }

    [RelayCommand] private void FecharModalCondutor() => IsModalCondutorAberto = false;

    [RelayCommand]
    private async Task SalvarCondutorAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoCondutor.Nome)) return;

        await _dbService.SalvarCondutorAsync(NovoCondutor);
        Condutores.Add(NovoCondutor);

        IsModalCondutorAberto = false;
    }

    [ObservableProperty]
    private ObservableCollection<Veiculo> _veiculos = new();

    [ObservableProperty]
    private ObservableCollection<Condutor> _condutores = new();

    // Construtor
    public MainWindowViewModel()
    {
        _dbService = new AppDatabaseService();

        // FORÇA A CRIAÇÃO DO BANCO E DAS TABELAS ANTES DE FAZER QUALQUER COISA
        using (var db = new AppDbContext())
        {
            db.Database.Migrate();
        }

        // Carrega os dados do banco assim que o App abre
        _ = CarregarDadosAsync();
    }

    private async Task CarregarDadosAsync()
    {
        // Busca do SQLite
        var listaVeiculos = await _dbService.GetVeiculosAsync();
        var listaCondutores = await _dbService.GetCondutoresAsync();

        // Atualiza a interface
        Veiculos = new ObservableCollection<Veiculo>(listaVeiculos);
        Condutores = new ObservableCollection<Condutor>(listaCondutores);
    }

    // ==========================================
    // NAVEGAÇÃO E ESTADO DA TELA
    // ==========================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboard))]
    [NotifyPropertyChangedFor(nameof(IsWizardVisible))]
    [NotifyPropertyChangedFor(nameof(IsVeiculos))]
    [NotifyPropertyChangedFor(nameof(IsCondutores))]
    [NotifyPropertyChangedFor(nameof(IsHistorico))]
    private Page _paginaAtual = Page.Dashboard;

    public bool IsDashboard => PaginaAtual == Page.Dashboard;
    public bool IsWizardVisible => PaginaAtual == Page.Emissao;
    public bool IsVeiculos => PaginaAtual == Page.Veiculos;
    public bool IsCondutores => PaginaAtual == Page.Condutores;
    public bool IsHistorico => PaginaAtual == Page.Historico;

    // ==========================================
    // CONTROLE DO WIZARD (EMISSÃO)
    // ==========================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasso1))]
    [NotifyPropertyChangedFor(nameof(IsPasso2))]
    [NotifyPropertyChangedFor(nameof(IsPasso3))]
    [NotifyPropertyChangedFor(nameof(IsPasso4))]
    private int _passoAtual = 1;

    public bool IsPasso1 => PassoAtual == 1;
    public bool IsPasso2 => PassoAtual == 2;
    public bool IsPasso3 => PassoAtual == 3;
    public bool IsPasso4 => PassoAtual == 4;

    [ObservableProperty] private Veiculo? _veiculoSelecionado;
    [ObservableProperty] private Condutor? _condutorSelecionado;

    // ==========================================
    // COMANDOS DE NAVEGAÇÃO
    // ==========================================

    [RelayCommand] private void AbrirDashboard() => PaginaAtual = Page.Dashboard;
    [RelayCommand] private void AbrirVeiculos() => PaginaAtual = Page.Veiculos;
    [RelayCommand] private void AbrirCondutores() => PaginaAtual = Page.Condutores;
    [RelayCommand] private void AbrirHistorico() => PaginaAtual = Page.Historico;

    [RelayCommand]
    private void IniciarEmissao()
    {
        PaginaAtual = Page.Emissao;
        PassoAtual = 1;
        VeiculoSelecionado = null;  // Limpa a seleção anterior
        CondutorSelecionado = null;
    }

    [RelayCommand] private void Cancelar() => PaginaAtual = Page.Dashboard;

    // ==========================================
    // COMANDOS DO WIZARD
    // ==========================================

    [RelayCommand]
    private void Avancar()
    {
        if (PassoAtual < 4) PassoAtual++;
    }

    [RelayCommand]
    private void Voltar()
    {
        if (PassoAtual > 1) PassoAtual--;
    }

    // ==========================================
    // ATALHOS GLOBAIS DE TECLADO
    // ==========================================
    [RelayCommand]
    private void GlobalEscape()
    {
        // 1º Prioridade: Se houver um modal aberto, fecha o modal.
        if (IsModalVeiculoAberto)
        {
            IsModalVeiculoAberto = false;
        }
        else if (IsModalCondutorAberto)
        {
            IsModalCondutorAberto = false;
        }
        // 2º Prioridade: Se não houver modal, mas estivermos na tela de Emissão, cancela e volta pro Dashboard.
        else if (PaginaAtual == Page.Emissao)
        {
            PaginaAtual = Page.Dashboard;
        }
    }
}