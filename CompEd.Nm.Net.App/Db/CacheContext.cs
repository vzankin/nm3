using Microsoft.EntityFrameworkCore;

namespace CompEd.Nm.Net.Db;

internal class CacheContext : DbContext
{
    public DbSet<Model.Mail> Mails { get; set; } = default!;

    public CacheContext(DbContextOptions<CacheContext> opt) : base(opt)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    }
}
