namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;

using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

public sealed class PackageSaveModel
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public IFormFile? File { get; set; }
}