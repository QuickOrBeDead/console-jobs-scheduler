using System.Security.Claims;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Identity;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using ConsoleJobScheduler.Core.Domain.Identity.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Identity;

namespace ConsoleJobScheduler.Core.Application;

public interface IIdentityApplicationService
{
    Task<PagedResult<UserListItem>> ListUsers(int? pageNumber = null);

    Task<UserDetailModel?> GetUserForEdit(int userId);

    Task<List<string>> GetAllRoles();

    Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model);

    Task<SignInResult> Login(LoginModel loginModel);

    Task Logout();

    UserContext? GetUserContext(ClaimsPrincipal user);

    Task AddInitialRolesAndUsers();
}

public sealed class IdentityApplicationService : IIdentityApplicationService
{
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminPassword = "Password";

    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly SignInManager<IdentityUser<int>> _signInManager;

    public IdentityApplicationService(
        IIdentityService identityService,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        SignInManager<IdentityUser<int>> signInManager)
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

    public UserContext? GetUserContext(ClaimsPrincipal user)
    {
        if (!_signInManager.IsSignedIn(user))
        {
            return null;
        }

        return new UserContext
        {
            UserName = GetClaimValue(user, ClaimTypes.Name),
            Roles = GetRoles(user)
        };
    }

    public async Task<PagedResult<UserListItem>> ListUsers(int? pageNumber = null)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await _userRepository.ListUsers(10, pageNumber);
        transactionScope.Complete();
        return result;
    }

    public async Task<UserDetailModel?> GetUserForEdit(int userId)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var user = await _userRepository.FindByIdAsync(userId);
        transactionScope.Complete();
        return user == null ? null : new UserDetailModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = user.Roles
        };
    }

    public async Task<List<string>> GetAllRoles()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await _roleRepository.GetRoles().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadCommitted();
        var result = await _identityService.SaveUser(model);
        transactionScope.Complete();

        return result;
    }

    public async Task AddInitialRolesAndUsers()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadCommitted();
        var roles = new[] { Roles.Admin, Roles.JobEditor, Roles.JobViewer };
        for (var i = 0; i < roles.Length; i++)
        {
            await _roleRepository.AddRole(roles[i]);
        }

        if (!await _userRepository.UserExists(DefaultAdminUserName).ConfigureAwait(false))
        {
            await SaveUser(new UserAddOrUpdateModel { UserName = DefaultAdminUserName, Password = DefaultAdminPassword, Roles = new List<string> { Roles.Admin } }).ConfigureAwait(false);
        }

        transactionScope.Complete();
    }

    private static string? GetClaimValue(ClaimsPrincipal contextUser, string type)
    {
        return contextUser.FindFirstValue(type);
    }

    private static List<string> GetRoles(ClaimsPrincipal contextUser)
    {
        return contextUser.FindAll(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
    }
}