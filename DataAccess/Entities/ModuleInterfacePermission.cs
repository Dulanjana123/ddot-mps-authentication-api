using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("module_interface_permission")]
public partial class ModuleInterfacePermission
{
    [Key]
    [Column("module_interface_permission_id")]
    public int ModuleInterfacePermissionId { get; set; }

    [Column("code")]
    [StringLength(100)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [Column("module_id")]
    public int? ModuleId { get; set; }

    [Column("interface_id")]
    public int? InterfaceId { get; set; }

    [Column("permission_id")]
    public int? PermissionId { get; set; }

    [Column("is_enabled")]
    public bool? IsEnabled { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("modified_by")]
    public int? ModifiedBy { get; set; }

    [Column("modified_date", TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("InterfaceId")]
    [InverseProperty("ModuleInterfacePermissions")]
    public virtual Interface? Interface { get; set; }

    [ForeignKey("ModuleId")]
    [InverseProperty("ModuleInterfacePermissions")]
    public virtual Module? Module { get; set; }

    [InverseProperty("ModuleInterfacePermission")]
    public virtual ICollection<RoleModuleInterfacePermission> RoleModuleInterfacePermissions { get; set; } = new List<RoleModuleInterfacePermission>();

    [ForeignKey("PermissionId")]
    [InverseProperty("ModuleInterfacePermissions")]
    public virtual MpsPermission? Permission { get; set; }
}
