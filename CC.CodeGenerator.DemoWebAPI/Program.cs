var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

CC.CodeGenerator.AutoDI.AddServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
