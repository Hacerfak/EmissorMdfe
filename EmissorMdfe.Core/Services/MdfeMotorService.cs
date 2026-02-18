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
        List<DocumentoFiscal> documentos,
        int numeroMdfe
        )
    {
        // 1. Agrupa os Municípios de Carregamento (Únicos)
        var municipiosCarregamento = documentos
            .Select(d => new { d.IbgeCarregamento, d.MunicipioCarregamento })
            .Distinct()
            .ToList();

        // Se não achou no XML (ex: nota manual), usa a da empresa como fallback
        if (!municipiosCarregamento.Any())
        {
            municipiosCarregamento.Add(new { IbgeCarregamento = config.CodigoIbgeCidade, MunicipioCarregamento = config.CidadeEmitente });
        }
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
                    TpAmb = config.Ambiente == 1 ? TipoAmbiente.Producao : TipoAmbiente.Homologacao,
                    TpEmit = (MDFeTipoEmitente)config.TipoEmitente,
                    TpTransp = MDFeTpTransp.CTC,
                    Mod = ModeloDocumento.MDFe,
                    Serie = 1,
                    NMDF = numeroMdfe, // FUTURO: Buscar do SQLite
                    CMDF = new Random().Next(10000000, 99999999), // Agora exige 'int'
                    CDV = 0,
                    Modal = MDFeModal.Rodoviario,
                    DhEmi = DateTime.Now, // Exige 'DateTime'
                    TpEmis = MDFeTipoEmissao.Normal,
                    ProcEmi = MDFeIdentificacaoProcessoEmissao.EmissaoComAplicativoContribuinte,
                    VerProc = "CoreMDFe_1.0",
                    UFIni = (Estado)Enum.Parse(typeof(Estado), ufOrigem),
                    UFFim = (Estado)Enum.Parse(typeof(Estado), ufDestino),
                    InfMunCarrega = municipiosCarregamento.Select(m => new MDFeInfMunCarrega
                    {
                        CMunCarrega = m.IbgeCarregamento.ToString(),
                        XMunCarrega = m.MunicipioCarregamento
                    }).ToList()
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
                    QNFe = documentos.Count,
                    vCarga = documentos.Sum(d => d.Valor),
                    CUnid = (MDFeCUnid)1, // 01 - KG
                    QCarga = documentos.Sum(d => d.Peso)
                },

                // ==========================================
                // 6. ADICIONAR NOTAS FISCAIS (DOCUMENTOS)
                // ==========================================
                InfDoc = new MDFeInfDoc
                {
                    // Inicializa a lista de descargas vazia para ser preenchida abaixo
                    InfMunDescarga = new List<MDFeInfMunDescarga>()
                }
            }
        };

        // AGRUPAMENTO MÁGICO: O Zeus exige que agrupemos as NFs por cidade de descarga!
        var gruposCidades = documentos.GroupBy(d => new { d.IbgeDescarga, d.MunicipioDescarga });

        foreach (var cidade in gruposCidades)
        {
            var infMunDescarga = new MDFeInfMunDescarga
            {
                CMunDescarga = cidade.Key.IbgeDescarga.ToString(),
                XMunDescarga = cidade.Key.MunicipioDescarga,
                InfNFe = new List<MDFeInfNFe>()
            };

            foreach (var doc in cidade)
            {
                infMunDescarga.InfNFe.Add(new MDFeInfNFe { ChNFe = doc.Chave });
            }

            mdfe.InfMDFe.InfDoc.InfMunDescarga.Add(infMunDescarga);
        }

        return mdfe;
    }
}