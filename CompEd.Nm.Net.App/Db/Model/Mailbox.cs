using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompEd.Nm.Net.Db.Model;

[Table("mailbox")]
public record Mailbox
{
    [Column("name"), Required, Key]
    public string Name { get; init; } = default!;

    [Column("imap_host")]
    public string ImapHost { get; init; } = default!;

    [Column("imap_port")]
    public int? ImapPort { get; init; }

    [Column("imap_username")]
    public string? ImapUsername { get; init; }

    [Column("imap_password")]
    public string? ImapPassword { get; init; }

    [Column("watch_enabled")]
    public bool IsWatchEnabled { get; init; } = false;

    [Column("clean_enabled")]
    public bool IsCleanEnabled { get; init; } = false;

    [Column("folder")]
    public string? Folder { get; init; }
}
