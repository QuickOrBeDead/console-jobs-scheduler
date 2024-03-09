using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using ConsoleJobScheduler.Core.Domain.Identity.Model;

namespace ConsoleJobScheduler.Core.Domain.Identity;

public interface IIdentityService
{
    Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model);
}

public sealed class IdentityService(IUserRepository userRepository) : IIdentityService
{
    public async Task<UserAddOrUpdateResultModel> SaveUser(UserAddOrUpdateModel model)
    {
        var user = new User(model.Id, model.UserName!, model.Roles);
        if (model.Id > 0)
        {
            var result = await userRepository.UpdateAsync(user, model.Password);
            return result.Succeeded ? UserAddOrUpdateResultModel.Success(model.Id) : UserAddOrUpdateResultModel.Fail(result.Errors);
        }
        else
        {
            var result = await userRepository.CreateAsync(user, model.Password!);
            model.Id = user.Id;
            return result.Succeeded ? UserAddOrUpdateResultModel.Success(model.Id) : UserAddOrUpdateResultModel.Fail(result.Errors);
        }
    }
}