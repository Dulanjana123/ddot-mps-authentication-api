namespace Model.Response
{
    public class AgencyResponseDto
    {
        public int AgencyId { get; set; }
        public int AgencyCategoryId { get; set; }
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
