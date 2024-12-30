using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    [Table("user_type")]
    public partial class UserType
    {
        [Key]
        [Column("user_type_id")]
        public int UserTypeId { get; set; }

        [Column("user_type_code")]
        [StringLength(20)]
        [Unicode(false)]
        public string UserTypeCode { get; set; }

        [Column("user_type_name")]
        [StringLength(50)]
        [Unicode(false)]
        public string? UserTypeName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("sort_id")]
        public int? SortId { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [InverseProperty("UserType")]
        public virtual ICollection<MpsUser> Users { get; set; } = new List<MpsUser>();
        
    }
}
