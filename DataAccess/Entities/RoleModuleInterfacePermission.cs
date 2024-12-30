using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("role_module_interface_permissions")]
public partial class RoleModuleInterfacePermission
{
    [Key]
    [Column("role_module_interface_permission_id")]
    public int RoleModuleInterfacePermissionId { get; set; }

    [Column("module_interface_permission_id")]
    public int? ModuleInterfacePermissionId { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

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

    [ForeignKey("ModuleInterfacePermissionId")]
    [InverseProperty("RoleModuleInterfacePermissions")]
    public virtual ModuleInterfacePermission? ModuleInterfacePermission { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("RoleModuleInterfacePermissions")]
    public virtual Role? Role { get; set; }
}
