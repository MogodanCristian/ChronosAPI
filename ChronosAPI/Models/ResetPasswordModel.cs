using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Models
{
    public class ResetPasswordModel
    {
        [Required, EmailAddress]
        public string email { get; set; }

        [Required]
        public string newPassword { get; set; }
    }
}
