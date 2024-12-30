namespace Model.Dtos
{
    public class PermissionCreationDto
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateRoleWithPermissionsDto
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
        public bool? IsActive { get; set; }
        public List<PermissionCreationDto> Permissions { get; set; } = new List<PermissionCreationDto>();
    }
}
