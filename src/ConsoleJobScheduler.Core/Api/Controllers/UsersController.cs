
using System.Net.Mime;
using System.Transactions;

using ConsoleJobScheduler.Core.Api.Models;
using ConsoleJobScheduler.Core.Infrastructure.Data;
using ConsoleJobScheduler.Core.Infrastructure.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly IdentityManagementDbContext _context;

    private readonly UserManager<IdentityUser<int>> _userManager;

    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UsersController(IdentityManagementDbContext context, UserManager<IdentityUser<int>> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<PagedResult<UserListItemModel>> Get(int? pageNumber = null)
    {
        const int PageSize = 10;
        var page = pageNumber ?? 1;

        var usersQuery = _context.Users;
        var totalCount = usersQuery.DeferredCount().FutureValue();
        var items = usersQuery.Select(
            user => new UserListItemModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = string.Join(
                                ", ",
                                from userRole in _context.UserRoles
                                join role in _context.Roles on userRole.RoleId equals role.Id
                                where userRole.UserId == user.Id
                                select role.Name)
            }).OrderBy(x => x.UserName).Skip((page - 1) * PageSize).Take(PageSize).Future();

        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var result = new PagedResult<UserListItemModel>(await items.ToListAsync(), PageSize, page, await totalCount.ValueAsync());
        transactionScope.Complete();
        return result;
    }

    [HttpGet("GetUser/{userId:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int userId)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var userDetail = await _context.Users.Where(x => x.Id == userId).Select(
                             user => new UserDetailModel
                             {
                                 UserName = user.UserName,
                                 Roles = (from userRole in _context.UserRoles
                                          join role in _context.Roles on userRole.RoleId equals role.Id
                                          where userRole.UserId == user.Id
                                          select role.Name).ToList()
                             }).SingleOrDefaultAsync();
        transactionScope.Complete();
        if (userDetail == null)
        {
            return NotFound();
        }

        return Ok(userDetail);
    }

    [HttpGet("GetRoles")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<List<string?>> GetRoles()
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
        transactionScope.Complete();
        return result;
    }

    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserAddOrUpdateResultModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public async Task<IActionResult> Post([FromBody] UserAddOrUpdateModel model)
    {
        if (model.Roles.Count == 0)
        {
            ModelState.AddModelError(nameof(model.Roles), "The Roles field is required.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        if (model.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "The Password field is required.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var user = new IdentityUser<int>(model.UserName!);
            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
            {
                return Ok(UserAddOrUpdateResultModel.Fail(result.Errors));
            }

            result = await _userManager.AddToRolesAsync(user, model.Roles);
            if (result.Succeeded)
            {
                transactionScope.Complete();
            }

            return Ok(UserAddOrUpdateResultModel.Success(user.Id));
        }
        else
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return Ok(UserAddOrUpdateResultModel.Fail(result.Errors));
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var identityResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!identityResult.Succeeded)
            {
                return Ok(UserAddOrUpdateResultModel.Fail(identityResult.Errors));
            }

            identityResult = await _userManager.AddToRolesAsync(user, model.Roles);
            if (!identityResult.Succeeded)
            {
                return Ok(UserAddOrUpdateResultModel.Fail(identityResult.Errors));
            }

            transactionScope.Complete();
            return Ok(UserAddOrUpdateResultModel.Success(user.Id));
        }
    }
}