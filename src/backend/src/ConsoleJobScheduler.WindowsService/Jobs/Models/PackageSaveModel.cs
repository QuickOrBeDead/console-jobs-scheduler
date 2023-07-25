﻿namespace ConsoleJobScheduler.WindowsService.Jobs.Models;

using System.ComponentModel.DataAnnotations;

public sealed class PackageSaveModel
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public IFormFile? File { get; set; }
}