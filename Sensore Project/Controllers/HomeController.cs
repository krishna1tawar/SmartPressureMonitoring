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

        // Default Home Page
        public IActionResult Index()
        {
            return View();
        }

        // Live Dashboard Page
        public IActionResult Dashboard()
        {
            return View();
        }

        // Alerts Page (Anomaly History)
        public IActionResult Alerts()
        {
            return View();
        }

        // Privacy Page
        public IActionResult Privacy()
        {
            return View();
        }

        // Error Page
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