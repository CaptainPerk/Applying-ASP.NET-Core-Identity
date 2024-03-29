﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Users.Models;

namespace Users.Controllers
{
    [Authorize(Roles = "Admins")]
    public class RoleAdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleAdminController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
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

        
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var members = new List<AppUser>();
            var nonMembers = new List<AppUser>();

            foreach (var user in _userManager.Users)
            {
                var userList = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                userList.Add(user);
            }

            return View(new RoleEditModel {Role = role, Members = members, NonMembers = nonMembers});
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RoleModificationModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[] {})
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        var identityResult = await _userManager.AddToRoleAsync(user, model.RoleName);

                        if (!identityResult.Succeeded)
                        {
                            AddErrorsFromResult(identityResult);
                        }
                    }
                }

                foreach (var userId in model.IdsToDelete ?? new string[] {})
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        var identityResult = await _userManager.RemoveFromRoleAsync(user, model.RoleName);

                        if (!identityResult.Succeeded)
                        {
                            AddErrorsFromResult(identityResult);
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return await Edit(model.RoleId);
            }
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
