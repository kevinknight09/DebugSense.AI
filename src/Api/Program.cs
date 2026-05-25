using DebugSense.Embedding;
using DebugSense.Infrastructure;
using DebugSense.Retrieval;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register our custom RAG Engine Services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<OllamaEmbeddingService>(sp =>
    new OllamaEmbeddingService(sp.GetRequiredService<HttpClient>(), "nomic-embed-text"));
builder.Services.AddSingleton<QdrantVectorStore>(sp =>
    new QdrantVectorStore("localhost", 6334));
builder.Services.AddSingleton<SemanticSearchService>();
builder.Services.AddSingleton<OllamaChatService>(sp =>
    new OllamaChatService(sp.GetRequiredService<HttpClient>(), "phi3"));
builder.Services.AddSingleton<RagOrchestratorService>();

// 1. Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection(); // Removed because it breaks local Angular HTTP requests

// 2. Enable CORS
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
