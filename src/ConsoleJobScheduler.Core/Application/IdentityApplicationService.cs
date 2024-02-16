using System.Security.Claims;
using System.Transactions;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Identity;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Identity;

namespace ConsoleJobScheduler.Core.Application;

public interface IIdentityApplicationService
{
    Task<PagedResult<UserListItemModel>> ListUsers(int? pageNumber = null);

    Task<UserDetailModel?> GetUserDetail(int userId);

    Task<List<string>> GetRoles();

    Task<UserAddOrUpdateResultModel?> SaveUser(UserAddOrUpdateModel model);

    Task<SignInResult> Login(LoginModel loginModel);

    Task Logout();

    UserModel? GetUser(ClaimsPrincipal user);

    Task AddRole(string roleName);
    Task SaveInitialData();
}

public sealed class IdentityApplicationService : IIdentityApplicationService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly SignInManager<IdentityUser<int>> _signInManager;

    public IdentityApplicationService(IIdentityService identityService, IUserRepository userRepository, IRoleRepository roleRepository, SignInManager<IdentityUser<int>> signInManager)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _signInManager = signInManager;
    }

    public Task<SignInResult> Login(LoginModel loginModel)
    {
        return _signInManager.PasswordSignInAsync(loginModel.UserName, loginModel.Password, loginModel.RememberMe, true);
    }

    public Task Logout()
    {
        return _signInManager.SignOutAsync();
    }

    public UserModel? GetUser(ClaimsPrincipal user)
    {
        if (!_signInManager.IsSignedIn(user))
        {
            return null;
        }

        return new UserModel
        {
            UserName = GetClaimValue(user, ClaimTypes.Name),
            Roles = GetRoles(user)
        };
    }

    public async Task<PagedResult<UserListItemModel>> ListUsers(int? pageNumber = null)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _userRepository.ListUsers(10, pageNumber);
        transactionScope.Complete();
        return result;
    }

    public async Task<UserDetailModel?> GetUserDetail(int userId)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var user = await _userRepository.FindByIdAsync(userId);
        transactionScope.Complete();
        return user == null ? null : new UserDetailModel
        {
            UserName = user.UserName,
            Roles = user.Roles
        };
    }

    public async Task<List<string>> GetRoles()
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _roleRepository.GetRoles().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<UserAddOrUpdateResultModel?> SaveUser(UserAddOrUpdateModel model)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var result = await _identityService.SaveUser(model);
        transactionScope.Complete();

        return result;
    }

    public async Task AddRole(string roleName)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await _roleRepository.AddRole(roleName);
        transactionScope.Complete();
    }

    public async Task SaveInitialData()
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var roles = new[] { Roles.Admin, Roles.JobEditor, Roles.JobViewer };
        for (var i = 0; i < roles.Length; i++)
        {
            var role = roles[i];
            await AddRole(role).ConfigureAwait(false);
        }

        if (!await _userRepository.UserExists("admin").ConfigureAwait(false))
        {
            await SaveUser(new UserAddOrUpdateModel { UserName = "admin", Password = "Password", Roles = new List<string> { Roles.Admin } })
                .ConfigureAwait(false);
        }

        transactionScope.Complete();
    }

    private static string? GetClaimValue(ClaimsPrincipal contextUser, string type)
    {
        return contextUser.Claims.FirstOrDefault(x => x.Type == type)?.Value;
    }

    private static List<string> GetRoles(ClaimsPrincipal contextUser)
    {
        return contextUser.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
    }
}