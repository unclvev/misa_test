using AssetManagement.Application.Interfaces;
using AssetManagement.Application.Services;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Repositories;
using Dapper;

// Đăng ký DateOnly type handler cho Dapper ngay từ đầu
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Đăng ký DbConnectionFactory cho Dapper
builder.Services.AddScoped<IDbConnectionFactory>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("MyCnn");
    return new DbConnectionFactory(connectionString ?? throw new InvalidOperationException("Connection string 'MyCnn' not found."));
});

// Đăng ký Repositories (Interface từ Application, Implementation từ Infrastructure)
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IIncreaseVoucherRepository, IncreaseVoucherRepository>();

// Đăng ký Services (Interface và Implementation đều từ Application)
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IIncreaseVoucherService, IncreaseVoucherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must be before UseHttpsRedirection
app.UseCors("AllowFrontend");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
