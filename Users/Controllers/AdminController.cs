using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Controllers
{
    [Authorize(Roles = "Admins")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserValidator<AppUser> _userValidator;
        private readonly IPasswordValidator<AppUser> _passwordValidator;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AdminController(
            UserManager<AppUser> userManager, 
            IUserValidator<AppUser> userValidator, 
            IPasswordValidator<AppUser> passwordValidator, 
            IPasswordHasher<AppUser> passwordHasher)
        {
            _userManager = userManager;
            _userValidator = userValidator;
            _passwordValidator = passwordValidator;
            _passwordHasher = passwordHasher;
        }

        public ViewResult Index() => View(_userManager.Users);

        public ViewResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser{ UserName = model.Name, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var identityResult = await _userManager.DeleteAsync(user);

                if (identityResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                AddErrorsFromResult(identityResult);
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }

            return View("Index", _userManager.Users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string email, string password)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.Email = email;
                var validateEmail = await _userValidator.ValidateAsync(_userManager, user);

                if (!validateEmail.Succeeded)
                {
                    AddErrorsFromResult(validateEmail);
                }

                IdentityResult validatePassword = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validatePassword = await _passwordValidator.ValidateAsync(_userManager, user, password);

                    if (validatePassword.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validatePassword);
                    }
                }

                if (validateEmail.Succeeded && validatePassword == null || validateEmail.Succeeded && password != string.Empty && validatePassword.Succeeded)
                {
                    var updateUser = await _userManager.UpdateAsync(user);

                    if (updateUser.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }

                    AddErrorsFromResult(updateUser);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }

            return View(user);
        }

            private void AddErrorsFromResult(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
