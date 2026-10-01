using System.IO;

namespace Produtos.App;

public static class Log
{
    private static readonly string Caminho = Path.Combine(AppContext.BaseDirectory, "log.txt");

    public static void Registrar(string mensagem)
    {
        File.AppendAllText(Caminho, $"[{DateTime.Now:yyyy-MM-dd HH:mm}] {mensagem}{Environment.NewLine}");
    }
}
