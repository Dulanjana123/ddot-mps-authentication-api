using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dtos
{
    public class ModuleUIDto
    {
        public int ModuleId { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? SortId { get; set; }

        public bool? IsActive { get; set; }

        public List<InterfaceUIDto> Interfaces { get; set; } = new List<InterfaceUIDto>();
    }
    public class InterfaceUIDto
    {
        public int InterfaceId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool HasCreate { get; set; }
        public bool Create { get; set; }
        public bool HasRead { get; set; }
        public bool Read { get; set; }
        public bool HasUpdate { get; set; }
        public bool Update { get; set; }
        public bool HasDeactivate { get; set; }
        public int? SortId { get; set; }
        public bool Deactivate { get; set; }
        public List<PermissionUIDto> Permissions { get; set; } = new List<PermissionUIDto>();
    }
    public class PermissionUIDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? SortId { get; set; }
        public bool Checked { get; set; }
        public bool? IsCrud { get; set; }
        public bool? IsActive { get; set; }
        public int ModuleInterfacePermissionId { get; set; }
    }
}
