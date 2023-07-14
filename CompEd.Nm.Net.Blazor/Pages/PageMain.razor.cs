using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace CompEd.Nm.Net.Pages;

public partial class PageMain
{
    [Inject]
    public IOptions<Settings> Settings { get; set; } = default!;

    [Inject]
    public Db.MainContext Db { get; set; } = default!;
}
