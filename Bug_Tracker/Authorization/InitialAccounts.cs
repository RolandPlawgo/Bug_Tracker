using Microsoft.AspNetCore.Identity;

namespace Bug_Tracker.Authorization
{
    public class InitialAccounts
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public InitialAccounts(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager; 
        }


        /// <summary>
        /// Checks if the admin account exists and creates it if it doesn't exist.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task EnsureAccountExistsAsync(string password, string email, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                throw new Exception("The role admin doesn't exist.");
            }

            IdentityUser? findUserResult = await _userManager.FindByEmailAsync(email);
            if (findUserResult != null)
            {
                if (!await _userManager.IsInRoleAsync(findUserResult, roleName))
                {
                    await _userManager.AddToRoleAsync(findUserResult, roleName);
                }
                return;
            }

            IdentityUser user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("The user doesn't exist. The pasword was probably too weak.");
            }

            await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}
