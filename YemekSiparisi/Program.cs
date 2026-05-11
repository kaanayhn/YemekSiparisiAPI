
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsý (SQL Server)
// Retry özelliði ile baðlantý hatalarýna karþý tekrar deneme yapýlýr
builder.Services.AddDbContext<YemekSiparisi.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// JWT kimlik doðrulama sistemi ekleniyor
// Gelen token’ýn geçerli olup olmadýðý kontrol edilir
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// Controller’lar projeye ekleniyor
builder.Services.AddControllers();

// Swagger için API dokümantasyonu aktif ediliyor
builder.Services.AddEndpointsApiExplorer();

// Swagger güvenlik ayarý (Bearer Token ekleme kýsmý)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Token'ý þu formatta giriniz: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Geliþtirme ortamýnda Swagger açýlýr
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS yönlendirme aktif
app.UseHttpsRedirection();

// Kimlik doðrulama middleware’i
app.UseAuthentication();

// Yetkilendirme middleware’i
app.UseAuthorization();

// Controller endpointlerini aktif eder
app.MapControllers();

// Uygulama baþlarken otomatik admin oluþturma iþlemi
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<YemekSiparisi.Data.AppDbContext>();

    // Sistemde admin yoksa otomatik oluþturulur
    if (!dbContext.Users.Any(u => u.RoleId == 1))
    {
        var adminUser = new YemekSiparisi.Models.User
        {
            Username = "admin",
            Email = "admin@yemeksiparisi.com",

            // Þifre hashlenerek güvenli þekilde saklanýr
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),

            RoleId = 1 // Admin rolü
        };

        dbContext.Users.Add(adminUser);
        dbContext.SaveChanges();
    }
}

// Uygulama çalýþtýrýlýr
app.Run();