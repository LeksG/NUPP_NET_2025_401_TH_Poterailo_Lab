using CounterStrike.Infrastructure;
using CounterStrike.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<CounterStrikeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<Repository<PlayerModel>>();
builder.Services.AddScoped<Repository<WeaponModel>>();
builder.Services.AddScoped<Repository<TeamModel>>();

// CrudService
builder.Services.AddScoped<ICrudServiceAsync<PlayerModel>, CrudServiceAsync<PlayerModel>>();
builder.Services.AddScoped<ICrudServiceAsync<WeaponModel>, CrudServiceAsync<WeaponModel>>();
builder.Services.AddScoped<ICrudServiceAsync<TeamModel>, CrudServiceAsync<TeamModel>>();

// Controllers, Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
