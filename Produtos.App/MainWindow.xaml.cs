using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Produtos.App;

public partial class MainWindow : Window
{
    private static readonly CultureInfo Cultura = new("pt-BR");

    private readonly ProdutoRepository _repositorio;
    private readonly ObservableCollection<Produto> _produtos = new();

    public MainWindow(ProdutoRepository repositorio)
    {
        InitializeComponent();
        _repositorio = repositorio;
        gridProdutos.ItemsSource = _produtos;

        CarregarLista();
        MostrarStatus($"{_produtos.Count} produto(s) carregado(s).");
    }

    // menu

    private void Inserir_Click(object sender, RoutedEventArgs e)
    {
        var produto = LerFormulario();
        if (produto is null)
            return;

        try
        {
            _repositorio.Inserir(produto);
            CarregarLista();
            Selecionar(produto.Id);
            MostrarStatus($"Produto inserido com Id {produto.Id}.");
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void Listar_Click(object sender, RoutedEventArgs e)
    {
        CarregarLista();
        MostrarStatus($"Lista atualizada: {_produtos.Count} produto(s).");
    }

    private void Buscar_Click(object sender, RoutedEventArgs e)
    {
        if (!LerId(out var id))
            return;

        try
        {
            var produto = _repositorio.BuscarPorId(id);
            if (produto is null)
            {
                MostrarAviso($"Nenhum produto encontrado com Id {id}.");
                return;
            }

            PreencherFormulario(produto);
            Selecionar(produto.Id);
            MostrarStatus($"Produto {id} encontrado.");
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void Atualizar_Click(object sender, RoutedEventArgs e)
    {
        if (!LerId(out var id))
            return;

        var produto = LerFormulario();
        if (produto is null)
            return;
        produto.Id = id;

        try
        {
            if (!_repositorio.Atualizar(produto))
            {
                MostrarAviso($"Nenhum produto encontrado com Id {id}.");
                return;
            }

            CarregarLista();
            Selecionar(id);
            MostrarStatus($"Produto {id} atualizado.");
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (!LerId(out var id))
            return;

        var resposta = MessageBox.Show(this, $"Excluir o produto {id}?", "Confirmar exclusão",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (resposta != MessageBoxResult.Yes)
            return;

        try
        {
            if (!_repositorio.Excluir(id))
            {
                MostrarAviso($"Nenhum produto encontrado com Id {id}.");
                return;
            }

            CarregarLista();
            LimparFormulario();
            MostrarStatus($"Produto {id} excluído.");
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void Limpar_Click(object sender, RoutedEventArgs e)
    {
        LimparFormulario();
    }

    private void Sair_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void GridProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (gridProdutos.SelectedItem is Produto produto)
            PreencherFormulario(produto);
    }

    // lista

    private void CarregarLista()
    {
        try
        {
            var produtos = _repositorio.Listar();
            _produtos.Clear();
            foreach (var produto in produtos)
                _produtos.Add(produto);
        }
        catch (Exception ex)
        {
            MostrarErro(ex.Message);
        }
    }

    private void Selecionar(int id)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == id);
        gridProdutos.SelectedItem = produto;
        if (produto is not null)
            gridProdutos.ScrollIntoView(produto);
    }

    // formulário

    private Produto? LerFormulario()
    {
        var nome = txtNome.Text.Trim();
        var categoria = txtCategoria.Text.Trim();

        // validações
        if (nome.Length == 0)
            return Invalido("Informe o nome do produto.");

        var precoValido = decimal.TryParse(txtPreco.Text.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Cultura, out var preco);
        if (!precoValido || preco < 0)
            return Invalido("Preço deve ser um número maior ou igual a zero, com vírgula nos centavos (ex.: 189,90).");

        var estoqueValido = int.TryParse(txtEstoque.Text.Trim(), out var estoque);
        if (!estoqueValido || estoque < 0)
            return Invalido("Estoque deve ser um número inteiro maior ou igual a zero.");

        if (categoria.Length == 0)
            return Invalido("Informe a categoria do produto.");

        return new Produto { Nome = nome, Preco = preco, Estoque = estoque, Categoria = categoria };
    }

    private Produto? Invalido(string mensagem)
    {
        MostrarAviso(mensagem);
        return null;
    }

    private bool LerId(out int id)
    {
        if (int.TryParse(txtId.Text.Trim(), out id) && id > 0)
            return true;

        MostrarAviso("Informe um Id válido: digite no campo Id ou selecione um produto na lista.");
        return false;
    }

    private void PreencherFormulario(Produto produto)
    {
        txtId.Text = produto.Id.ToString();
        txtNome.Text = produto.Nome;
        txtPreco.Text = produto.Preco.ToString("0.00", Cultura);
        txtEstoque.Text = produto.Estoque.ToString();
        txtCategoria.Text = produto.Categoria;
    }

    private void LimparFormulario()
    {
        gridProdutos.SelectedItem = null;
        txtId.Clear();
        txtNome.Clear();
        txtPreco.Clear();
        txtEstoque.Clear();
        txtCategoria.Clear();
        txtNome.Focus();
    }

    // mensagens

    private void MostrarStatus(string mensagem)
    {
        txtStatus.Text = $"[{DateTime.Now:HH:mm:ss}] {mensagem}";
    }

    private void MostrarAviso(string mensagem)
    {
        MessageBox.Show(this, mensagem, "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void MostrarErro(string mensagem)
    {
        MessageBox.Show(this, mensagem, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
