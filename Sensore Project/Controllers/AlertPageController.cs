using Microsoft.AspNetCore.Mvc;

namespace Sensore_Project.Controllers
{
    /// <summary>
    /// MVC Controller for the Alerts page UI.
    /// Separate from the API AlertsController to avoid route conflicts.
    /// </summary>
    public class AlertPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
