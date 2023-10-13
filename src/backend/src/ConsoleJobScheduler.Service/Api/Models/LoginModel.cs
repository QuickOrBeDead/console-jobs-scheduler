﻿namespace ConsoleJobScheduler.Service.Api.Models;

public sealed class LoginModel
{
    public string UserName { get; set; }

    public string Password { get; set; }

    public bool RememberMe { get; set; }
}