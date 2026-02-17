using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EmissorMdfe.UI.ViewModels;

// Adicionámos a página "Historico"
public enum Page
{
    Dashboard,
    Emissao,
    Veiculos,
    Condutores,
    Historico
}

public partial class MainWindowViewModel : ViewModelBase
{
    // ==========================================
    // NAVEGAÇÃO E ESTADO DO ECRÃ
    // ==========================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboard))]
    [NotifyPropertyChangedFor(nameof(IsWizardVisible))]
    [NotifyPropertyChangedFor(nameof(IsVeiculos))]
    [NotifyPropertyChangedFor(nameof(IsCondutores))]
    [NotifyPropertyChangedFor(nameof(IsHistorico))] // <- Novo
    private Page _paginaAtual = Page.Dashboard;

    public bool IsDashboard => PaginaAtual == Page.Dashboard;
    public bool IsWizardVisible => PaginaAtual == Page.Emissao;
    public bool IsVeiculos => PaginaAtual == Page.Veiculos;
    public bool IsCondutores => PaginaAtual == Page.Condutores;
    public bool IsHistorico => PaginaAtual == Page.Historico; // <- Novo

    // ==========================================
    // CONTROLO DO WIZARD (EMISSÃO)
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

    // ==========================================
    // COMANDOS DE NAVEGAÇÃO
    // ==========================================

    [RelayCommand]
    private void AbrirDashboard() => PaginaAtual = Page.Dashboard;

    [RelayCommand]
    private void AbrirVeiculos() => PaginaAtual = Page.Veiculos;

    [RelayCommand]
    private void AbrirCondutores() => PaginaAtual = Page.Condutores;

    [RelayCommand] // <- Novo
    private void AbrirHistorico() => PaginaAtual = Page.Historico;

    [RelayCommand]
    private void IniciarEmissao()
    {
        PaginaAtual = Page.Emissao;
        PassoAtual = 1;
    }

    [RelayCommand]
    private void Cancelar() => PaginaAtual = Page.Dashboard;

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
}