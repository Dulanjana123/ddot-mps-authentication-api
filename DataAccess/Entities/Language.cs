using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("language")]
public partial class Language
{
    [Key]
    [Column("language_id")]
    public int LanguageId { get; set; }

    [Column("language_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string LanguageCode { get; set; } = null!;

    [Column("language_name")]
    [StringLength(30)]
    [Unicode(false)]
    public string? LanguageName { get; set; }

    [Column("is_system_language")]
    public bool IsSystemLanguage { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("sort_id")]
    public int? SortId { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("modified_by")]
    public int? ModifiedBy { get; set; }

    [Column("modified_date", TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }
}
