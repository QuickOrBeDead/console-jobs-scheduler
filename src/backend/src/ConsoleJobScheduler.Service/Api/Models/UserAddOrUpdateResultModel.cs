namespace ConsoleJobScheduler.Service.Api.Models;

using Microsoft.AspNetCore.Identity;

public sealed class UserAddOrUpdateResultModel
{
    private IList<IdentityError>? _errors;

    public int UserId { get; set; }

    public bool Succeeded { get; set; }

    public IList<IdentityError> Errors
    {
        get => _errors ??= new List<IdentityError>();
        set => _errors = value;
    }

    public static UserAddOrUpdateResultModel Success(int userId)
    {
        return new UserAddOrUpdateResultModel {Succeeded = true, UserId = userId};
    }

    public static UserAddOrUpdateResultModel Fail(IEnumerable<IdentityError> errors)
    {
        return new UserAddOrUpdateResultModel { Succeeded = false, Errors = errors.ToList() };
    }
}