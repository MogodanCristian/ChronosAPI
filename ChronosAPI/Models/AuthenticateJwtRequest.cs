using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Models
{
    public class AuthenticateJwtRequest
    {
        [Required]
        public string jwt { get; set; }
    }
}
