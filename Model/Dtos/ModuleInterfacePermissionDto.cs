namespace Model.Dtos
{
    public class ModuleInterfacePermissionDto
    {

        public int ModuleId { get; set; }

        public int? InterfaceId { get; set; }

        public int PermissionId { get; set; }

        public bool? IsEnabled { get; set; }

    }
}
