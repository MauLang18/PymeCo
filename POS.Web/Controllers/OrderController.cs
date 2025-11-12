using Microsoft.AspNetCore.Mvc;

namespace POS.Web.Controllers;

public class OrderController : Controller
{
    [HttpGet]
    public IActionResult Index() => View("~/Views/Navigation/OrderView.cshtml");
}
