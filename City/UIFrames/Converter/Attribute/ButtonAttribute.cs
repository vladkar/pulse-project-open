using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.Converter.Models;
using Fusion.Engine.Frames;

namespace City.UIFrames.Converter
{
    public class ButtonAttribute : UIAttribute
    {
        public readonly string Name;

        public ButtonAttribute(string name)
        {
            this.Name = name;
        }

        public override Frame CreateFrame(FrameProcessor ui, MemberInfo mi, object uiObj, int x, int y, int w, int h)
        {
            var frame = FrameHelper.createButton(ui, x, y, w, h, this.Name, () =>((MethodInfo)mi).Invoke(uiObj, new object[0]));
            frame.Font = ConstantFrameUI.segoeSemiBold15;
            return frame;
        }
    }
}
