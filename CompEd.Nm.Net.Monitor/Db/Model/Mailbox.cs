using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompEd.Nm.Net.Db.Model;

[Table("mailbox")]
[Index("Name", IsUnique = true)]
public class Mailbox
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name"), Required]
    public required string Name { get; set; }

    [Column("imap_host"), Required]
    public required string ImapHost { get; set; }

    [Column("imap_port")]
    public int? ImapPort { get; set; } = 993;

    [Column("imap_username")]
    public string? ImapUsername { get; set; }

    [Column("imap_password")]
    public string? ImapPassword { get; set; }

    [Column("watch_enabled")]
    public bool IsWatchEnabled { get; set; } = false;

    [Column("clean_enabled")]
    public bool IsCleanEnabled { get; set; } = false;

    [Column("clean_age")]
    public int CleanOlderThan { get; set; } = 30;

    [Column("folder")]
    public string? Folder { get; set; }
}
