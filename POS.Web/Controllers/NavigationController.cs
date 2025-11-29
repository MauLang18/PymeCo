using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.Web.Controllers
{
    [Authorize] // Requiere login para todo
    public class NavigationController : Controller
    {
        private readonly ILogger<NavigationController> _logger;

        public NavigationController(ILogger<NavigationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Dashboard - Solo Admin y Vendedor
        /// </summary>
        [Authorize(Roles = "Admin,Vendedor")]
        [HttpGet]
        public IActionResult DashboardView()
        {
            _logger.LogInformation("DashboardView accessed by {User}", User.Identity?.Name);
            return View();
        }

        /// <summary>
        /// Pedidos - Admin y Cajero
        /// </summary>
        [Authorize(Roles = "Admin,Cajero")]
        [HttpGet]
        public IActionResult OrderView()
        {
            _logger.LogInformation("OrderView accessed by {User}", User.Identity?.Name);
            return View();
        }

        /// <summary>
        /// Inventario - Solo Admin
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult InventarioView()
        {
            _logger.LogInformation("InventarioView accessed by {User}", User.Identity?.Name);
            return View();
        }
    }
}