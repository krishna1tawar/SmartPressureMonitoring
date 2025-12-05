using Microsoft.AspNetCore.Mvc;

namespace Sensore_Project.Controllers
{
    /// <summary>
    /// MVC Controller for sensor data views.
    /// </summary>
    public class SensorViewController : Controller
    {
        /// <summary>
        /// Displays the sensor data history page.
        /// </summary>
        public IActionResult History()
        {
            return View();
        }
    }
}