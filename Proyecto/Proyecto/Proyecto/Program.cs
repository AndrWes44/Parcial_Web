using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;            // Ajusta este namespace si tu proyecto se llama distinto
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1) Registrar el AppDbContext usando "DefaultConnection" de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 2) Registrar MVC, Sesión y Caché en memoria
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

// 3) Registrar configuración SMTP
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp")
);
builder.Services.AddTransient<EmailService>();

// 4) Registrar autenticación por Cookie
builder.Services.AddAuthentication("MiCookieAuth")
    .AddCookie("MiCookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        // Puedes ajustar otros parámetros si lo necesitas
    });

var app = builder.Build();

// 5) Middleware de manejo de excepciones y HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6) Middleware de Sesión, Autenticación y Autorización
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 7) Rutas para controllers + views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
