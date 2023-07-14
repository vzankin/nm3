using CompEd.Nm.Net.Db;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace CompEd.Nm.Net.Pages;

public partial class PageMailbox
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    public required MainContext Db { get; set; }

    [Inject]
    public required ILoggerFactory LoggerFactory { get; set; }

    [Inject]
    public required IOptions<Settings> Settings { get; set; }

    public required Db.Model.Mailbox Mailbox { get; set; }

    protected override void OnInitialized()
    {
        Mailbox = Db.Mailboxes.Single(x => x.Id == Id);
    }
}
