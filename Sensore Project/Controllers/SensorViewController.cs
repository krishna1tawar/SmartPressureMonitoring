using Microsoft.AspNetCore.Mvc;

namespace Sensore_Project.Controllers
{
    public class SensorViewController : Controller
    {
        public IActionResult History()
        {
            return View();
        }
    }
}