using WebApplication1.Data;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string loginType, string username, string password)
        {
            if (loginType == "User")
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == username && u.Password == password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserType", "User");
                    
                }
            }
            else if (loginType == "Clinical")
            {
                var clinical = await _context.Clinicals
                    .FirstOrDefaultAsync(c => c.ClinicalId == username && c.Password == password);

                if (clinical != null)
                {
                    HttpContext.Session.SetString("UserType", "Clinical");
                    HttpContext.Session.SetInt32("ClinicalId", clinical.Id);
                    
                }
            }
            else if (loginType == "Admin")
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

                if (admin != null)
                {
                    HttpContext.Session.SetString("UserType", "Admin");
                    HttpContext.Session.SetInt32("AdminId", admin.Id);
                   
                }
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register (User Self Registration)
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if UserId already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (existingUser != null)
                {
                    ModelState.AddModelError("UserId", "This User ID is already taken");
                    
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Registration successful! Please login.";
                
            }
            return View(user);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}