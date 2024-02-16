using System.Globalization;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Identity.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Domain.Identity.Infra;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);

    Task<IdentityResult> UpdateAsync(User user, string? password);

    Task<IdentityResult> CreateAsync(User user, string password);

    Task<PagedResult<UserListItemModel>> ListUsers(int pageSize = 10, int? pageNumber = null);
    Task<bool> UserExists(string userName);
}

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly IdentityManagementDbContext _dbContext;

    public UserRepository(UserManager<IdentityUser<int>> userManager, IdentityManagementDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        var identityUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (identityUser == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(identityUser).ConfigureAwait(false);

        return new User(identityUser.Id, identityUser.UserName!, roles);
    }

    public Task<bool> UserExists(string userName)
    {
       return _userManager.Users.AnyAsync(x => x.UserName == userName);
    }

    public async Task<IdentityResult> UpdateAsync(User user, string? password)
    {
        var identityUser = new IdentityUser<int>(user.UserName);
        var result = await _userManager.UpdateAsync(identityUser).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            identityUser.PasswordHash = _userManager.PasswordHasher.HashPassword(identityUser, password);

            result = await _userManager.UpdateAsync(identityUser);
            if (!result.Succeeded)
            {
                return result;
            }
        }

        result = await _userManager.RemoveFromRolesAsync(identityUser, user.Roles).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        return await _userManager.AddToRolesAsync(identityUser, user.Roles).ConfigureAwait(false);
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var identityUser = new IdentityUser<int>(user.UserName);
        var result =  await _userManager.CreateAsync(identityUser, password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        return await _userManager.AddToRolesAsync(identityUser, user.Roles).ConfigureAwait(false);
    }

    public async Task<PagedResult<UserListItemModel>> ListUsers(int pageSize = 10, int? pageNumber = null)
    {
        var page = pageNumber ?? 1;

        var usersQuery = _dbContext.Users;
        var totalCount = usersQuery.DeferredCount().FutureValue();
        var items = usersQuery.Select(
            user => new UserListItemModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = string.Join(
                    ", ",
                    from userRole in _dbContext.UserRoles
                    join role in _dbContext.Roles on userRole.RoleId equals role.Id
                    where userRole.UserId == user.Id
                    select role.Name)
            }).OrderBy(x => x.UserName).Skip((page - 1) * pageSize).Take(pageSize).Future();

        return new PagedResult<UserListItemModel>(await items.ToListAsync(), pageSize, page, await totalCount.ValueAsync());
    }
}