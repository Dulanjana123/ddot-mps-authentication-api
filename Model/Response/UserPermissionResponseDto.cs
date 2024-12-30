namespace Model.Response
{
    public  class UserPermissionResponseDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
