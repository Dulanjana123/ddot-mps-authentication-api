namespace Model.Dtos
{
    public class InterfaceDto
    {
        public int InterfaceId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool HasCreate { get; set; }
        public bool Create { get; set; }
        public bool HasRead { get; set; }
        public bool Read { get; set; }
        public bool HasUpdate { get; set; }
        public bool Update { get; set; }
        public bool HasDeactivate { get; set; }
        public bool Deactivate { get; set; }
    }
}
