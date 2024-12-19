using college_management.Constantes;
using college_management.Contextos.Interfaces;
using college_management.Dados;
using college_management.Dados.Modelos;
using college_management.Dados.Repositorios;
using college_management.Views;


namespace college_management.Contextos;


public class ContextoCargos : Contexto<Cargo>
{
	public ContextoCargos(BaseDeDados baseDeDados,
	                      Usuario     usuarioContexto) :
		base(baseDeDados,
		     usuarioContexto) { }

	public override async Task Cadastrar() 
    {

    }

	public override async Task Editar() { throw new NotImplementedException(); }

	public override async Task Excluir() { throw new NotImplementedException(); }

	public override void Visualizar()  
	{
        var permissaoAdmin = CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);


		if(permissaoAdmin)
		{
            OpcoesDeVisualizacao();

            Console.ReadKey();
		}

    }


	public override void VerDetalhes() 
    {

    }




    #region Metodos privados uteis

	void OpcoesDeVisualizacao()
	{
        MenuView menuPesquisa = new("Cargos",
                                    "Selecione um dos campos:",
                                    ["Nome do Cargo", "Id", "Exibir Todos"]);

        List<Cargo> cargos = new();
        RelatorioView<Cargo> relatorioView = null!;


        menuPesquisa.ConstruirLayout();
        menuPesquisa.LerEntrada();

        KeyValuePair<string, string>? campoPesquisa = menuPesquisa.OpcaoEscolhida switch
        {
            1 => new KeyValuePair<string, string>("Nome do Cargo",
                                                  "Insira o Nome do Cargo: "),
            2 => new KeyValuePair<string, string>("Id",
                                                  "Insira o Id do Cargo: "),

            3 => new KeyValuePair<string, string>("Exiir Todos", 
                                                  "Pressione Enter para ver todos os cargos"),

            _ => null
        };

        InputView inputPesquisa = new("Ver Detalhes: Pesquisar Cargo");

        if (campoPesquisa is null)
        {
            inputPesquisa.LerEntrada("Campo",
                                     "Campo inv�lido. Tente novamente.");

            return;
        }

        inputPesquisa.LerEntrada(campoPesquisa?.Key,
                                 campoPesquisa?.Value);

        

        if (menuPesquisa.OpcaoEscolhida is 1)
        {
            var nomeDoCargo = inputPesquisa.ObterEntrada("Nome do Cargo");
            cargos.Add(BaseDeDados.Cargos.ObterPorNome(nomeDoCargo));
        }

        else if (menuPesquisa.OpcaoEscolhida is 2)
        {
            var id = inputPesquisa.ObterEntrada("Id");
            cargos.Add(BaseDeDados.Cargos.ObterPorId(id));
        }

        else if(menuPesquisa.OpcaoEscolhida is 3)
        {
            cargos = BaseDeDados.Cargos.ObterTodos();
        }

        if (!cargos.Any())
        {
            inputPesquisa.LerEntrada("Cargo",
                                     "Cargo n�o encontrado.");
            return;
        }

        relatorioView = new RelatorioView<Cargo>("Cargos", cargos);
        

        relatorioView.ConstruirLayout();
        relatorioView.Exibir();
    }

    #endregion
}