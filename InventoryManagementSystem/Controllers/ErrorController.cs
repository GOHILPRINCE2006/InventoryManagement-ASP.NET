using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
            // ⭐ Log the exception details
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                _logger.LogError(exceptionFeature.Error, "Unhandled exception");
            }

            return View();
        }

        [Route("Error/{code:int}")]
        public IActionResult Generic(int code)
        {
            ViewBag.Code = code;
            return View();
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View("ServerError");
        }
    }
}