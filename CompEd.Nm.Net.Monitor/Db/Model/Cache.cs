using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompEd.Nm.Net.Db.Model;

[Table("info")]
public class Cache
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("imap_validity")]
    public uint Validity { get; set; }

    [Column("last_check")]
    public DateTime LastCheck { get; set; }
}
