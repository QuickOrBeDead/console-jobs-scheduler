using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Models;

public sealed class PackageSaveModel
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public IFormFile? File { get; set; }
}