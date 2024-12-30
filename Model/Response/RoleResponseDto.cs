namespace Model.Response
{
    public  class RoleResponseDto
    {
        public int RoleId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
        public int UserGroupId { get; set; }
        public bool IsActive { get; set; }
    }
}
