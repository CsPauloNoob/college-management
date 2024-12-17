using college_management.Constantes;
using college_management.Contextos.Interfaces;
using college_management.Dados;
using college_management.Dados.Modelos;
using college_management.Dados.Repositorios;
using college_management.Utilitarios;
using college_management.Views;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace college_management.Contextos;


public class ContextoMaterias : Contexto<Materia>
{
	public ContextoMaterias(BaseDeDados baseDeDados,
	                        Usuario     usuarioContexto) :
		base(baseDeDados,
		     usuarioContexto) { }

	public override async Task Cadastrar() 
	{
        var temPermissao =
            CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita)
            || CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        InputView inputUsuario = new("Cadastrar Mat�ria");
        inputUsuario.ConstruirLayout();

        if (!temPermissao)
        {
            inputUsuario.LerEntrada("Erro",
                                    "Voc� n�o tem permiss�o "
                                    + "para acessar esse recurso. ");

            return;
        }

        Dictionary<string, string> cadastroMateria
            = ObterCadastroMateria(inputUsuario);

        if (cadastroMateria["Confirma"] is not "S") return;

        if (!Enum.TryParse(cadastroMateria["Turno"], out Turno turnoEscolhido)) 
        {
            inputUsuario.LerEntrada("Erro",
                                    "O Turno inserido n�o foi "
                                    + "encontrado."
                                    + "Pressione Enter para continuar.");

            return;
        }

        if (!int.TryParse(cadastroMateria["CargaHoraria"], out int cargaHoraria))
        {
            inputUsuario.LerEntrada("Erro",
                                    "A carga hor�ria inserida n�o � v�lida."
                                    + "Pressione Enter para continuar.");

            return;
        }

        Materia? novaMateria = new(cadastroMateria["Nome"], turnoEscolhido, cargaHoraria);

        if (novaMateria is null)
        {
            inputUsuario
                .LerEntrada("Erro",
                            $"N�o foi poss�vel criar uma nova {nameof(Materia)}.");

            return;
        }

        var foiAdicionado
            = await BaseDeDados.Materias.Adicionar(novaMateria);

        var mensagemOperacao = foiAdicionado
                                   ? $"{nameof(Materia)} cadastrada com sucesso."
                                   : $"N�o foi poss�vel cadastrar uma nova {nameof(Materia)}.";

        inputUsuario.LerEntrada("Sair", mensagemOperacao);
    }

    private Dictionary<string, string> ObterCadastroMateria(InputView inputUsuario)
    {
        KeyValuePair<string, string?>[] mensagensUsuario =
        [
            new("Nome", "Insira o Nome: "),
            new("Turno", "Insira o Turno: "),
            new("CargaHoraria", "Insira a Carga Hor�ria: ")
        ];

        foreach (KeyValuePair<string, string?> mensagem
                 in mensagensUsuario)
            inputUsuario.LerEntrada(mensagem.Key,
                                    mensagem.Value);

        DetalhesView detalhesView = new("Confirmar Cadastro",
                                        inputUsuario
                                            .EntradasUsuario);

        detalhesView.ConstruirLayout();

        StringBuilder mensagemConfirmacao = new();
        mensagemConfirmacao.AppendLine(detalhesView.Layout
                                                   .ToString());

        mensagemConfirmacao.AppendLine("Confirma o Cadastro?\n");
        mensagemConfirmacao.Append("[S]im\t[N]�o: ");

        inputUsuario.LerEntrada("Confirma",
                                mensagemConfirmacao.ToString());

        return inputUsuario.EntradasUsuario;
    }

    public override async Task Editar() 
    {
        var temPermissao =
            CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita)
            || CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        Materia? materia = ObterDetalhesMateria();

        if (materia == null) return;

        InputView inputUsuario = new("Editar Mat�ria");
        inputUsuario.ConstruirLayout();

        if (!temPermissao)
        {
            inputUsuario.LerEntrada("Erro",
                                    "Voc� n�o tem permiss�o "
                                    + "para acessar esse recurso. ");

            return;
        }

        Dictionary<string, string> editarMateria
            = ObterCadastroMateria(inputUsuario);

        if (editarMateria["Confirma"] is not "S") return;

        if (editarMateria["Nome"] != null)
        {
            materia.Nome = editarMateria["Nome"];
        }

        if (editarMateria["Turno"] != "")
        {
            if (!Enum.TryParse(editarMateria["Turno"], out Turno turnoEscolhido))
            {
                inputUsuario.LerEntrada("Erro",
                                        "O Turno inserido n�o foi "
                                        + "encontrado."
                                        + "Pressione Enter para continuar.");

                return;
            }

            materia.Turno = turnoEscolhido;
        }

        if (editarMateria["CargaHoraria"] != "")
        {
            if (!int.TryParse(editarMateria["CargaHoraria"], out int cargaHoraria))
            {
                inputUsuario.LerEntrada("Erro",
                                        "A carga hor�ria inserida n�o � v�lida."
                                        + "Pressione Enter para continuar.");

                return;
            }

            materia.CargaHoraria = cargaHoraria;
        }

        var foiAtualizado
            = await BaseDeDados.Materias.Atualizar(materia);

        var mensagemOperacao = foiAtualizado
                                   ? $"{nameof(Materia)} atualizada com sucesso."
                                   : $"N�o foi poss�vel atualizar a {nameof(Materia)}.";

        inputUsuario.LerEntrada("Sair", mensagemOperacao);
    }

	public override async Task Excluir() 
    {
        var temPermissao =
            CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita)
            || CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        Materia? materia = ObterDetalhesMateria();

        if (materia == null) return;

        InputView inputUsuario = new("Deletar Mat�ria");

        StringBuilder mensagemConfirmacao = new();
        mensagemConfirmacao.AppendLine(inputUsuario.Layout
                                                   .ToString());

        mensagemConfirmacao.AppendLine("Tem certeza que deseja deletar essa mat�ria?\n");
        mensagemConfirmacao.Append("[S]im\t[N]�o: ");

        inputUsuario.LerEntrada("Confirma",
                                mensagemConfirmacao.ToString());

        if (inputUsuario.EntradasUsuario["Confirma"] is not "S") return;

        var foiDeletado
            = await BaseDeDados.Materias.Remover(materia.Id);

        var mensagemOperacao = foiDeletado
                                   ? $"{nameof(Materia)} deletada com sucesso."
                                   : $"N�o foi poss�vel deletar a {nameof(Materia)}.";

        inputUsuario.LerEntrada("Sair", mensagemOperacao);
    }

	public override void Visualizar()  
	{
        RelatorioView<Materia> relatorioView;

        relatorioView = new RelatorioView<Materia>("Visualizar Mat�rias",
                                                       BaseDeDados.Materias.ObterTodos());

        relatorioView.ConstruirLayout();

        InputView inputRelatorio = new(relatorioView.Titulo);
        inputRelatorio.LerEntrada("Sair", relatorioView.Layout.ToString());
	}

	public override void VerDetalhes() 
    {
        Materia? materia = ObterDetalhesMateria();

        if (materia == null) return;

        Dictionary<string, string> detalhes =
            UtilitarioTipos.ObterPropriedades(materia,
                                              ["Nome", "Turno", "CargaHoraria", "Id"]);

        DetalhesView detalhesMateria = new("Mat�ria Encontrada", detalhes);
        detalhesMateria.ConstruirLayout();

        InputView inputPesquisa = new("");
        inputPesquisa.LerEntrada("Sair", detalhesMateria.Layout.ToString());
    }

    private Materia ObterDetalhesMateria() 
    {
        MenuView menuPesquisa = new("Pesquisar Mat�ria",
                                    "Selecione um dos campos para pesquisar.",
                                    ["Nome", "Id"]);

        menuPesquisa.ConstruirLayout();
        menuPesquisa.LerEntrada();

        KeyValuePair<string, string>? campoPesquisa = menuPesquisa.OpcaoEscolhida switch
        {
            1 => new KeyValuePair<string, string>("Nome",
                                                  "Insira o Nome da Mat�ria: "),
            2 => new KeyValuePair<string, string>("Id",
                                                  "Insira o Id da Mat�ria: "),
            _ => null
        };

        InputView inputPesquisa = new("Ver Detalhes: Pesquisar Mat�ria");

        if (campoPesquisa is null)
        {
            inputPesquisa.LerEntrada("Campo",
                                     "Campo inv�lido. Tente novamente.");

            return null;
        }

        inputPesquisa.LerEntrada(campoPesquisa?.Key,
                                 campoPesquisa?.Value);

        Materia? materia = null;

        if (menuPesquisa.OpcaoEscolhida is 1)
        {
            var nome = inputPesquisa.ObterEntrada("Nome");
            materia = BaseDeDados.Materias.ObterPorNome(nome);
        }
        else if (menuPesquisa.OpcaoEscolhida is 2)
        {
            var id = inputPesquisa.ObterEntrada("Id");
            materia = BaseDeDados.Materias.ObterPorId(id);
        }

        if (materia is null)
        {
            inputPesquisa.LerEntrada("Mat�ria",
                                     "Mat�ria n�o encontrada.");

            return null;
        }

        return materia;
    }
}
