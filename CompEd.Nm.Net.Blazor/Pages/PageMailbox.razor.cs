using CompEd.Nm.Net.Db;
using CompEd.Nm.Net.Db.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace CompEd.Nm.Net.Pages;

public partial class PageMailbox : IDisposable
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    public required MainContext Db { get; set; }

    [Inject]
    public required CacheContextFactory CacheFactory { get; set; }

    [Inject]
    public required IStringLocalizer<PageMailbox> PageMailboxResources { get; set; }

    [Inject]
    public required IStringLocalizer<SharedResources> SharedResources { get; set; }

    public required Mailbox Mailbox { get; set; }
    public required CacheContext Cache { get; set; }
    public CacheInfo? Info { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Mailbox = Db.Mailboxes.Single(x => x.Id == Id);
        Cache = await CacheFactory.CreateCacheContext(Mailbox, default).ConfigureAwait(false);
        Info = await Cache.Cache.OrderBy(x => x.Id).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        Cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
