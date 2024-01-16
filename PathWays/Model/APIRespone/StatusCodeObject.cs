using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Model.APIRespone
{
    public class StatusCodeObject
    {
        public string Value { get; set; }
        public object[] Formatters { get; set; }
        public object[] ContentTypes { get; set; }
        public object DeclaredType { get; set; }
        public int StatusCode { get; set; }
    }
}
