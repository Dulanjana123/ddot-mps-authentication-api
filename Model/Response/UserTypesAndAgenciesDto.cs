namespace Model.Response
{
    public class UserTypesAndAgenciesDto
    {
        public List<UserTypeComboDto> UserTypes { get; set; } = new List<UserTypeComboDto>();
        public List<AgencyComboDto> Agencies { get; set; } = new List<AgencyComboDto>();
    }

    public class UserTypeComboDto
    {
        public int Key { get; set; }
        public string Value { get; set; } = null!;
    }

    public class AgencyComboDto
    {
        public int Key { get; set; }
        public string Value { get; set; } = null!;
    }
}
