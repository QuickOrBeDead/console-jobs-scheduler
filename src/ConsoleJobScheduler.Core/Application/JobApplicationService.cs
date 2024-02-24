using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz;

namespace ConsoleJobScheduler.Core.Application;

public interface IJobApplicationService
{
    Task<byte[]?> GetJobRunAttachmentContent(long id);

    Task SavePackage(string packageName, byte[] content);

    Task<List<string>> GetAllPackageNames();

    Task<PackageDetails?> GetPackageDetails(string name);

    Task<PagedResult<PackageListItem>> ListPackages(int pageSize = 10, int page = 1);

    Task<PagedResult<JobListItem>> ListJobs(int? pageNumber = null);

    Task<JobDetail?> GetJobDetail(string group, string name);

    Task AddOrUpdateJob(JobAddOrUpdateModel model);

    Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id);
}

public sealed class JobApplicationService : IJobApplicationService
{
    private readonly IJobRunService _jobRunService;
    private readonly ISchedulerService _schedulerService;
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobRunAttachmentRepository _jobRunAttachmentRepository;
    private readonly IJobPackageRepository _jobPackageRepository;

    public JobApplicationService(
        IJobRunRepository jobRunRepository,
        IJobRunAttachmentRepository jobRunAttachmentRepository,
        IJobPackageRepository jobPackageRepository,
        IJobRunService jobRunService,
        ISchedulerService schedulerService)
    {
        _jobRunService = jobRunService;
        _schedulerService = schedulerService;
        _jobRunRepository = jobRunRepository;
        _jobRunAttachmentRepository = jobRunAttachmentRepository;
        _jobPackageRepository = jobPackageRepository;
    }

    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        var logs = await _jobRunRepository.GetJobRunLogs(id).ConfigureAwait(false);
        var attachments = await _jobRunAttachmentRepository.GetJobRunAttachments(id).ConfigureAwait(false);

        return new JobExecutionDetailModel(logs, attachments);
    }

    public Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        return _jobRunAttachmentRepository.GetJobRunAttachmentContent(id);
    }

    public Task SavePackage(string packageName, byte[] content)
    {
        return _jobRunService.SavePackage(packageName, content);
    }

    public Task<List<string>> GetAllPackageNames()
    {
        return _jobPackageRepository.GetAllPackageNames();
    }

    public Task<PackageDetails?> GetPackageDetails(string name)
    {
        return _jobPackageRepository.GetPackageDetails(name);
    }

    public Task<PagedResult<PackageListItem>> ListPackages(int pageSize = 10, int page = 1)
    {
        return _jobPackageRepository.ListPackages(pageSize, page);
    }

    public Task<PagedResult<JobListItem>> ListJobs(int? pageNumber = null)
    {
        return _schedulerService.ListJobs(pageNumber ?? 1);
    }

    public async Task<JobDetail?> GetJobDetail(string group, string name)
    {
        var jobKey = new JobKey(name, group);
        var jobDetail = await _schedulerService.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        return
            new JobDetail
            {
                JobName = jobDetail.JobName,
                JobGroup = jobDetail.JobGroup,
                Description = jobDetail.Description,
                CronExpression = jobDetail.CronExpression,
                CronExpressionDescription = jobDetail.CronExpressionDescription,
                Package = jobDetail.Package,
                Parameters = jobDetail.Parameters
            };
    }

    public Task AddOrUpdateJob(JobAddOrUpdateModel model)
    {
        return _schedulerService.AddOrUpdateJob(model);
    }
}