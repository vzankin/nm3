using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CompEd.Nm.Net.Db;

internal class MainContext : DbContext
{
    private readonly Settings settings;

    public DbSet<Model.Mailbox> Mailboxes { get; set; } = default!;

    public MainContext(IOptions<Settings> settings, DbContextOptions<MainContext> opt) : base(opt) =>
        this.settings = settings.Value;

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        var fileroot = settings.RootFolder;
        if (!Directory.Exists(fileroot))
            Directory.CreateDirectory(fileroot);
        var filepath = Path.Combine(fileroot, "settings.db");

        var csb = new SqliteConnectionStringBuilder();
        csb.DataSource = filepath;
        builder.UseSqlite(csb.ConnectionString).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
