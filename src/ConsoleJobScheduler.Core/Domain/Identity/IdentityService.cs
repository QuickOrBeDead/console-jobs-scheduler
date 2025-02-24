using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using ConsoleJobScheduler.Core.Domain.Identity.Model;

namespace ConsoleJobScheduler.Core.Domain.Identity;

public interface IIdentityService
{
    Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model);
}

public sealed class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;

    public IdentityService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model)
    {
        var user = new User(model.Id, model.UserName!, model.Roles);
        if (model.Id > 0)
        {
            var result = await _userRepository.UpdateAsync(user, model.Password);
            return result.Succeeded ? UserAddOrUpdateResultModel.Success(model.Id) : UserAddOrUpdateResultModel.Fail(result.Errors);
        }
        else
        {
            var result = await _userRepository.CreateAsync(user, model.Password!);
            model.Id = user.Id;
            return result.Succeeded ? UserAddOrUpdateResultModel.Success(model.Id) : UserAddOrUpdateResultModel.Fail(result.Errors);
        }
    }
}