using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Model.APIRespone
{
    public class PersonInsertResponse
    {
        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("insertId")]
        public string InsertId { get; set; }
    }
}
