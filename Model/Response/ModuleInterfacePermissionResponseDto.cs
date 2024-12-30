namespace Model.Dtos
{
    public class ModuleInterfacePermissionResponseDto
    {
        public int ModuleInterfacePermissionId { get; set; }

        public string Code { get; set; } = null!;

        public int ModuleId { get; set; }

        public int? InterfaceId { get; set; }

        public int PermissionId { get; set; }

        public bool? IsEnabled { get; set; }
    }
}
