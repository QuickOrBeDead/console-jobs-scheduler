using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobPackageRepository
{
    Task<JobPackage?> GetByName(string name);

    Task SavePackage(JobPackage jobPackage);

    Task<List<string>> GetAllPackageNames();

    Task<PackageDetailsModel?> GetPackageDetails(string name);

    Task<PagedResult<PackageListItemModel>> ListPackages(int pageSize = 10, int page = 1);
}