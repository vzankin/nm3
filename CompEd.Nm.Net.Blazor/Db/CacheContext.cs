using Microsoft.EntityFrameworkCore;

namespace CompEd.Nm.Net.Db;

public class CacheContext : DbContext
{
    public DbSet<Model.Mail> Mails { get; set; }

    public CacheContext(DbContextOptions<CacheContext> opt) : base(opt)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    }
}
