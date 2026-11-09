using System;
using System.Collections.Generic;
using System.Text;

namespace igraUP.Models
{
    public class User
    {
        public string login { get; set; }

        public string passwordhash { get; set; }

        public int bestscore { get; set; }

        public int score { get; set; }
    }
}
