using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace WebDoodle.DataModels
{
    [Table("UserDataTable")]
    [PrimaryKey("uid")]
    public class UserData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int uid { get; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public UserData() {}
    }
}
