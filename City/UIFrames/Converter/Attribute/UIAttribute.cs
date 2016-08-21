using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Frames;

namespace City.UIFrames.Converter.Models
{
    public abstract class UIAttribute : System.Attribute
    {
        public abstract Frame CreateFrame(FrameProcessor ui, MemberInfo mi, object uiObj, int x, int y, int w, int h);
    }
}
