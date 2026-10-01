using System.IO;
using Microsoft.Data.Sqlite;

namespace Produtos.App;

public class ProdutoRepository
{
    private readonly string _connectionString;

    public ProdutoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CriarBancoSeNaoExistir(string caminhoScript)
    {
        var arquivoBanco = new SqliteConnectionStringBuilder(_connectionString).DataSource;
        if (File.Exists(arquivoBanco))
            return;

        var script = File.ReadAllText(caminhoScript);

        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = script;
            comando.ExecuteNonQuery();

            Log.Registrar($"Banco criado pelo script {Path.GetFileName(caminhoScript)}");
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao criar o banco - {ex.Message}");
            throw new Exception("Não foi possível criar o banco de dados.", ex);
        }
    }

    public void Inserir(Produto produto)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = """
                INSERT INTO Produtos (Nome, Preco, Estoque, Categoria)
                VALUES (@nome, @preco, @estoque, @categoria)
                """;
            comando.Parameters.AddWithValue("@nome", produto.Nome);
            comando.Parameters.AddWithValue("@preco", produto.Preco);
            comando.Parameters.AddWithValue("@estoque", produto.Estoque);
            comando.Parameters.AddWithValue("@categoria", produto.Categoria);
            comando.ExecuteNonQuery();

            using var comandoId = conexao.CreateCommand();
            comandoId.CommandText = "SELECT last_insert_rowid()";
            produto.Id = Convert.ToInt32(comandoId.ExecuteScalar());

            Log.Registrar($"Produto inserido - Id={produto.Id}, Nome={produto.Nome}");
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao inserir produto - {ex.Message}");
            throw new Exception("Não foi possível inserir o produto.", ex);
        }
    }

    public List<Produto> Listar()
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos ORDER BY Id";

            var produtos = new List<Produto>();
            using var leitor = comando.ExecuteReader();
            while (leitor.Read())
                produtos.Add(Mapear(leitor));

            Log.Registrar($"Produtos listados - {produtos.Count} registro(s)");
            return produtos;
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao listar produtos - {ex.Message}");
            throw new Exception("Não foi possível listar os produtos.", ex);
        }
    }

    public Produto? BuscarPorId(int id)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = "SELECT Id, Nome, Preco, Estoque, Categoria FROM Produtos WHERE Id = @id";
            comando.Parameters.AddWithValue("@id", id);

            using var leitor = comando.ExecuteReader();
            var produto = leitor.Read() ? Mapear(leitor) : null;

            Log.Registrar(produto is null
                ? $"Produto não encontrado - Id={id}"
                : $"Produto encontrado - Id={id}, Nome={produto.Nome}");
            return produto;
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao buscar produto Id={id} - {ex.Message}");
            throw new Exception("Não foi possível buscar o produto.", ex);
        }
    }

    public bool Atualizar(Produto produto)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = """
                UPDATE Produtos
                SET Nome = @nome, Preco = @preco, Estoque = @estoque, Categoria = @categoria
                WHERE Id = @id
                """;
            comando.Parameters.AddWithValue("@nome", produto.Nome);
            comando.Parameters.AddWithValue("@preco", produto.Preco);
            comando.Parameters.AddWithValue("@estoque", produto.Estoque);
            comando.Parameters.AddWithValue("@categoria", produto.Categoria);
            comando.Parameters.AddWithValue("@id", produto.Id);
            var atualizou = comando.ExecuteNonQuery() > 0;

            Log.Registrar(atualizou
                ? $"Produto atualizado - Id={produto.Id}, Nome={produto.Nome}"
                : $"Produto não encontrado para atualizar - Id={produto.Id}");
            return atualizou;
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao atualizar produto Id={produto.Id} - {ex.Message}");
            throw new Exception("Não foi possível atualizar o produto.", ex);
        }
    }

    public bool Excluir(int id)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            using var comando = conexao.CreateCommand();
            comando.CommandText = "DELETE FROM Produtos WHERE Id = @id";
            comando.Parameters.AddWithValue("@id", id);
            var excluiu = comando.ExecuteNonQuery() > 0;

            Log.Registrar(excluiu
                ? $"Produto excluído - Id={id}"
                : $"Produto não encontrado para excluir - Id={id}");
            return excluiu;
        }
        catch (SqliteException ex)
        {
            Log.Registrar($"ERRO ao excluir produto Id={id} - {ex.Message}");
            throw new Exception("Não foi possível excluir o produto.", ex);
        }
    }

    // mapeamento
    private static Produto Mapear(SqliteDataReader leitor)
    {
        return new Produto
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("Id")),
            Nome = leitor.GetString(leitor.GetOrdinal("Nome")),
            Preco = leitor.GetDecimal(leitor.GetOrdinal("Preco")),
            Estoque = leitor.GetInt32(leitor.GetOrdinal("Estoque")),
            Categoria = leitor.GetString(leitor.GetOrdinal("Categoria"))
        };
    }
}
