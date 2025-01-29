using DatabaseIntegrationBase.ApiService;
using Microsoft.OpenApi.Models;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add MySql integration
builder.AddMySqlDataSource("moviedb");

// Add swagger to API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Movie API",
        Description = "API for managing a list of movies and their active status.",
        TermsOfService = new Uri("https://example.com/terms")
    });
});

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// GET: /movieitems - Retorna todos os filmes
app.MapGet("/movieitems", async (MySqlConnection connection) =>
{
    var movies = new List<Movie>();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Name, Gender, IsActive FROM movies";

    await connection.OpenAsync();
    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        movies.Add(new Movie
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Gender = reader.GetString(2),
            IsActive = reader.GetBoolean(3),
        });
    }
    await connection.CloseAsync();
    return Results.Ok(movies);
});

// POST: /movieitems - Adiciona um novo filme
app.MapPost("/movieitems", async (MySqlConnection connection, Movie movie) =>
{
    var command = connection.CreateCommand();
    command.CommandText = "INSERT INTO movies (Name, Gender, IsActive) VALUES (@Name, @Gender, @IsActive)";
    command.Parameters.AddWithValue("@Name", movie.Name ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@Gender", movie.Gender ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@IsActive", movie.IsActive);

    await connection.OpenAsync();
    await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();

    return Results.Created("/movieitems", movie);
});

// GET: /movieitems/active - Retorna filmes ativos
app.MapGet("/movieitems/active", async (MySqlConnection connection) =>
{
    var movies = new List<Movie>();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Name, Gender, IsActive FROM movies WHERE IsActive = 1";

    await connection.OpenAsync();
    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        movies.Add(new Movie
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Gender = reader.GetString(2),
            IsActive = reader.GetBoolean(3),
        });
    }
    await connection.CloseAsync();
    return Results.Ok(movies);
});

// PUT: /movieitem - Atualiza um filme
app.MapPut("/movieitem", async (MySqlConnection connection, Movie movie) =>
{
    var command = connection.CreateCommand();
    command.CommandText = "UPDATE movies SET Name = @Name, Gender = @Gender, IsActive = @IsActive WHERE Id = @Id";
    command.Parameters.AddWithValue("@Id", movie.Id);
    command.Parameters.AddWithValue("@Name", movie.Name ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@Gender", movie.Gender ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@IsActive", movie.IsActive);

    await connection.OpenAsync();
    var rowsAffected = await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();

    if (rowsAffected == 0)
        return Results.NotFound();

    return Results.NoContent();
});

// DELETE: /movieitem/{id} - Remove um filme
app.MapDelete("/movieitem/{id}", async (MySqlConnection connection, int id) =>
{
    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM movies WHERE Id = @Id";
    command.Parameters.AddWithValue("@Id", id);

    await connection.OpenAsync();
    var rowsAffected = await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();

    if (rowsAffected == 0)
        return Results.NotFound();

    return Results.NoContent();
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
