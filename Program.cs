using Microsoft.OpenApi.Models;
using RecordApi.Middleware;
using RecordApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Record Management API",
        Version = "v1",
        Description = "A RESTful API built with .NET 8 Minimal APIs"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseApiKeyAuth();
var jsonData = File.ReadAllText("masterdata.json");
var master = JsonSerializer.Deserialize<MasterData>(jsonData);
var records = master?.Records ?? new List<Record>();


// Create
app.MapPost("/api/records", (Record record) =>
{
    record.Id = records.Count > 0 ? records.Max(r => r.Id) + 1 : 1;
    records.Add(record);
    return Results.Created($"/api/records/{record.Id}", record);
});

// Read all 
app.MapGet("/api/records", (string? category, string? status) =>
{
    var result = records.AsQueryable();
    if (!string.IsNullOrEmpty(category))
        result = result.Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    if (!string.IsNullOrEmpty(status))
        result = result.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(result.ToList());
});

// Read by ID
app.MapGet("/api/records/{id:int}", (int id) =>
{
    var record = records.FirstOrDefault(r => r.Id == id);
    return record is null ? Results.NotFound() : Results.Ok(record);
});

// Update
app.MapPut("/api/records/{id:int}", (int id, Record updated) =>
{
    var record = records.FirstOrDefault(r => r.Id == id);
    if (record is null) return Results.NotFound();
    record.Name = updated.Name;
    record.Category = updated.Category;
    record.Status = updated.Status;
    return Results.NoContent();
});

// Delete
app.MapDelete("/api/records/{id:int}", (int id) =>
{
    var record = records.FirstOrDefault(r => r.Id == id);
    if (record is null) return Results.NotFound();
    records.Remove(record);
    return Results.NoContent();
});


app.Run("http://localhost:5000");
