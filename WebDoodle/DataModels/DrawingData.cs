using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDoodle.DataModels
{
    [Table("DrawingTable")]
    [PrimaryKey("id")]
    public class DrawingData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int id { get; set; }
        [Required]
        public int uid { get; set; }
        [Required]
        public string data { get; set; }
        public DrawingData() {}
    }
}
