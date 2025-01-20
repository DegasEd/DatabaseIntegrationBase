-- Conceder todas as permissões no schema 'movie' ao usuário postgres
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;

-- Criação da tabela 'movies' no schema 'movie' se não existir
CREATE TABLE IF NOT EXISTS public.movies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    gender VARCHAR(50),
    is_active BOOLEAN NOT NULL
);

-- Conceder todas as permissões na tabela 'movies' ao usuário postgres
GRANT ALL PRIVILEGES ON TABLE public.movies TO postgres;