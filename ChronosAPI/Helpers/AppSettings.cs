using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronosAPI.Helpers
{
    public class AppSettings
    {
        public String Secret { get; set; }

        public String ChronosDBCon { get; set; }

        public String dev_team_email { get; set; }

        public String dev_team_password { get; set; }
    }
}
