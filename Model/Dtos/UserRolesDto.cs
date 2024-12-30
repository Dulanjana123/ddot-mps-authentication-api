namespace Model.Dtos
{
    public class UserRolesDto
    {
        public int UserId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
