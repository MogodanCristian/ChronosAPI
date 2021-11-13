using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Models
{
    public class PlanDispatcher
    {
        public int PlanDispatcherId { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
