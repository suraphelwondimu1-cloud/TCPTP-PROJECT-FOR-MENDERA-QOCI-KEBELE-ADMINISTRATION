using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Models;
using MenderaQociDFMS.ViewModels;

namespace MenderaQociDFMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "Staff",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(userList);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email ?? $"{model.UserName}@menderaqoci.gov.et",
                    FullName = model.FullName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    TempData["Success"] = $"User account '{model.UserName}' successfully created.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Staff",
                IsActive = user.IsActive
            };

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Protection: prevent self-deactivation or self-demotion if only admin
            if (currentUser != null && currentUser.Id == user.Id)
            {
                if (!model.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "You cannot deactivate your own logged-in administrator account.");
                    return View(model);
                }
                if (model.Role != "Admin")
                {
                    ModelState.AddModelError(string.Empty, "You cannot remove Admin role from your own account.");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.IsActive = model.IsActive;

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    // Update Role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Reset password if specified
                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    }

                    TempData["Success"] = $"User account '{user.UserName}' updated.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "You cannot deactivate your own logged-in administrator account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"User '{user.UserName}' status changed to {(user.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
