using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace City.UIFrames.Converter.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    class ClassAttribute : System.Attribute
    {
        public string name { get; set; }

        public ClassAttribute(string name)
        {
            this.name = name;
        }
    }
}
