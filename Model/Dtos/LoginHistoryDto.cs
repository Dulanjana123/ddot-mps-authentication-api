using Core.CommonDtos;
using Core.Enums;

namespace Model.Dtos
{
    public class LoginHistoryBaseDto
    {

        public UserIntractionType Userintractionid { get; set; }

        public string UserEmail { get; set; }

        public DateTime? Timestamp { get; set; }

    }
    public class LoginHistoryHeaderDto
    {
        public string Agent { get; set; }
        public string IpAddress  { get; set; }
}
    public class FullLoginHistoryDto : LoginHistoryRequestInfo
    {
        public UserIntractionType Userintractionid { get; set; }

        public int UserId { get; set; }

        public DateTime? Timestamp { get; set; }
    }
    public class LoginHistoryDto : LoginHistoryBaseDto
    {
        public int Id { get; set; }
    }
}
