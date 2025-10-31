using System;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models
{
    public class Complaints
    {
        public int ComplaintID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
