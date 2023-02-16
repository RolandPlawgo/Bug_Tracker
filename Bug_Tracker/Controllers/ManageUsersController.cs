using Bug_Tracker.Authorization;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracker.Controllers
{
    public class ManageUsersController : Controller
    {
        private readonly ILogger<ManageUsersController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;
        public ManageUsersController(ILogger<ManageUsersController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
        }

        //GET: ManageUsers
        [HttpGet]
        public async Task<ActionResult> Index(string searchString, string roleFilter)
        {
            _logger.LogInformation("GET: ManageUsers");

            ViewData["SearchString"] = searchString;
            ViewData["RoleFilter"] = roleFilter;

            ViewData["RoleNames"] = await GetRoleNamesAsync();

            List<ManageUsersViewModel> model = new List<ManageUsersViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                ManageUsersViewModel item = new ManageUsersViewModel();
                item.Id = user.Id;
                item.Email = user.Email;
                item.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() == null ? Constants.NoRole : (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                
                model.Add(item);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                model.RemoveAll(m => !m.Email.Contains(searchString));
                //foreach (var item in model)
                //{
                //    if (!item.Email.Contains(searchString))
                //    {
                //        model.Remove(item);
                //    }
                //}
            }
            if (!string.IsNullOrEmpty(roleFilter))
            {
                model.RemoveAll(m => m.Role != roleFilter);
                //foreach (var item in model)
                //{
                //    if (item.Role != roleFilter)
                //    {
                //        model.Remove(item);
                //    }
                //}
            }

            return View(model);
        }

        //GET: ManageUsers/Edit/
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            _logger.LogInformation("GET: ManageUsers/Edit/{id}", id);

            ViewData["Roles"] = await GetRoleNamesAsync();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ManageUsersViewModel model = new ManageUsersViewModel();
            model.Id = user.Id;
            model.Email = user.Email;
            model.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() == null ? Constants.NoRole : (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return View(model);
        }

        //POST: ManageUsers/Edit/
        [HttpPost]
        public async Task<ActionResult> Edit(string id, [Bind("UserName,Email,Role")] ManageUsersViewModel model)
        {
            _logger.LogInformation("POST: ManageUsers/Edit/{id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName = model.Email;
            user.Email = model.Email;
            var usersRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() == null ? Constants.NoRole : (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (model.Role != usersRole)
            {
                if (usersRole != Constants.NoRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, usersRole);
                }
                if (model.Role != Constants.NoRole)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        //GET: ManageUsers/Delete/
        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            _logger.LogInformation("POST: ManageUsers/Delete/{id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ManageUsersViewModel model = new ManageUsersViewModel();
            model.Id = user.Id;
            model.Email = user.Email;
            model.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() == null ? Constants.NoRole : (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return View(model);
        }

        //POST: ManageUsers/Delete/
        [HttpPost]
        public async Task<ActionResult> Delete(string id, IFormCollection formCollection)
        {
            _logger.LogInformation("POST: ManageUsers/Delete/{id}", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }



        private async Task<List<string>> GetRoleNamesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            List<string> roleNames = new List<string>();
            foreach (var role in roles)
            {
                roleNames.Add(role.Name);
            }
            roleNames.Add(Constants.NoRole);

            return roleNames;
        }
    }
}
