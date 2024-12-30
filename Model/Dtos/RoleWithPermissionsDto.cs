namespace Model.Dtos
{
    public class RoleWithPermissionsDto
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
        public bool? IsActive { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
