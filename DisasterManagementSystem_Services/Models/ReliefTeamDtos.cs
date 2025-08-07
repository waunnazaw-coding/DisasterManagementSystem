using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Models
{
    public class ReliefTeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string TeamLeaderName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? NumberOfMembers { get; set; }
        public string Specialization { get; set; }
        public DateOnly? EstablishedDate { get; set; }
    }

    public class CreateReliefTeamDto
    {
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public int? LocationId { get; set; }
        public string Address { get; set; }
        public string TeamLeaderName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? NumberOfMembers { get; set; }
        public string Specialization { get; set; }
    }

    public class UpdateReliefTeamDto
    {
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public int? LocationId { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string TeamLeaderName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? NumberOfMembers { get; set; }
        public string Specialization { get; set; }
    }
}
