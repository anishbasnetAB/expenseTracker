using expenseTracker.Models; // For ApplicationUser
using expenseTracker.ViewModels; // For RegistrationViewModel
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace expenseTracker.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterController(UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Register
        [HttpGet]
        public IActionResult Index()
        {
            // Explicitly specify the full path to Index.cshtml
            return View("~/Views/Register/Index.cshtml");
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationViewModel model)
        {
            // 1. Check model validity
            if (!ModelState.IsValid)
            {
                // Return the same view with validation errors
                return View("~/Views/Register/Index.cshtml", model);
            }

            // 2. Create a new ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            // 3. Attempt to create the user
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // 4. (Optional) sign them in immediately
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // 5. If creation failed, display the errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Return the same view so user sees error messages
            return View("~/Views/Register/Index.cshtml", model);
        }
    }
}
