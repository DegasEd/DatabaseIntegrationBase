using System.Runtime.Intrinsics.Arm;
using DatabaseIntegrationBase.ApiService;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

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

// Add postgres integration
builder.AddNpgsqlDataSource("postgres");

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/movieitems", async (NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var command = dbConnection.CreateCommand();
    command.CommandText = "SELECT * FROM public.movies";

    await using var reader = await command.ExecuteReaderAsync();

    var movies = new List<Movie>();

    while (await reader.ReadAsync())
    {
        movies.Add(new Movie
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Gender = reader.IsDBNull(2) ? null : reader.GetString(2),
            IsActive = reader.GetBoolean(3)
        });

    }
    return Results.Ok(movies);

}).WithName("GetAllMovies");
   

app.MapGet("/movieitems/active", async (NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var command = dbConnection.CreateCommand();
    command.CommandText = "SELECT * FROM public.movies WHERE is_active = TRUE";

    await using var reader = await command.ExecuteReaderAsync();

    var movies = new List<Movie>();

    while (await reader.ReadAsync())
    {
        movies.Add(new Movie
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Gender = reader.IsDBNull(2) ? null : reader.GetString(2),
            IsActive = reader.GetBoolean(3)
        });

    }
    return Results.Ok(movies);

}).WithName("GetActiveMovies");
    

app.MapGet("/movieitems/{id}", async (int id, NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var command = dbConnection.CreateCommand();
    command.CommandText = "SELECT * FROM public.movies WHERE id = @id";
    command.Parameters.AddWithValue("id", id);

    await using var reader = await command.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        var movie = new Movie
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Gender = reader.IsDBNull(2) ? null : reader.GetString(2),
            IsActive = reader.GetBoolean(3)
        };
        return Results.Ok(movie);
    }
    return Results.NotFound();
    

}).WithName("GetMovieById");
    

app.MapPost("/movieitems", async (Movie movie, NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var cmd = dataSource.CreateCommand("INSERT INTO public.movies (name, gender, is_active) VALUES (@name, @gender, @is_active) RETURNING id");
    cmd.Parameters.AddWithValue("name", movie.Name);
    cmd.Parameters.AddWithValue("gender", (object)movie.Gender ?? DBNull.Value);
    cmd.Parameters.AddWithValue("is_active", movie.IsActive);

    movie.Id = (int)await cmd.ExecuteScalarAsync();

    return Results.Ok(movie);

}).WithName("CreateMovie");

app.MapPut("/movieitems/{id}", async (int id, Movie inputMovie, NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var cmd = dataSource.CreateCommand("UPDATE public.movies SET name = @name, gender = @gender, is_active = @is_active WHERE id = @id");
    cmd.Parameters.AddWithValue("name", inputMovie.Name);
    cmd.Parameters.AddWithValue("gender", (object)inputMovie.Gender ?? DBNull.Value);
    cmd.Parameters.AddWithValue("is_active", inputMovie.IsActive);
    cmd.Parameters.AddWithValue("id", inputMovie.Id);

    var result = await cmd.ExecuteNonQueryAsync();

    return result > 0 ?
    Results.Ok(inputMovie) : 
    Results.NotFound();

});

app.MapDelete("/movieitems/{id}", async (int id, NpgsqlDataSource dataSource) =>
{
    using var dbConnection = dataSource.OpenConnection();

    await using var cmd = dataSource.CreateCommand("DELETE FROM public.movies WHERE id = @id");
    cmd.Parameters.AddWithValue("id", id);

    var result = await cmd.ExecuteNonQueryAsync();

    return result > 0 ?
        Results.Ok() : 
        Results.NotFound();

}).WithName("DeleteMovie");


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
