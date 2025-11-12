using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Web.ViewModels;

namespace POS.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserService userService,
            ILogger<AuthController> logger
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} signed in", model.Email);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} locked out", model.Email);
                ModelState.AddModelError(
                    string.Empty,
                    "Tu cuenta está bloqueada por 5 minutos debido a múltiples intentos fallidos."
                );
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var identityUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
            };

            var createIdentity = await _userManager.CreateAsync(identityUser, model.Password);
            if (!createIdentity.Succeeded)
            {
                foreach (var error in createIdentity.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            try
            {
                var usuario = new Usuario
                {
                    Nombre = model.FullName,
                    EstadoUsuario = "Activo",
                    RolId = null,
                };

                await _userService.CreateAsync(usuario);

                await _userManager.AddToRoleAsync(identityUser, "Cajero");

                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                _logger.LogInformation(
                    "User {Email} registered and mirrored into Usuario",
                    model.Email
                );
                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to mirror user into Usuario. Rolling back Identity user for {Email}",
                    model.Email
                );
                await _userManager.DeleteAsync(identityUser);
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo completar el registro. Intenta nuevamente."
                );
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

