using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.Converter.Models;
using Fusion.Engine.Frames;

namespace City.UIFrames.Converter.Attribute
{
    class EditBoxAttribute : UIAttribute
    {
        public readonly string Name;
        public readonly string Text;

        public EditBoxAttribute(string name, string text)
        {
            this.Name = name;
            this.Text = text;
        }

        public override Frame CreateFrame(FrameProcessor ui, MemberInfo mi, object uiObj, int x, int y, int w, int h)
        {
            var pi = (PropertyInfo)mi;
            var frame = FrameHelper.createEditBox(ui, x, y, w, h, Text, Name);
            pi.SetValue(uiObj, frame.Text);
            frame.PropertyChanged += (sender, args) =>
            {
                pi.SetValue(uiObj, frame.Text);
            };
            return frame;
        }
    }
}
