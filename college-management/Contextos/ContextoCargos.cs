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
        var temPermissao =
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita) || 
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        Cargo novoCargo = null;

        if (temPermissao)
            novoCargo = TelaDeCadastro();

        if(novoCargo is null) return;

        var resultado = await BaseDeDados.Cargos.Adicionar(novoCargo);

        if (resultado) Console.WriteLine("Cargo salvo com sucesso!");

        Console.ReadKey();
        


    }

	public override async Task Editar() 
    {
        var temPermissao =
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita) ||
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        Cargo cargo = null;
        string nomeCargo = "";


        InputView inputView = new InputView("--Editor de cargos--");

        if(temPermissao)
        {
            nomeCargo = TelaSelecaoParaEditar(inputView);

            cargo = BaseDeDados.Cargos.ObterPorNome(nomeCargo);

            cargo = TelaDeEdicao(cargo, inputView);

            if(await BaseDeDados.Cargos.Atualizar(cargo))
                inputView.LerEntrada("Sair", "Cargo Editado com sucesso");

        }
    }

	public override async Task Excluir() 
    {
        var temPermissao =
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita) ||
        CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);

        InputView inputView = new InputView("Exclusao de Cargo");
        string cargoNome = "";
        string cargoId = "";

        if(temPermissao)
        {
            cargoNome = TelaExclusao(inputView);

            cargoId = BaseDeDados.Cargos.ObterPorNome(cargoNome).Id!;

            
            if (await BaseDeDados.Cargos.Remover(cargoId))
                inputView.LerEntrada("Sair", "Exclus�o realizada com sucesso");

            
        }


    }

	public override void Visualizar()  
	{
        var temPermissao = CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores) ||
                                           CargoContexto.TemPermissao(PermissoesAcesso.AcessoEscrita);

        RelatorioView<Cargo> relatorioView;

        if (temPermissao)
        {
            relatorioView = new RelatorioView<Cargo>("Visualizar Usu�rios",
                                                       BaseDeDados.Cargos.ObterTodos());

        }

        else
            relatorioView = new RelatorioView<Cargo>("Visualizar Usu�rio", new List<Cargo>() 
            { 
                BaseDeDados.Cargos.ObterPorId(UsuarioContexto.CargoId) 
            });


        relatorioView.ConstruirLayout();
        relatorioView.Exibir();

        InputView inputRelatorio = new(relatorioView.Titulo);
        inputRelatorio.LerEntrada("Sair", relatorioView.Layout.ToString());
    }


	public override void VerDetalhes() 
    {
        var permissaoAdmin = CargoContexto.TemPermissao(PermissoesAcesso.AcessoAdministradores);


        if (permissaoAdmin)
        {
            OpcoesDeVisualizacao();

            Console.ReadKey();
        }
    }




    #region Metodos privados uteis

    Cargo TelaDeCadastro()
    {
        InputView inputUsuario = new("Cadastrar cargo");
        inputUsuario.ConstruirLayout();

        /*KeyValuePair<string, string?>[] mensagensUsuario =
        {
            new("Nome", "insira o nome do cargo"),
            new("Permiss�es","Selecione o nivel de permiss�o do cargo")
        };*/

        inputUsuario.LerEntrada("Nome", "Insira o nome do novo cargo: ");

        string nivelDePermissao = SelecaoDePermissao();
        string nomeCargo = inputUsuario.EntradasUsuario["Nome"];

        if (nomeCargo is not null &&
            nivelDePermissao is not null)
        {
            return new Cargo(nomeCargo, new List<string>() { nivelDePermissao });
        }

        else
            return null;
    }



	void OpcoesDeVisualizacao()
	{
        MenuView menuPesquisa = new("Cargos",
                                    "Selecione um dos campos:",
                                    ["Nome do Cargo", "Id"]);

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


    string SelecaoDePermissao()
    {

        var propriedades = typeof(PermissoesAcesso).GetFields();
        string[] nomePropriedades = new string[propriedades.Length];

        for (int i = 0; i < propriedades.Length; i++)
        {
            nomePropriedades[i] = propriedades[i].Name;
        }

        MenuView menuView = new MenuView("Permiss�es", "Selecione o nivel de permiss�o do cargo,", nomePropriedades);

        menuView.ConstruirLayout();
        menuView.Exibir();
        menuView.LerEntrada();
        Console.Clear();



        return nomePropriedades[menuView.OpcaoEscolhida-1];
    }


    public string TelaExclusao(InputView inputView)
    {

        inputView.LerEntrada("name", "Insira o nome do cargo a ser excluido do banco de dados: ");

        var nomeCargo = inputView.ObterEntrada("name");

        return nomeCargo;
    }

    public string TelaSelecaoParaEditar(InputView inputView)
    {

        inputView.LerEntrada("name", "Insira o nome do cargo a ser editado: ");

        var cargoId = inputView.ObterEntrada("name");

        return cargoId;
    }


    public Cargo TelaDeEdicao(Cargo cargoAtual, InputView inputView)
    {

        inputView.LerEntrada("nome",
            $"Nome Atual: {cargoAtual.Nome}\n\nEscolha um novo nome: ");

        cargoAtual.Permissoes[0] = SelecaoDePermissao();

        cargoAtual.Nome = inputView.ObterEntrada("nome");

        return cargoAtual;
    }

    #endregion
}