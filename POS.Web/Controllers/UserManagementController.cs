using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Web.ViewModels;

namespace POS.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Solo Admin puede gestionar usuarios
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: UserManagement/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "Sin Rol",
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return View(userViewModels);
        }

        // GET: UserManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await GetRolesSelectList();
            return View();
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesSelectList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                _logger.LogInformation("Usuario {Email} creado con rol {Role}", model.Email, model.Role);
                TempData["SuccessMessage"] = $"Usuario {model.FullName} creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = await GetRolesSelectList();
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                CurrentRole = roles.FirstOrDefault() ?? "Sin Rol",
                NewRole = roles.FirstOrDefault() ?? "",
                IsActive = user.IsActive
            };

            ViewBag.Roles = await GetRolesSelectList();
            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetRolesSelectList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Actualizar información básica
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                ViewBag.Roles = await GetRolesSelectList();
                return View(model);
            }

            // Cambiar rol si es diferente
            if (model.CurrentRole != model.NewRole)
            {
                if (!string.IsNullOrEmpty(model.CurrentRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, model.CurrentRole);
                }
                await _userManager.AddToRoleAsync(user, model.NewRole);
                _logger.LogInformation("Rol de {Email} cambiado de {OldRole} a {NewRole}",
                    user.Email, model.CurrentRole, model.NewRole);
            }

            // Cambiar contraseña si se proporcionó una nueva
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.Roles = await GetRolesSelectList();
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = $"Usuario {model.FullName} actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserManagementViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Sin Rol",
                IsActive = user.IsActive
            };

            return View(model);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // No permitir eliminar al propio usuario
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == user.Id)
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propia cuenta";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogWarning("Usuario {Email} eliminado por {Admin}", user.Email, currentUser?.Email);
                TempData["SuccessMessage"] = $"Usuario {user.FullName} eliminado exitosamente";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper: Obtener lista de roles
        private async Task<SelectList> GetRolesSelectList()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return new SelectList(roles);
        }
    }
}