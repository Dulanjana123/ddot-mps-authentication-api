namespace Model.Response
{
    public class DatagridStateResponseDto
    {
        public int SettingsGridStateId { get; set; }
        public int UserId { get; set; }
        public int InterfaceId { get; set; }
        public string? GridObjectJson { get; set; }
        public bool? IsActive { get; set; }
    }
}
