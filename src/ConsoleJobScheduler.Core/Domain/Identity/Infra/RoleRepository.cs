using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Identity.Infra
{
    public interface IRoleRepository
    {
        Task<List<string>> GetRoles();

        Task AddRole(string role);
    }

    public sealed class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RoleRepository(RoleManager<IdentityRole<int>> roleManager)
        {
            _roleManager = roleManager;
        }

        public Task<List<string>> GetRoles()
        {
            return _roleManager.Roles.Select(x => x.Name!).ToListAsync();
        }

        public async Task AddRole(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
    }
}