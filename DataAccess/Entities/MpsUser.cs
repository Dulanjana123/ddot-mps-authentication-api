using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

[Table("users")]
public partial class MpsUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("email_address")]
    [StringLength(100)]
    [Unicode(false)]
    public string EmailAddress { get; set; } = null!;

    [Column("first_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? LastName { get; set; }

    [Column("mobile_number")]
    [StringLength(15)]
    [Unicode(false)]
    public string? MobileNumber { get; set; }

    [Column("language_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string? LanguageCode { get; set; }

    [Column("language_other")]
    [StringLength(30)]
    [Unicode(false)]
    public string? LanguageOther { get; set; }

    [Column("language_proficiency_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string? LanguageProficiencyCode { get; set; }

    [Column("gender_code")]
    [StringLength(3)]
    [Unicode(false)]
    public string? GenderCode { get; set; }

    [Column("age_group_code")]
    [StringLength(10)]
    [Unicode(false)]
    public string? AgeGroupCode { get; set; }

    [Column("ethnicity_id")]
    public int? EthnicityId { get; set; }

    [Column("ethnicity_other")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EthnicityOther { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [Column("is_account_suspended")]
    public bool IsAccountSuspended { get; set; }

    [Column("account_suspended_by")]
    public int? AccountSuspendedBy { get; set; }

    [Column("account_suspension_reason")]
    [StringLength(2000)]
    [Unicode(false)]
    public string? AccountSuspensionReason { get; set; }

    [Column("account_suspension_date", TypeName = "datetime")]
    public DateTime? AccountSuspensionDate { get; set; }

    [Column("is_account_locked")]
    public bool IsAccountLocked { get; set; }

    [Column("last_account_lock_time", TypeName = "datetime")]
    public DateTime? LastAccountLockTime { get; set; }

    [Column("login_count")]
    public int? LoginCount { get; set; }

    [Column("login_fail_attempts")]
    public int? LoginFailAttempts { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("modified_date", TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    [Column("modified_by")]
    public int? ModifiedBy { get; set; }

    [Column("otp")]
    public int? Otp { get; set; }

    [Column("otp_generated_on", TypeName = "datetime")]
    public DateTime? OtpGeneratedOn { get; set; }

    [Column("is_migrated_from_legacy")]
    public bool IsMigratedFromLegacy { get; set; }

    [Column("is_initial_password_reset")]
    public bool IsInitialPasswordReset { get; set; }

    [Column("is_email_verified")]
    public bool IsEmailVerified { get; set; }

    [Column("is_sms_verified")]
    public bool IsSmsVerified { get; set; }

    [Column("otp_incorrect_count")]
    public int? OtpIncorrectCount { get; set; }

    [Column("is_reset_password_link_used")]
    public bool IsResetPasswordLinkUsed { get; set; }

    [Column("user_type_id")]
    public int? UserTypeId { get; set; }

    [Column("agency_id")]
    public int? AgencyId { get; set; }

    [ForeignKey("EthnicityId")]
    [InverseProperty("Users")]
    public virtual Ethnicity? Ethnicity { get; set; }

    [ForeignKey("UserTypeId")]
    [InverseProperty("Users")]
    public virtual UserType? UserType { get; set; }

    [ForeignKey("AgencyId")]
    [InverseProperty("Users")]
    public virtual Agency? Agency { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<LoginHistory> MpsLoginHistories { get; set; } = new List<LoginHistory>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
