using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CompEd.Nm.Net.Db;

public class ContextFactory
{
    private readonly Settings settings;
    private readonly ILoggerFactory lf;

    public ContextFactory(IOptions<Settings> options, ILoggerFactory lf)
    {
        this.settings = options.Value;
        this.lf = lf;
    }

    public async Task<CacheContext> CreateCacheContext(Model.Mailbox mailbox, CancellationToken ct)
    {
        var folder = mailbox.Folder ?? Path.Combine(settings.RootFolder, mailbox.Name);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var csb = new SqliteConnectionStringBuilder();
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
