namespace Model.Dtos
{
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? SortId { get; set; }
        public bool Checked { get; set; }

    }
}
