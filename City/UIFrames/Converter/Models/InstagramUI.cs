using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using City.UIFrames.Converter.Attribute;

namespace City.UIFrames.Converter.Models
{
    [ClassAttribute("Instagram")]
    public class InstagramUI
    {
        [CheckboxAttribute("CheckboxName")]
        public bool IsVisible { get; set; }

        [CheckboxAttribute("CheckboxName")]
        public bool IsVisible1 { get; set; }

        [EditBoxAttribute("", "start text")]
        public string textField { get; set; }

        [SliderAttribute("SliderName", 0, 1)]
        public float Slider { get; set; }
     
        [Button("!")]
        public static void MethodTest()
        {
            Console.WriteLine("Test function!");
        }
    }
}
