using System;
using System.ComponentModel.DataAnnotations;

namespace Redis.Models
{
    public class ProductViewModel
    {
        public Guid id { get; set; }
        [Required]
        [Display(Name = "产品")]
        public String Name { get; set; }
        [Required]
        [Display(Name = "路径")]
        public String Url { get; set; }
        public int ViewCount { get; set; }
    }
}