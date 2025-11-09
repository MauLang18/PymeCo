using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models;

namespace POS.Web.Controllers
{
    [Route("")]
    public class ErrorsController : Controller
    {
        private readonly ILogger<ErrorsController> _logger;

        public ErrorsController(ILogger<ErrorsController> logger) => _logger = logger;

        // Handles non-exception status codes (e.g., 404, 403)
        [Route("errors/{code:int}")]
        public IActionResult StatusCodeHandler(int code)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            _logger.LogWarning(
                "HTTP {StatusCode} for {Path}{Query}",
                code,
                feature?.OriginalPath,
                feature?.OriginalQueryString
            );

            return code switch
            {
                404 => View("404"),
                403 => View("403"),
                _ => View("GenericStatus", code),
            };
        }

        // Handles unhandled exceptions (500)
        [Route("error")]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var traceId = HttpContext.TraceIdentifier;

            _logger.LogError(
                feature?.Error,
                "Unhandled exception at {Path}. TraceId: {TraceId}",
                feature?.Path,
                traceId
            );

            return View("500", new ErrorViewModel { TraceId = traceId, Path = feature?.Path });
        }
    }
}
