using DatabaseIntegrationBase.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Add SqlServer client

builder.AddSqlServerClient("sqldb");

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

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/movieitems", async ([FromServices] SqlConnection connection) =>
{
    var movies = new List<Movie>();
    var command = new SqlCommand("SELECT Id, Name, Gender, IsActive FROM movie", connection);

    await connection.OpenAsync();
    var reader = await command.ExecuteReaderAsync();

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
app.MapPost("/movieitems", async ([FromServices] SqlConnection connection, Movie movie) =>
{
    var command = new SqlCommand("INSERT INTO movie (Name, Gender, IsActive) VALUES (@Name, @Gender, @IsActive)", connection);
    command.Parameters.AddWithValue("@Name", movie.Name ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@Gender", movie.Gender ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@IsActive", movie.IsActive);

    await connection.OpenAsync();
    await command.ExecuteNonQueryAsync();
    await connection.CloseAsync();

    return Results.Created($"/movieitems/{movie.Id}", movie);
});

// GET: /movieitems/active - Retorna apenas filmes ativos
app.MapGet("/movieitems/active", async ([FromServices] SqlConnection connection) =>
{
    var movies = new List<Movie>();
    var command = new SqlCommand("SELECT Id, Name, Gender, IsActive FROM movie WHERE IsActive = 1", connection);

    await connection.OpenAsync();
    var reader = await command.ExecuteReaderAsync();

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
app.MapPut("/movieitem", async ([FromServices] SqlConnection connection, Movie movie) =>
{
    var command = new SqlCommand("UPDATE movie SET Name = @Name, Gender = @Gender, IsActive = @IsActive WHERE Id = @Id", connection);
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
app.MapDelete("/movieitem/{id}", async ([FromServices] SqlConnection connection, int id) =>
{
    var command = new SqlCommand("DELETE FROM movie WHERE Id = @Id", connection);
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