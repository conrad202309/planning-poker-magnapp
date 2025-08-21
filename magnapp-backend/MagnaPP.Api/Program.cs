using MagnaPP.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddScoped<ISessionService, MemorySessionService>();
builder.Services.AddHostedService<SessionCleanupService>();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MagnaPP Planning Poker API",
        Version = "v1",
        Description = "REST API for MagnaPP Planning Poker application"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MagnaPP API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
   .WithName("HealthCheck");

app.Run();
