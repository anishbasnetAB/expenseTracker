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

        // GET: /Register/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Register/Index.cshtml");
        }

        // POST: /Register/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Register/Index.cshtml", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Instead of signing in immediately, redirect to confirmation page.
                return RedirectToAction("RegistrationConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Views/Register/Index.cshtml", model);
        }

        // GET: /Register/RegistrationConfirmation
        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {
            return View();
        }
    }
}
