using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Identity.Infra;

public sealed class IdentityManagementDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{
    public IdentityManagementDbContext(DbContextOptions<IdentityManagementDbContext> options) : base(options)
    {
    }
}