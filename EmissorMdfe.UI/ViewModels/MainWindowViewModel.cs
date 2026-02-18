using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmissorMdfe.Core.Data;
using EmissorMdfe.Core.Models;
using EmissorMdfe.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System;
using System.IO;
using NFe.Servicos;
using DFe.Classes.Flags;
using DFe.Classes.Entidades;
using NFe.Classes.Servicos.ConsultaCadastro;
using NFe.Utils;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using DFe.Utils;
using MDFe.Classes.Extensoes;
using MDFe.Utils.Configuracoes;
using System.Xml.Linq;
using System.Globalization;
using System.Linq;

namespace EmissorMdfe.UI.ViewModels;

public enum Page
{
    Dashboard, Emissao, Veiculos, Condutores, Historico, Configuracoes
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

    // Controle do expansor de campos avançados
    [ObservableProperty] private bool _isVeiculoAvancadoAberto;

    // Listas oficiais da SEFAZ
    public ObservableCollection<string> ListaTiposRodado { get; } = new()
    {
        "01 - Truck", "02 - Toco", "03 - Cavalo Mecânico", "04 - VAN", "05 - Utilitário", "06 - Outros"
    };

    public ObservableCollection<string> ListaTiposCarroceria { get; } = new()
    {
        "00 - Não aplicável", "01 - Aberta", "02 - Fechada/Baú", "03 - Granelera", "04 - Porta Container", "05 - Sider"
    };

    // Índices selecionados nos ComboBoxes
    [ObservableProperty] private int _rodadoSelecionadoIndex = 2; // Padrão: Cavalo Mecânico (índice 2 = código 3)
    [ObservableProperty] private int _carroceriaSelecionadaIndex = 2; // Padrão: Fechada (índice 2 = código 2)

    [ObservableProperty] private bool _isModalCondutorAberto;
    [ObservableProperty] private Condutor _novoCondutor = new();

    // -- Comandos Veículo --
    [RelayCommand]
    private void AbrirModalVeiculo()
    {
        NovoVeiculo = new Veiculo();
        IsVeiculoAvancadoAberto = false; // Garante que o painel avançado começa fechado
        RodadoSelecionadoIndex = 2; // Reseta para Cavalo Mecânico
        CarroceriaSelecionadaIndex = 2; // Reseta para Baú
        IsModalVeiculoAberto = true;
    }

    [RelayCommand] private void FecharModalVeiculo() => IsModalVeiculoAberto = false;

    [RelayCommand]
    private void FecharModais()
    {
        IsModalVeiculoAberto = false;
        IsModalCondutorAberto = false;
    }

    [RelayCommand]
    private async Task SalvarVeiculoAsync()
    {
        if (NovoVeiculo != null && !string.IsNullOrWhiteSpace(NovoVeiculo.Placa))
        {
            // O Rodado na Sefaz começa no 1 (por isso Somamos 1 ao Index que começa no 0)
            NovoVeiculo.TipoRodado = RodadoSelecionadoIndex + 1;

            // A Carroceria na Sefaz começa no 0, então o Index já é o código exato!
            NovoVeiculo.TipoCarroceria = CarroceriaSelecionadaIndex;

            // Se o usuário não preencheu UF, pegamos a UF da Empresa como padrão
            if (string.IsNullOrEmpty(NovoVeiculo.UfLicenciamento))
                NovoVeiculo.UfLicenciamento = UfSelecionada;

            await _dbService.SalvarVeiculoAsync(NovoVeiculo);
            Veiculos.Add(NovoVeiculo);
            IsModalVeiculoAberto = false;
        }
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
        if (NovoCondutor != null && !string.IsNullOrWhiteSpace(NovoCondutor.Nome))
        {
            await _dbService.SalvarCondutorAsync(NovoCondutor);
            Condutores.Add(NovoCondutor);
            IsModalCondutorAberto = false;
        }
    }

    [RelayCommand]
    private async Task SalvarConfiguracoesAsync()
    {
        try
        {
            // 1. FAZER UPLOAD (CÓPIA) DO CERTIFICADO PARA A PASTA SEGURA
            if (!string.IsNullOrEmpty(CaminhoCertificado) && File.Exists(CaminhoCertificado))
            {
                // Encontra a nossa pasta do SQLite
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                var appFolder = Path.Join(path, "EmissorMdfeCore", "Certificados");

                // Cria a pasta "Certificados" se ela ainda não existir
                Directory.CreateDirectory(appFolder);

                // Pega apenas o nome do arquivo (ex: "meucertificado.pfx")
                var fileName = Path.GetFileName(CaminhoCertificado);
                var destinationPath = Path.Join(appFolder, fileName);

                // Copia apenas se o arquivo de origem for diferente do destino
                if (CaminhoCertificado != destinationPath)
                {
                    File.Copy(CaminhoCertificado, destinationPath, overwrite: true);
                    CaminhoCertificado = destinationPath; // Atualiza a variável para o novo caminho interno!
                }
            }

            // 2. PREENCHER O MODELO PARA SALVAR NO BANCO
            _configAtual.CaminhoCertificado = CaminhoCertificado;
            _configAtual.SenhaCertificado = SenhaCertificado;
            _configAtual.UfEmitente = UfSelecionada;
            _configAtual.CnpjEmitente = CnpjEmitente;
            _configAtual.RazaoSocialEmitente = RazaoSocialEmitente;
            _configAtual.IeEmitente = IeEmitente;
            _configAtual.LogradouroEmitente = LogradouroEmitente;
            _configAtual.NumeroEmitente = NumeroEmitente;
            _configAtual.BairroEmitente = BairroEmitente;
            _configAtual.CepEmitente = CepEmitente;
            _configAtual.CidadeEmitente = CidadeEmitente;
            _configAtual.CodigoIbgeCidade = IbgeEmitente;

            // 3. ENVIAR PARA O SQLITE
            await _dbService.SalvarConfiguracaoAsync(_configAtual);

            Console.WriteLine("Configurações Salvas com sucesso!");
            PaginaAtual = Page.Dashboard; // Volta para o Dashboard
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar configurações: {ex.Message}");
        }
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

    private ConfiguracaoApp _configAtual = new();

    private async Task CarregarDadosAsync()
    {
        // Busca do SQLite
        var listaVeiculos = await _dbService.GetVeiculosAsync();
        var listaCondutores = await _dbService.GetCondutoresAsync();
        var config = await _dbService.GetConfiguracaoAsync(); // NOVO

        // Atualiza a interface
        Veiculos = new ObservableCollection<Veiculo>(listaVeiculos);
        Condutores = new ObservableCollection<Condutor>(listaCondutores);

        // NOVO: Se já existir configuração no banco, preenche a tela
        if (config != null)
        {
            _configAtual = config;
            CaminhoCertificado = config.CaminhoCertificado;
            SenhaCertificado = config.SenhaCertificado;
            UfSelecionada = config.UfEmitente;
            CnpjEmitente = config.CnpjEmitente;
            RazaoSocialEmitente = config.RazaoSocialEmitente;
            IeEmitente = config.IeEmitente;
            LogradouroEmitente = config.LogradouroEmitente;
            NumeroEmitente = config.NumeroEmitente;
            BairroEmitente = config.BairroEmitente;
            CepEmitente = config.CepEmitente;
            CidadeEmitente = config.CidadeEmitente;
            IbgeEmitente = config.CodigoIbgeCidade;
        }

        // =================================================================
        // INICIALIZAÇÃO GLOBAL DO MOTOR FISCAL (ZEUS) NO BOOT DO SISTEMA
        // =================================================================
        if (!string.IsNullOrEmpty(config.CaminhoCertificado) && File.Exists(config.CaminhoCertificado))
        {
            try
            {
                // Diz ao Zeus onde está o nosso ficheiro .pfx e a senha
                MDFeConfiguracao.Instancia.ConfiguracaoCertificado.TipoCertificado = TipoCertificado.A1ByteArray;
                MDFeConfiguracao.Instancia.ConfiguracaoCertificado.ArrayBytesArquivo = File.ReadAllBytes(config.CaminhoCertificado);
                MDFeConfiguracao.Instancia.ConfiguracaoCertificado.Senha = config.SenhaCertificado;
                MDFeConfiguracao.Instancia.ConfiguracaoCertificado.ManterDadosEmCache = true;

                // Aponta para a pasta dos Schemas (.xsd) que criamos no Core
                var diretorioBase = AppDomain.CurrentDomain.BaseDirectory;
                MDFeConfiguracao.Instancia.CaminhoSchemas = Path.Combine(diretorioBase, "Schemas");

                // Configura a Versão da SEFAZ para o MDF-e
                MDFeConfiguracao.Instancia.VersaoWebService.VersaoLayout = MDFe.Utils.Flags.VersaoServico.Versao300;

                Console.WriteLine("Motor Fiscal (Zeus) configurado com sucesso no arranque!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aviso: Falha ao carregar o certificado no Zeus: {ex.Message}");
            }
        }
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
    [NotifyPropertyChangedFor(nameof(IsConfiguracoes))]
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

    // ==========================================
    // TELA DE CONFIGURAÇÕES (EMITENTE E CERTIFICADO)
    // ==========================================

    public bool IsConfiguracoes => PaginaAtual == Page.Configuracoes;

    [RelayCommand] private void AbrirConfiguracoes() => PaginaAtual = Page.Configuracoes;

    // Campos da Tela
    [ObservableProperty] private string _caminhoCertificado = string.Empty;
    [ObservableProperty] private string _senhaCertificado = string.Empty;
    [ObservableProperty] private string _ufSelecionada = "SP";

    // Lista de UFs para o ComboBox
    public ObservableCollection<string> ListaUFs { get; } = new()
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    // Dados extraídos/consultados
    [ObservableProperty] private string _cnpjEmitente = string.Empty;
    [ObservableProperty] private string _razaoSocialEmitente = string.Empty;
    [ObservableProperty] private string _ieEmitente = string.Empty;
    [ObservableProperty] private string _logradouroEmitente = string.Empty;
    [ObservableProperty] private string _numeroEmitente = string.Empty;
    [ObservableProperty] private string _bairroEmitente = string.Empty;
    [ObservableProperty] private string _cepEmitente = string.Empty;
    [ObservableProperty] private string _cidadeEmitente = string.Empty;
    [ObservableProperty] private long _ibgeEmitente = 0;

    // Lista de documentos importados
    public ObservableCollection<DocumentoFiscal> DocumentosFiscais { get; } = new();

    // Propriedades calculadas que atualizam a tela automaticamente
    public decimal ValorTotalCarga => DocumentosFiscais.Sum(d => d.Valor);
    public decimal PesoTotalCarga => DocumentosFiscais.Sum(d => d.Peso);

    [RelayCommand]
    private async Task ProcurarCertificadoAsync()
    {
        // Obtém a janela principal da aplicação para abrir o Dialog por cima dela
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window == null) return;

            // Abre a janela nativa do Windows/Linux/Mac para escolher o ficheiro
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Selecione o Certificado Digital A1",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Certificado PFX") { Patterns = new[] { "*.pfx", "*.p12" } },
                    new FilePickerFileType("Todos os Arquivos") { Patterns = new[] { "*.*" } }
                }
            });

            // Se o utilizador escolheu um ficheiro e não clicou em cancelar
            if (files != null && files.Count > 0)
            {
                // Preenche a caixa de texto magicamente
                CaminhoCertificado = files[0].Path.LocalPath;
            }
        }
    }

    [RelayCommand]
    private void ConsultarSefaz()
    {
        try
        {
            if (string.IsNullOrEmpty(CaminhoCertificado) || string.IsNullOrEmpty(SenhaCertificado))
            {
                Console.WriteLine("Informe o caminho e a senha do certificado.");
                return;
            }

            // 1. Carrega o Certificado Digital (A1) usando o padrão .NET 9 / 10
            var certificado = X509CertificateLoader.LoadPkcs12FromFile(CaminhoCertificado, SenhaCertificado);

            // 2. Extrai o CNPJ de dentro do Certificado (Geralmente fica no Subject)
            Match match = Regex.Match(certificado.Subject, @"([0-9]{14})");
            if (match.Success)
            {
                CnpjEmitente = match.Groups[1].Value;
            }
            else
            {
                Console.WriteLine("Não foi possível encontrar o CNPJ no certificado.");
                return;
            }

            var diretorioBase = AppDomain.CurrentDomain.BaseDirectory;

            var cfg = new ConfiguracaoServico
            {
                tpAmb = TipoAmbiente.Producao,
                tpEmis = TipoEmissao.teNormal,
                ProtocoloDeSeguranca = System.Net.SecurityProtocolType.Tls12,
                cUF = (Estado)Enum.Parse(typeof(Estado), UfSelecionada),
                VersaoLayout = VersaoServico.Versao400,
                ModeloDocumento = ModeloDocumento.NFe,
                VersaoNfeConsultaCadastro = VersaoServico.Versao400,

                // === CONFIGURAÇÃO DOS SCHEMAS ===
                DiretorioSchemas = Path.Combine(diretorioBase, "Schemas"),
                ValidarSchemas = true // Agora podemos ligar a validação com segurança!
            };

            // 4. Instancia o Serviço NFe do Zeus passando a config e o certificado no construtor
            using var servicoSefaz = new ServicosNFe(cfg, certificado);

            // 5. Faz a chamada WebService
            var retornoSefaz = servicoSefaz.NfeConsultaCadastro(UfSelecionada, ConsultaCadastroTipoDocumento.Cnpj, CnpjEmitente);

            // 6. Preenche a tela se a Sefaz retornar os dados
            if (retornoSefaz != null && retornoSefaz.Retorno != null && retornoSefaz.Retorno.infCons != null)
            {
                // AQUI FOI CORRIGIDO: Como infCad é um objeto direto, apenas verificamos se não é nulo
                if (retornoSefaz.Retorno.infCons.infCad != null)
                {
                    var dadosSefaz = retornoSefaz.Retorno.infCons.infCad;

                    RazaoSocialEmitente = dadosSefaz.xNome ?? string.Empty;
                    IeEmitente = dadosSefaz.IE ?? string.Empty;

                    if (dadosSefaz.ender != null)
                    {
                        LogradouroEmitente = dadosSefaz.ender.xLgr ?? string.Empty;
                        NumeroEmitente = dadosSefaz.ender.nro ?? string.Empty;
                        BairroEmitente = dadosSefaz.ender.xBairro ?? string.Empty;
                        CepEmitente = dadosSefaz.ender.CEP?.ToString() ?? string.Empty;
                        CidadeEmitente = dadosSefaz.ender.xMun ?? string.Empty;
                        IbgeEmitente = (dadosSefaz?.ender?.cMun != null) ? long.Parse(dadosSefaz.ender.cMun) : 0;
                    }

                    Console.WriteLine("Dados consultados com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Sefaz retornou: {retornoSefaz.Retorno.infCons.xMotivo}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao consultar SEFAZ: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EmitirMdfeAsync()
    {
        try
        {
            if (VeiculoSelecionado == null || CondutorSelecionado == null)
            {
                Console.WriteLine("Por favor, selecione o veículo e o condutor no Passo 3.");
                return;
            }

            if (string.IsNullOrEmpty(_configAtual.CaminhoCertificado))
            {
                Console.WriteLine("Configure o seu certificado digital antes de emitir.");
                return;
            }

            Console.WriteLine("Iniciando a montagem do MDF-e...");

            // 1. Instancia o nosso Motor Fiscal
            var motor = new MdfeMotorService();

            // 3. O Motor gera a classe oficial do Zeus
            var mdfe = motor.MontarMDFe(_configAtual, "SP", "MG", VeiculoSelecionado, CondutorSelecionado, DocumentosFiscais.ToList());

            // 4. A GRANDE MAGIA: O Zeus já tem as configurações na memória! 
            // Ele calcula a Chave, Dígito Verificador, QR Code, VALIDA contra os Schemas e Assina!
            mdfe.Assina(); // Assina digitalmente
            mdfe.Valida(); // Verifica se o XML não tem falhas estruturais

            // 5. Gera o XML final
            var xmlAssinadoString = mdfe.XmlString();

            Console.WriteLine("================ XML DO MDF-E ASSINADO E VALIDADO ================");
            Console.WriteLine(xmlAssinadoString);
            Console.WriteLine("==================================================================");

            Console.WriteLine("MDF-e Gerado com sucesso! O próximo passo é transmitir à Sefaz.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro crasso ao gerar MDF-e: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportarXmlAsync()
    {
        // Usa a mesma estratégia segura que funcionou para o certificado
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window == null) return;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Selecione os arquivos XML da Nota Fiscal",
                AllowMultiple = true, // Permite selecionar vários de uma vez!
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Arquivos XML") { Patterns = new[] { "*.xml" } },
                    new FilePickerFileType("Todos os Arquivos") { Patterns = new[] { "*.*" } }
                }
            });

            if (files != null)
            {
                foreach (var file in files)
                {
                    // Chama o nosso método de processamento para cada arquivo
                    AdicionarDocumentoViaXml(file.Path.LocalPath);
                }
            }
        }
    }

    public void AdicionarDocumentoViaXml(string caminhoArquivo)
    {
        try
        {
            // Carrega o XML
            var doc = XDocument.Load(caminhoArquivo);

            // Procura a tag <infNFe> e verifica se existe
            var infNFe = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "infNFe");
            if (infNFe == null)
            {
                Console.WriteLine("O arquivo não contém a tag <infNFe>.");
                return;
            }

            // 1. CHAVE DE ACESSO (Leitura Segura)
            var idAttr = infNFe.Attribute("Id");
            string chave = idAttr != null ? idAttr.Value.Replace("NFe", "") : "";

            // Se já existe, não adiciona duplicado
            if (DocumentosFiscais.Any(d => d.Chave == chave)) return;

            // 2. VALOR DA CARGA
            var vNfNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "vNF");
            decimal valor = 0;
            if (vNfNode != null && !string.IsNullOrEmpty(vNfNode.Value))
            {
                decimal.TryParse(vNfNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
            }

            // 3. PESO DA CARGA
            var pesoNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "pesoB");
            decimal peso = 0;
            if (pesoNode != null && !string.IsNullOrEmpty(pesoNode.Value))
            {
                decimal.TryParse(pesoNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out peso);
            }

            // 4. MUNICÍPIO DE DESCARGA
            long ibge = 0;
            string municipio = "DESCONHECIDO";

            var enderDest = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "enderDest");
            if (enderDest != null)
            {
                var cMunNode = enderDest.Elements().FirstOrDefault(x => x.Name.LocalName == "cMun");
                if (cMunNode != null && !string.IsNullOrEmpty(cMunNode.Value))
                {
                    long.TryParse(cMunNode.Value, out ibge);
                }

                var xMunNode = enderDest.Elements().FirstOrDefault(x => x.Name.LocalName == "xMun");
                if (xMunNode != null)
                {
                    municipio = xMunNode.Value;
                }
            }

            // Adiciona à lista
            DocumentosFiscais.Add(new DocumentoFiscal
            {
                Chave = chave,
                Valor = valor,
                Peso = peso,
                IbgeDescarga = ibge,
                MunicipioDescarga = municipio.ToUpper()
            });

            // Avisa a interface
            OnPropertyChanged(nameof(ValorTotalCarga));
            OnPropertyChanged(nameof(PesoTotalCarga));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar XML: {ex.Message}");
        }
    }
}