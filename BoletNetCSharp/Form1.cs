using Boleto2Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace BoletNetCSharp
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGerar_Click(object sender, EventArgs e)
        {
            Boletos objBoletos = new Boletos();
            string strImpressora = null;
            bool blnImprimir = false;
            short intCopias = 1;
            try
            {


                var conta = new ContaBancaria
                {
                    Agencia = "1234",
                    DigitoAgencia = "X",
                    Conta = "123456",
                    DigitoConta = "X",
                    CarteiraPadrao = "09",
                    TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                    TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                    TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa
                };

                var ender = new Endereco
                {
                    LogradouroEndereco = "Av. das Pitangas",
                    LogradouroNumero = "569",
                    LogradouroComplemento = "Logradouro",
                    Bairro = "São João",
                    Cidade = "Sertãozinho",
                    UF = "SP",
                    CEP = "14170230"
                };

                objBoletos.Banco = Banco.Instancia(237);
                objBoletos.Banco.Cedente = Utils.GerarCedente("123456", "6", "000000", conta, ender);

                objBoletos.Banco.FormataCedente();

                for (int i = 1; i <= 1; i++)
                {

                    // DENTRO DA CLASSE UTILS CONTÉM O METODO PARA GERAR O TITULO / BOLETO : GerarBoleto();

                    //CRIAÇÃO DO TITULO
                    var Titulo = new Boleto(objBoletos.Banco);
                    Titulo.Sacado = Utils.GerarSacado();
                    Titulo.CodigoOcorrencia = "01";
                    Titulo.DescricaoOcorrencia = "Remessa Registrar";
                    Titulo.NumeroDocumento = i.ToString();
                    Titulo.NumeroControleParticipante = "12";
                    Titulo.NossoNumero = "123456" + i;
                    Titulo.DataEmissao = DateTime.Now;
                    Titulo.DataVencimento = DateTime.Now.AddDays(15);
                    Titulo.ValorTitulo = 200;
                    Titulo.Aceite = "N";
                    Titulo.EspecieDocumento = TipoEspecieDocumento.DM;
                    Titulo.DataDesconto = DateTime.Now.AddDays(15);
                    Titulo.ValorDesconto = 45;
                    //
                    //PARTE DA MULTA
                    Titulo.DataMulta = DateTime.Now.AddDays(15);
                    Titulo.PercentualMulta = 2;
                    Titulo.ValorMulta = Titulo.ValorTitulo * Titulo.PercentualMulta / 100;
                    Titulo.MensagemInstrucoesCaixa = $"Cobrar multa de {Titulo.ValorMulta.ToString("#,##0")} após a data de vencimento.";
                    //
                    //PARTE JUROS DE MORA
                    Titulo.DataJuros = DateTime.Now.AddDays(15);
                    Titulo.PercentualJurosDia = 10 / 30;
                    Titulo.ValorJurosDia = Titulo.ValorTitulo * Titulo.PercentualJurosDia / 100;
                    var instrucoes = $"Cobrar juros de {Titulo.ValorMulta.ToString("#,##0")} por dia.";

                    if (string.IsNullOrEmpty(Titulo.MensagemInstrucoesCaixa))
                    {
                        Titulo.MensagemInstrucoesCaixa = instrucoes;
                    }
                    else
                    {
                        Titulo.MensagemInstrucoesCaixa += Environment.NewLine + instrucoes;
                    }

                    Titulo.CodigoProtesto = TipoCodigoProtesto.NaoProtestar;
                    Titulo.DiasProtesto = 0;
                    Titulo.CodigoBaixaDevolucao = TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver;
                    Titulo.DiasBaixaDevolucao = 0;
                    Titulo.ValidarDados();
                    objBoletos.Add(Titulo);
                }

                if (File.Exists(Application.StartupPath + @"\remessa.txt"))
                {
                    File.Delete(Application.StartupPath + @"\remessa.txt");
                }

                //GERA ARQUIVO DE REMESSA
                var st = new MemoryStream();
                var remessa = new ArquivoRemessa(objBoletos.Banco, TipoArquivo.CNAB240, 1);
                remessa.GerarArquivoRemessa(objBoletos, st);
                var arquivo = new FileStream(Application.StartupPath + @"\remessa.txt", FileMode.Create, FileAccess.ReadWrite);

                st.WriteTo(arquivo);
                arquivo.Close();
                st.Close();

                StreamReader LerArquivo = new StreamReader(Application.StartupPath + @"\remessa.txt");

                StreamWriter RefazArquivo = new StreamWriter(Application.StartupPath + @"\remessa2.txt"); // Arquivo verificado para ser enviado ao banco
                string strTexto = null;
                int conta1 = 0;

                while (LerArquivo.Peek() != -1)
                {
                    strTexto = LerArquivo.ReadLine();
                    conta1 = strTexto.Length;
                    if (conta1 < 240)
                    {
                        conta1 = 240 - conta1;
                        string strEspaco = null;
                        for (int I = 1; I <= conta1; I++)
                            strEspaco = strEspaco + " ";
                        RefazArquivo.WriteLine(strTexto + strEspaco);
                    }
                    else
                        RefazArquivo.WriteLine(strTexto);
                }

                RefazArquivo.Close();
                LerArquivo.Close();

                // Gera boletos
                int numBoletos = 0;
                foreach (var linha in objBoletos)
                {
                    numBoletos += 1;
                    var NovoBoleto = new BoletoBancario();
                    NovoBoleto.Boleto = linha;
                    var pdf = NovoBoleto.MontaBytesPDF(false);
                    File.WriteAllBytes(Application.StartupPath + @"\boleto" + numBoletos + ".pdf", pdf);
                }

                MessageBox.Show("Boleto Gerado.");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IBanco _banco;
            var contaBancaria = new ContaBancaria
            {
                Agencia = "1234",
                DigitoAgencia = "X",
                Conta = "123456",
                DigitoConta = "X",
                CarteiraPadrao = "09",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa
            };

            var conta = new ContaBancaria
            {
                Agencia = "1234",
                DigitoAgencia = "X",
                Conta = "123456",
                DigitoConta = "X",
                CarteiraPadrao = "09",
                TipoCarteiraPadrao = TipoCarteira.CarteiraCobrancaSimples,
                TipoFormaCadastramento = TipoFormaCadastramento.ComRegistro,
                TipoImpressaoBoleto = TipoImpressaoBoleto.Empresa
            };

            var ender = new Endereco
            {
                LogradouroEndereco = "Av. das Pitangas",
                LogradouroNumero = "569",
                LogradouroComplemento = "Logradouro",
                Bairro = "São João",
                Cidade = "Sertãozinho",
                UF = "SP",
                CEP = "14170230"
            };

            _banco = Banco.Instancia(Bancos.Bradesco);
            _banco.Cedente = Utils.GerarCedente("123456", "6", "000000", conta, ender);
            _banco.FormataCedente();

            Utils.TestarHomologacao(_banco, TipoArquivo.CNAB400, "Bradesco Carteira 09", 1, true, "?", 223344);
        }
    }
}
