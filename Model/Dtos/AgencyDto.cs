namespace Model.Dtos
{
    public class AgencyDto
    {
        public string AgencyCategoryCode { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }
        public string AgencyTelephone { get; set; }
        public string ContactName { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public bool? IsActive { get; set; }
    }
}
