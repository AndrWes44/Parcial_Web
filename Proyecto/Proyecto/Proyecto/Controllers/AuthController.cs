using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using System.Security.Claims;

namespace Proyecto.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            var usuario = _context.Usuario
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Correo == correo && u.Contrasena == contrasena);

            if (usuario != null)
            {
                // Crear los claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Usuario")
        };

                var claimsIdentity = new ClaimsIdentity(claims, "MiCookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Iniciar sesión con cookie
                await HttpContext.SignInAsync("MiCookieAuth", claimsPrincipal);

                // Guardar también en sesión si lo deseas
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
                HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "");

                // Redirigir según el rol
                switch (usuario.Rol?.Nombre)
                {
                    case "Administrador":
                        return RedirectToAction("Index", "Admin");
                    case "Técnico":
                        return RedirectToAction("Index", "Tecnico");
                    case "Cliente":
                        return RedirectToAction("Index", "Cliente");
                    default:
                        return RedirectToAction("Login");
                }
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MiCookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
