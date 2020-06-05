using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis.Data
{
    public class Products
    {
        public Guid id { get; set; } = new Guid();
        public String Name { get; set; }
        public String Url { get; set; }
        public int ViewCount { get; set; }
    }
}
