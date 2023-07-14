using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompEd.Nm.Net.Db.Model;

[Table("mailbox")]
[Index("Name", IsUnique = true)]
public record Mailbox
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name"), Required]
    public required string Name { get; init; }

    [Column("imap_host"), Required]
    public required string ImapHost { get; init; }

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
