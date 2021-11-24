using Microsoft.OpenApi.Models;
using Orleans;
using Orleans.WebApi.Abstractions;
using Sample.Gateway;
using Sample.Gateway.HostServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ClusterClientHostedService>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetService<ClusterClientHostedService>());
builder.Services.AddSingleton<IClusterClient>(sp => sp.GetService<ClusterClientHostedService>().Client);
builder.Services.AddSingleton<IGrainFactory>(sp => sp.GetService<ClusterClientHostedService>().Client);
builder.Services.AddSingleton<IClusterFactory, ClusterFactory>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Sample.Gateway.xml");
    c.IncludeXmlComments(filePath, true);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample.Gateway", Version = "v1" });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();