using Microsoft.AspNetCore.Identity;

namespace Bug_Tracker.Authorization
{
    public class Roles
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public Roles(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        /// <summary>
        /// Checks if the specified role exists and creates the role if it doesnt exist.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public async Task EnsureRoleExistsAsync(string roleName)
        {
            if (_roleManager == null)
            {
                throw new Exception("Role manager is null");
            }
            if (await _roleManager.FindByNameAsync(roleName) != null)
            {
                return;
            }
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
