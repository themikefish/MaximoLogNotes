using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheMagicLogNotePhraseDetector5000v1
{
    class FindReplace
    {
        public FindReplace(string find, string replace)
        {
            Find = find;
            Replace = replace;
        }

        public string Find { get; set; }
        public string Replace { get; set; }
    }
}
