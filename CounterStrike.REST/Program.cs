using CounterStrike.Infrastructure;
using CounterStrike.Infrastructure.Identity;
using CounterStrike.Infrastructure.Models;
using CounterStrike.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// ----------------------
// 1. Database
// ----------------------
var conn = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(conn))
{
    conn = "Data Source=new_counterstrike.db";
    builder.Configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value = conn;
}

services.AddDbContext<CounterStrikeContext>(options =>
{
    if (conn.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(conn);
    else
        options.UseSqlServer(conn);
});

// ----------------------
// 2. Identity
// ----------------------
services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequireDigit = true;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<CounterStrikeContext>()
.AddDefaultTokenProviders();

// ----------------------
// 3. JWT Auth
// ----------------------
var jwt = configuration.GetSection("JwtSettings");
var secret = jwt.GetValue<string>("Secret") ??
             "VeryLongSecretKey_ChangeThisToSomethingSecure_AtLeast32Chars";

var key = Encoding.UTF8.GetBytes(secret);

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.GetValue<string>("Issuer") ?? "CounterStrikeApi",
        ValidateAudience = true,
        ValidAudience = jwt.GetValue<string>("Audience") ?? "CounterStrikeClients",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

// ----------------------
// 4. Swagger + JWT support
// ----------------------
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CounterStrike API", Version = "v1" });

    // 🔥 Додаємо можливість вводити JWT токен
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Example: Bearer {your token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

// ----------------------
// 5. Services
// ----------------------
services.AddScoped<Repository<PlayerModel>>();
services.AddScoped<Repository<WeaponModel>>();
services.AddScoped<Repository<TeamModel>>();

services.AddScoped<ICrudServiceAsync<PlayerModel>, CrudServiceAsync<PlayerModel>>();
services.AddScoped<ICrudServiceAsync<WeaponModel>, CrudServiceAsync<WeaponModel>>();
services.AddScoped<ICrudServiceAsync<TeamModel>, CrudServiceAsync<TeamModel>>();

services.AddControllers();

var app = builder.Build();

// ----------------------
// 6. Migrate DB + Seed admin
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider;
    var db = svc.GetRequiredService<CounterStrikeContext>();
    db.Database.Migrate();

    await RoleSeeder.SeedRolesAndAdminAsync(svc, configuration);
}

// ----------------------
// 7. Middleware
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // 🔥 must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
