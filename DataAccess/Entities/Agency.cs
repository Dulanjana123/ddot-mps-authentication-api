using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("agencies")]
public partial class Agency
{
    [Key]
    [Column("agency_id")]
    public int AgencyId { get; set; }

    [Column("agency_category_id")]
    public int? AgencyCategoryId { get; set; }

    [Column("agency_code")]
    [StringLength(40)]
    [Unicode(false)]
    public string AgencyCode { get; set; } = null!;

    [Column("agency_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? AgencyName { get; set; }

    [Column("agency_address")]
    [StringLength(200)]
    [Unicode(false)]
    public string? AgencyAddress { get; set; }

    [Column("agency_telephone")]
    [StringLength(20)]
    [Unicode(false)]
    public string? AgencyTelephone { get; set; }

    [Column("contact_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string? ContactName { get; set; }

    [Column("contact_telephone")]
    [StringLength(20)]
    [Unicode(false)]
    public string? ContactTelephone { get; set; }

    [Column("contact_email")]
    [StringLength(100)]
    [Unicode(false)]
    public string? ContactEmail { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("sort_id")]
    public int? SortId { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("modified_by", TypeName = "datetime")]
    public DateTime? ModifiedBy { get; set; }

    [Column("modified_date", TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [Column("legacy_id_for_migration")]
    public int? LegacyIdForMigration { get; set; }

    [ForeignKey("AgencyCategoryId")]
    [InverseProperty("Agencies")]
    public virtual AgencyCategory? AgencyCategory { get; set; }

    [InverseProperty("Agency")]
    public virtual ICollection<MpsUser> Users { get; set; } = new List<MpsUser>();
}
