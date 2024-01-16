using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Model.APIRespone
{
    public class GetClockInPerson<T>
    {
        public StatusCodeObject StatusCode { get; set; }
        public bool Success { get; set; }
        public T Data { get; set; }
    }

 

   

}
