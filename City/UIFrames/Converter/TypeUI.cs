using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace City.UIFrames.Converter
{
    [System.AttributeUsage(System.AttributeTargets.Class |
        System.AttributeTargets.Struct | 
        System.AttributeTargets.Field |
        System.AttributeTargets.Method,
            AllowMultiple = true)  ]
    class TypeUI : System.Attribute
    {
        string name;
        private string type;
        private string formName;
        public TypeUI(string name)
        {
            this.name = name;
        }

        public TypeUI(String name, String type, String formName)
        {
            this.name = name;
            this.type = type;
            this.formName = formName;
        }

        public string GetName()
        {
            return name;
        }

        public string GetType()
        {
            return type;
        }

        public string GetFormName()
        {
            return formName;
        }
    }
}
