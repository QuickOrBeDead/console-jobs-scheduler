
using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly IIdentityApplicationService _identityApplicationService;

    public UsersController(IIdentityApplicationService identityApplicationService)
    {
        _identityApplicationService = identityApplicationService;
    }

    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<PagedResult<UserListItemModel>> Get(int? pageNumber = null)
    {
        return _identityApplicationService.ListUsers(pageNumber);
    }

    [HttpGet("GetUser/{userId:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int userId)
    {
        var userDetail = await _identityApplicationService.GetUserDetail(userId);
        return userDetail == null ? NotFound() : Ok(userDetail);
    }

    [HttpGet("GetRoles")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<List<string>> GetRoles()
    {
        return _identityApplicationService.GetRoles();
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

        if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "The Password field is required.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var result = await _identityApplicationService.SaveUser(model);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}