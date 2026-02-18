using EmissorMdfe.Core.Models;
using MDFe.Classes.Flags;
using MDFe.Classes.Informacoes;
using MDFe.Utils.Flags;
using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using System;
using System.Collections.Generic;

namespace EmissorMdfe.Core.Services;

public class MdfeMotorService
{
    /// <summary>
    /// Converte os dados da nossa aplicação para o formato nativo do Zeus MDFe
    /// </summary>
    public MDFe.Classes.Informacoes.MDFe MontarMDFe(
        ConfiguracaoApp config,
        string ufOrigem,
        string ufDestino,
        Veiculo veiculo,
        Condutor condutor,
        List<string> chavesNFe)
    {
        // ==========================================
        // 1. INICIALIZA O OBJETO MDF-E
        // ==========================================
        var mdfe = new MDFe.Classes.Informacoes.MDFe
        {
            InfMDFe = new MDFeInfMDFe
            {
                Versao = MDFe.Utils.Flags.VersaoServico.Versao300,
                Id = "", // Fica vazio. O Zeus gera automaticamente ao assinar!

                // ==========================================
                // 2. IDENTIFICAÇÃO DA VIAGEM
                // ==========================================
                Ide = new MDFeIde
                {
                    CUF = (Estado)Enum.Parse(typeof(Estado), config.UfEmitente),
                    TpAmb = config.Ambiente == "Producao" ? TipoAmbiente.Producao : TipoAmbiente.Homologacao,
                    TpEmit = MDFeTipoEmitente.TransportadorCargaPropria,
                    TpTransp = MDFeTpTransp.CTC,
                    Mod = ModeloDocumento.MDFe,
                    Serie = 1,
                    NMDF = 1, // FUTURO: Buscar do SQLite
                    CMDF = new Random().Next(10000000, 99999999), // Agora exige 'int'
                    CDV = 0,
                    Modal = MDFeModal.Rodoviario,
                    DhEmi = DateTime.Now, // Exige 'DateTime'
                    TpEmis = MDFeTipoEmissao.Normal,
                    ProcEmi = MDFeIdentificacaoProcessoEmissao.EmissaoComAplicativoContribuinte,
                    VerProc = "EmissorMdfeCore_1.0",
                    UFIni = (Estado)Enum.Parse(typeof(Estado), ufOrigem),
                    UFFim = (Estado)Enum.Parse(typeof(Estado), ufDestino),
                    InfMunCarrega = new List<MDFeInfMunCarrega>
                    {
                        new MDFeInfMunCarrega
                        {
                            CMunCarrega = config.CodigoIbgeCidade.ToString(),
                            XMunCarrega = config.CidadeEmitente
                        }
                    }
                },

                // ==========================================
                // 3. DADOS DA NOSSA EMPRESA (EMITENTE)
                // ==========================================
                Emit = new MDFeEmit
                {
                    CNPJ = config.CnpjEmitente,
                    IE = config.IeEmitente,
                    XNome = config.RazaoSocialEmitente,
                    XFant = config.RazaoSocialEmitente,
                    EnderEmit = new MDFeEnderEmit
                    {
                        XLgr = config.LogradouroEmitente,
                        Nro = config.NumeroEmitente,
                        XBairro = config.BairroEmitente,
                        CMun = config.CodigoIbgeCidade,
                        XMun = config.CidadeEmitente,
                        CEP = string.IsNullOrWhiteSpace(config.CepEmitente) ? 0 : long.Parse(config.CepEmitente.Replace("-", "")), // Exige 'long'
                        UF = (Estado)Enum.Parse(typeof(Estado), config.UfEmitente)
                    }
                },

                // ==========================================
                // 4. MODAL RODOVIÁRIO (VEÍCULO E CONDUTOR)
                // ==========================================
                InfModal = new MDFeInfModal
                {
                    VersaoModal = MDFeVersaoModal.Versao300,
                    Modal = new MDFeRodo // Zeus mapeia o bloco 'rodo' aqui dentro da propriedade 'Modal' polimórfica
                    {
                        VeicTracao = new MDFeVeicTracao
                        {
                            Placa = veiculo.Placa.Replace("-", "").Trim(),
                            RENAVAM = veiculo.Renavam,
                            Tara = veiculo.TaraKG > 0 ? veiculo.TaraKG : 10000,         // Fallback de segurança se esquecer
                            CapKG = veiculo.CapacidadeKG > 0 ? veiculo.CapacidadeKG : 20000,
                            TpRod = veiculo.TipoRodado > 0 ? (MDFeTpRod)veiculo.TipoRodado : MDFeTpRod.Outros,
                            TpCar = (MDFeTpCar)veiculo.TipoCarroceria,
                            ProxyUF = string.IsNullOrEmpty(veiculo.UfLicenciamento) ? config.UfEmitente : veiculo.UfLicenciamento,
                            Condutor = new List<MDFeCondutor>
                            {
                                new MDFeCondutor { XNome = condutor.Nome, CPF = condutor.Cpf.Replace(".", "").Replace("-", "").Trim() }
                            }
                        }
                    }
                },

                // ==========================================
                // 5. TOTAIS DO MANIFESTO
                // ==========================================
                Tot = new MDFeTot
                {
                    QNFe = chavesNFe.Count,
                    vCarga = 5000.00m, // Esta foi a única propriedade não padronizada pelo Zeus. Continua com 'v' minúsculo
                    CUnid = MDFeCUnid.KG,
                    QCarga = 1500.000m
                },

                // ==========================================
                // 6. ADICIONAR NOTAS FISCAIS (DOCUMENTOS)
                // ==========================================
                InfDoc = new MDFeInfDoc
                {
                    InfMunDescarga = new List<MDFeInfMunDescarga>
                    {
                        new MDFeInfMunDescarga
                        {
                             CMunDescarga = "3106200", // A Sefaz pede o código do IBGE.
                             XMunDescarga = "BELO HORIZONTE",
                             InfNFe = new List<MDFeInfNFe>()
                        }
                    }
                }
            }
        };

        // Insere as chaves no município de descarregamento
        foreach (var chave in chavesNFe)
        {
            mdfe.InfMDFe.InfDoc.InfMunDescarga[0].InfNFe.Add(new MDFeInfNFe { ChNFe = chave });
        }

        return mdfe;
    }
}