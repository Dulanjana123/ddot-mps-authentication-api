namespace Core.CommonDtos
{
    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
