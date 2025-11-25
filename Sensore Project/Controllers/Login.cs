using Microsoft.AspNetCore.Mvc;

namespace Sensore_Project.Controllers
{
    public class LoginController : Controller
    {
        // Landing page where user selects their login role
        public IActionResult Select()
        {
            return View("loginselect");
        }

        // User login page
        public IActionResult UserLogin()
        {
            return View("userlogin");
        }

        // Clinician login page
        public IActionResult ClinicianLogin()
        {
            return View("clinicianlogin");
        }

        // Admin login page
        public IActionResult AdminLogin()
        {
            return View("adminlogin");
        }
    }
}
