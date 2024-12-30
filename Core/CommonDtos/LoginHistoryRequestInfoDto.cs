namespace Core.CommonDtos
{
    public class LoginHistoryRequestInfo
    {
        public string Browser { get; set; }
        public string Os { get; set; }
        public string Device { get; set; }
        public string Detaileddescription { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}
