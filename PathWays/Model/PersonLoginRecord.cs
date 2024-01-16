using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Model
{
    public class PersonLoginRecord
    {
        public int Id { get; set; }
        public string? Person_Id { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string? VisitorPersonId { get; set; }
    }
}
