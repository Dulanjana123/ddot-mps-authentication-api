using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("agency_category")]
public partial class AgencyCategory
{
    [Key]
    [Column("agency_category_id")]
    public int AgencyCategoryId { get; set; }

    [Column("agency_category_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string AgencyCategoryCode { get; set; } = null!;

    [Column("agency_category_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? AgencyCategoryName { get; set; }

    [Column("agency_category_description")]
    [StringLength(200)]
    [Unicode(false)]
    public string? AgencyCategoryDescription { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

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

    [InverseProperty("AgencyCategory")]
    public virtual ICollection<Agency> Agencies { get; set; } = new List<Agency>();
}
