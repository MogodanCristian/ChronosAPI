using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Models
{
    public class TaskRequestModel
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Progress { get; set; }
        public int Priority { get; set; }
        public int BucketId { get; set; }
        public int UserId { get; set; }
    }
}
