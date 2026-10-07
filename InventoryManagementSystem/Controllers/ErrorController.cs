using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
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