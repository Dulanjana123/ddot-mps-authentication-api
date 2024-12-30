using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("user_merge_email_log")]
public partial class UserMergeEmailLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("email")]
    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("is_error")]
    public bool IsError { get; set; }

    [Column("error")]
    [StringLength(2000)]
    [Unicode(false)]
    public string? Error { get; set; }

    [Column("batch")]
    [StringLength(40)]
    [Unicode(false)]
    public string? Batch { get; set; }

    [Column("language")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Language { get; set; }

    [Column("attempts")]
    public int? Attempts { get; set; }

    [Column("email_sent_date", TypeName = "datetime")]
    public DateTime? EmailSentDate { get; set; }

    [Column("first_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? LastName { get; set; }

    [Column("mobile_number")]
    [StringLength(15)]
    [Unicode(false)]
    public string? MobileNumber { get; set; }

    [Column("is_migrated_to_b2c")]
    public bool IsMigratedToB2c { get; set; }

    [Column("migration_error")]
    [StringLength(2000)]
    [Unicode(false)]
    public string? MigrationError { get; set; }

    [Column("migrated_date", TypeName = "datetime")]
    public DateTime? MigratedDate { get; set; }

    [Column("migration_attempts")]
    public int? MigrationAttempts { get; set; }
}
