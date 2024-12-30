namespace Model.Response
{
    public  class PermissionResponseDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? SortId { get; set; }
    }

}
