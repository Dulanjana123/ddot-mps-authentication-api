using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("settings_grid_state")]
public partial class SettingsGridState
{
    [Key]
    [Column("settings_grid_state_id")]
    public int SettingsGridStateId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("interface_id")]
    public int? InterfaceId { get; set; }

    [Column("grid_object_json")]
    public string? GridObjectJson { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("sort_id")]
    public int? SortId { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("modified_by")]
    public int? ModifiedBy { get; set; }

    [Column("modified_date", TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }
}
