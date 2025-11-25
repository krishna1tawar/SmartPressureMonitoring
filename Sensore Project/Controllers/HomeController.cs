using Microsoft.AspNetCore.Mvc;
using Sensore_Project.Models;
using System.Diagnostics;

namespace Sensore_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Main landing page
        public IActionResult Index()
        {
            return View();
        }

        // Live monitoring dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // Alerts / Anomalies history UI page
        public IActionResult AlertsPage()
        {
            return View();
        }

        // Privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}