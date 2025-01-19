using System;

namespace DatabaseIntegrationBase.ApiService;

public class Movie
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; }
}
