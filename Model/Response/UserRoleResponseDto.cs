namespace Model.Response
{
    public  class UserRoleResponseDto
    {
        public int RoleId { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
