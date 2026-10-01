using System.IO;
using System.Windows;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Produtos.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var configuracao = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuracao.GetConnectionString("Produtos")
                ?? throw new InvalidOperationException("Connection string 'Produtos' não encontrada no appsettings.json.");

            // banco na pasta do executável
            var conexao = new SqliteConnectionStringBuilder(connectionString);
            conexao.DataSource = Path.Combine(AppContext.BaseDirectory, conexao.DataSource);

            var repositorio = new ProdutoRepository(conexao.ConnectionString);
            repositorio.CriarBancoSeNaoExistir(Path.Combine(AppContext.BaseDirectory, "banco", "criar-tabela-produtos.sql"));

            new MainWindow(repositorio).Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro ao iniciar", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
