using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Users.Controllers
{
    public class RoleAdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleAdminController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public ViewResult Index() => View(_roleManager.Roles);

        public ViewResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create([Required] string name)
        {
            if (ModelState.IsValid)
            {
                var identityResult = await _roleManager.CreateAsync(new IdentityRole(name));

                if (identityResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                AddErrorsFromResult(identityResult);
            }

            return View(name);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null)
            {
                var identityResult = await _roleManager.DeleteAsync(role);

                if (identityResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                AddErrorsFromResult(identityResult);
            }
            else
            {
                ModelState.AddModelError("", "No role found");
            }

            return View("Index", _roleManager.Roles);
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
