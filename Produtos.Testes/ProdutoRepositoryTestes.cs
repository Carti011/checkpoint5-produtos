using System.Globalization;
using Produtos.App;

namespace Produtos.Testes;

public class ProdutoRepositoryTestes : IDisposable
{
    private readonly string _arquivoBanco = Path.Combine(Path.GetTempPath(), $"produtos-teste-{Guid.NewGuid():N}.db");
    private readonly ProdutoRepository _repositorio;

    public ProdutoRepositoryTestes()
    {
        _repositorio = new ProdutoRepository($"Data Source={_arquivoBanco};Pooling=False");
        _repositorio.CriarBancoSeNaoExistir(Path.Combine(AppContext.BaseDirectory, "banco", "criar-tabela-produtos.sql"));
    }

    public void Dispose()
    {
        File.Delete(_arquivoBanco);
    }

    [Fact]
    public void Inserir_e_BuscarPorId_devolvem_o_mesmo_produto_com_preco_decimal_intacto()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        var produto = new Produto { Nome = "Teclado", Preco = 189.90m, Estoque = 10, Categoria = "Periféricos" };

        _repositorio.Inserir(produto);
        var encontrado = _repositorio.BuscarPorId(produto.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("Teclado", encontrado.Nome);
        Assert.Equal(189.90m, encontrado.Preco);
        Assert.Equal(10, encontrado.Estoque);
        Assert.Equal("Periféricos", encontrado.Categoria);
    }

    [Fact]
    public void Listar_devolve_os_produtos_inseridos()
    {
        var mouse = NovoProduto("Mouse");
        var webcam = NovoProduto("Webcam");
        _repositorio.Inserir(mouse);
        _repositorio.Inserir(webcam);

        var produtos = _repositorio.Listar();

        Assert.Contains(produtos, p => p.Id == mouse.Id && p.Nome == "Mouse");
        Assert.Contains(produtos, p => p.Id == webcam.Id && p.Nome == "Webcam");
    }

    [Fact]
    public void Atualizar_altera_os_campos()
    {
        var produto = NovoProduto("Cabo HDMI");
        _repositorio.Inserir(produto);

        produto.Nome = "Cabo HDMI 2.1";
        produto.Preco = 59.90m;
        produto.Estoque = 3;
        produto.Categoria = "Cabos";
        var atualizou = _repositorio.Atualizar(produto);
        var encontrado = _repositorio.BuscarPorId(produto.Id);

        Assert.True(atualizou);
        Assert.NotNull(encontrado);
        Assert.Equal("Cabo HDMI 2.1", encontrado.Nome);
        Assert.Equal(59.90m, encontrado.Preco);
        Assert.Equal(3, encontrado.Estoque);
        Assert.Equal("Cabos", encontrado.Categoria);
    }

    [Fact]
    public void Excluir_remove_e_BuscarPorId_passa_a_devolver_null()
    {
        var produto = NovoProduto("Headset");
        _repositorio.Inserir(produto);

        var excluiu = _repositorio.Excluir(produto.Id);

        Assert.True(excluiu);
        Assert.Null(_repositorio.BuscarPorId(produto.Id));
    }

    [Theory]
    [InlineData("'; DROP TABLE Produtos; --")]
    [InlineData("x', 0, 0, 'x'); DROP TABLE Produtos; --")]
    public void Inserir_com_sql_injection_no_nome_grava_o_texto_literal_e_a_tabela_sobrevive(string nomeMalicioso)
    {
        var produto = NovoProduto(nomeMalicioso);

        _repositorio.Inserir(produto);
        var encontrado = _repositorio.BuscarPorId(produto.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(nomeMalicioso, encontrado.Nome);
        Assert.Contains(_repositorio.Listar(), p => p.Id == produto.Id);
    }

    private static Produto NovoProduto(string nome) =>
        new() { Nome = nome, Preco = 10m, Estoque = 1, Categoria = "Teste" };
}
