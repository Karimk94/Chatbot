using Application.Interfaces;
using Infrastructure.Ai;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

// --- Service Configuration ---

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:7108", "null")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddSingleton(sp => new GraphServiceClient(sp.GetRequiredService<IAuthenticationProvider>()));

builder.Services.AddSingleton<IAiAnalysisService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    string relativePath = config["LocalAi:ModelPath"] ?? throw new InvalidOperationException("LocalAi:ModelPath is not configured.");
    string absolutePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, relativePath));
    return new LocalAiAnalysisService(absolutePath);
});

var app = builder.Build();

// --- Middleware Pipeline ---

if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors(MyAllowSpecificOrigins);

app.Run();