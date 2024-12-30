namespace Model.Dtos
{
    public class ModuleDto
    {
        public int ModuleId { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int? SortId { get; set; }

        public bool? IsActive { get; set; }

        public List<PermissionDto> OtherPermissions { get; set; } = new List<PermissionDto>();
        public List<InterfaceDto> Interfaces { get; set; } = new List<InterfaceDto>();
    }
}
