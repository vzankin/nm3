using MailKit;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompEd.Nm.Net.Db.Model;

[Table("mail")]
[Index("MessageId", IsUnique = true)]
[Index("PecId")]
[Index("SdiId")]
[Index("UidValidity", "Uid", IsUnique = true)]
public class Mail
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("imap_validity")]
    public uint? UidValidity { get; set; }

    [Column("imap_uid")]
    public uint? Uid { get; set; }

    [Column("msg_id")]
    public string? MessageId { get; set; }

    [Column("subject")]
    public string? Subject { get; set; }

    [Column("from")]
    public string? From { get; set; }

    [Column("date")]
    public DateTime? Date { get; set; }

    [Column("pec_type")]
    public string? PecType { get; set; }

    [Column("pec_id")]
    public string? PecId { get; set; }

    [Column("sdi_type")]
    public string? SdiType { get; set; }

    [Column("sdi_id")]
    public string? SdiId { get; set; }


    [NotMapped]
    public UniqueId? UniqueId
    {
        get => UidValidity.HasValue && Uid.HasValue ? new(UidValidity.Value, Uid.Value) : null;
        set
        {
            UidValidity = value?.Validity;
            Uid = value?.Id;
        }
    }
}
