using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapperSaraAin
{
    public class TEStreamJson
    {
        public int ID { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string author { get; set; }
        public string country { get; set; }
        public string category { get; set; }
        public string image { get; set; }
        public int importance { get; set; }
        public DateTime date { get; set; }
        public DateTime expiration { get; set; }
        public string html { get; set; }
    }
}
