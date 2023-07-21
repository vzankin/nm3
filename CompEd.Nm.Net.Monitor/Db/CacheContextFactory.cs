using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CompEd.Nm.Net.Db;

public class CacheContextFactory
{
    private readonly Settings settings;
    private readonly ILoggerFactory lf;

    public CacheContextFactory(Settings settings, ILoggerFactory lf)
    {
        this.settings = settings;
        this.lf = lf;
    }

    public async Task<CacheContext> CreateCacheContext(Model.Mailbox mailbox, CancellationToken ct)
    {
        var csb = new SqliteConnectionStringBuilder();
        var folder = mailbox.Folder ?? Path.Combine(settings.RootFolder, mailbox.Name);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        csb.DataSource = Path.Combine(folder, "cache.db");

        var ob = new DbContextOptionsBuilder<CacheContext>();
        ob.UseLoggerFactory(lf);
        ob.UseSqlite(csb.ConnectionString);
        ob.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        var cache = new CacheContext(ob.Options);
        await cache.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
        return cache;
    }
}
