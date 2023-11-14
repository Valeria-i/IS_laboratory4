using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Fname { get; set; }

        [MaxLength(100)]
        [Required]
        public string Lname { get; set; }

        [Range(18, 100)]
        [Required]
        public int Age { get; set; }

        [Required]
        public bool Status { get; set; }

    }
}
