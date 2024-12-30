using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("ethnicity")]
public partial class Ethnicity
{
    [Key]
    [Column("ethnicity_id")]
    public int EthnicityId { get; set; }

    [Column("ethnicity_code")]
    [StringLength(30)]
    [Unicode(false)]
    public string EthnicityCode { get; set; } = null!;

    [Column("ethnicity_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EthnicityName { get; set; }

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

    [InverseProperty("Ethnicity")]
    public virtual ICollection<MpsUser> Users { get; set; } = new List<MpsUser>();
}
