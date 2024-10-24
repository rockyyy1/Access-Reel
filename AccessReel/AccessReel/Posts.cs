using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessReel
{
    //Posts refer to Blog Posts
    public class Posts
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public DateTime? Date { get; set; }
        public Image? Image { get; set; }
    }
    
}
