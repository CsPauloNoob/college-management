using college_management.Dados.Repositorios.Interfaces;
using college_management.Dados.Servicos;
using college_management.Modelos;

namespace college_management.Dados.Repositorios;

public sealed class Repositorio<T> : IRepositorio<T> where T : Modelo
{
    public Repositorio()
    {
        _servicoDeArquivos = new ServicoDeArquivos<T>();
        _baseDeDados = new List<T>();
    }

    private List<T>? _baseDeDados;
    private readonly ServicoDeArquivos<T> _servicoDeArquivos;
    
    public async void Dispose()
    {
        await _servicoDeArquivos.SalvarAssicrono(_baseDeDados);
    }

    public void Adicionar(T modelo)
    {
        _baseDeDados.Add(modelo);
        
        Dispose();
    }

    public async Task<List<T>> ObterTodos()
    {
        if (_baseDeDados.Count is 0)
           _baseDeDados = await _servicoDeArquivos.CarregarAssincrono();
        
        return _baseDeDados;
    }

    public T ObterPorId(string? id)
    {
        return _baseDeDados.FirstOrDefault(t => t.Id == id);
    }

    public void Atualizar(T modelo)
    {
        var modeloAntigo = ObterPorId(modelo.Id);

        if (modeloAntigo is null)
        {
            Adicionar(modelo);
            
            return;
        }
        
        Remover(modelo.Id);
        Adicionar(modelo);
        Dispose();
    }

    public void Remover(string? id)
    {
        var modelo = ObterPorId(id);

        if (modelo is null)
            return;
        
        _baseDeDados.Remove(modelo);
        Dispose();
    }
}