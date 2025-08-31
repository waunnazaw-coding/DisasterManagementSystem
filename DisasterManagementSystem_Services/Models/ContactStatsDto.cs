using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class ContactStatsDto
    {
        public int TotalContacts { get; set; }
        public int Last30Days { get; set; }
        public int Last7Days { get; set; }
        public int Today { get; set; }
    }
}
