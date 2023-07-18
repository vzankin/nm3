using Microsoft.EntityFrameworkCore;

namespace CompEd.Nm.Net.Db;

public class CacheContext : DbContext
{
    public DbSet<Model.Mail> Mails { get; set; }
    public DbSet<Model.Cache> Cache { get; set; }

    public CacheContext(DbContextOptions<CacheContext> opt) : base(opt)
    {
    }
}
