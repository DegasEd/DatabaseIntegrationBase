-- Certifique-se de criar e usar o banco de dados
USE moviedb;

-- Criar a tabela com ajustes no tipo BOOLEAN
CREATE TABLE movies (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255),
    Gender VARCHAR(255),
    IsActive TINYINT(1) NOT NULL 
);  