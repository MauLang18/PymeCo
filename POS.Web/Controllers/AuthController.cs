using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POS.Domain.Entities;
using POS.Web.ViewModels;

namespace POS.Web.Controllers;

/// <summary>
/// Controlador de autenticación (Login, Registro, Logout)
/// </summary>
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AuthController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Mostrar formulario de login
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Permite acceso sin autenticación
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Procesar login
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Intentar login
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true // Bloquea después de 5 intentos fallidos
        );

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario {Email} inició sesión", model.Email);

            // Redireccionar a la URL original o al home
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Usuario {Email} bloqueado por intentos fallidos", model.Email);
            ModelState.AddModelError(string.Empty, "Tu cuenta está bloqueada por 5 minutos debido a múltiples intentos fallidos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
        return View(model);
    }

    /// <summary>
    /// Mostrar formulario de registro
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Signup()
    {
        return View();
    }

    /// <summary>
    /// Procesar registro de nuevo usuario
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Crear nuevo usuario
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true // En producción esto debería ser false hasta confirmar email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("Nuevo usuario registrado: {Email}", model.Email);

            // Asignar rol por defecto (Cajero)
            await _userManager.AddToRoleAsync(user, "Cajero");

            // Login automático después del registro
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // Si hay errores, mostrarlos
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// Cerrar sesión
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuario cerró sesión");
        return RedirectToAction("Login", "Auth");
    }

    /// <summary>
    /// Página de acceso denegado
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}