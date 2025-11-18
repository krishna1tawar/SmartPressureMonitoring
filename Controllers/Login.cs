using Microsoft.AspNetCore.Mvc;

namespace Sensore_Project.Controllers
{
    public class Login : Controller
    {
        public IActionResult loginselect()
        {
            return View();
        }
        public IActionResult userlogin()
        {
            return View();
        }
        public IActionResult clinicianlogin()
        {
            return View();
        }
        public IActionResult adminlogin()
        {
            return View();
        }
    }
}
