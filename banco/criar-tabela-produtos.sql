-- Checkpoint 5 - Cadastro de Produtos (SQLite)

CREATE TABLE IF NOT EXISTS Produtos (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT    NOT NULL,
    Preco     NUMERIC NOT NULL,
    Estoque   INTEGER NOT NULL,
    Categoria TEXT    NOT NULL
);

-- dados de exemplo
INSERT INTO Produtos (Nome, Preco, Estoque, Categoria) VALUES ('Teclado mecânico', 189.90, 15, 'Periféricos');
INSERT INTO Produtos (Nome, Preco, Estoque, Categoria) VALUES ('Monitor 24 polegadas', 899.00, 8, 'Monitores');
INSERT INTO Produtos (Nome, Preco, Estoque, Categoria) VALUES ('Cadeira de escritório', 649.50, 4, 'Móveis');
