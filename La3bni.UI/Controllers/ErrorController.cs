using Microsoft.AspNetCore.Mvc;

namespace La3bni.UI.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorTitle = " 404 Not found!";
                    ViewBag.ErrorMessage = $"Sorry, the resources that you requested could not be found! " +
                        $"Please register to be able to use our services.";
                    break;
                default: //500 Enternal server error
                    ViewBag.ErrorMessage = $"an error occurred while processing your request. " +
                     $"The support team was notified and we are working on the fix.";
                    break;
            }
            return View("Error");
        }
    }
}
