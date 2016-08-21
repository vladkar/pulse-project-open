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
    public class CheckboxAttribute : UIAttribute
    {
        public readonly string Name;

        public CheckboxAttribute(string name)
        {
            this.Name = name;
        }

        public override Frame CreateFrame(FrameProcessor ui, MemberInfo mi, object uiObj, int x, int y, int w, int h)
        {
            if (mi is MethodInfo)
            {
                var frame = FrameHelper.createCheckbox(ui, x, y, w, h, this.Name, () =>
                {
                    ((MethodInfo) mi).Invoke(uiObj, new object[0]);
                });
                frame.Font = ConstantFrameUI.segoeReg15;
                frame.PaddingLeft = 0;
                return frame;
            }
            else if (mi is PropertyInfo)
            {
                var pi = (PropertyInfo)mi;
                var frame = FrameHelper.createCheckbox(ui, x, y, w, h, this.Name, () => { pi.SetValue(uiObj, !((bool)pi.GetValue(uiObj))); });
                frame.Font = ConstantFrameUI.segoeReg15;
                frame.PaddingLeft = 0;
                return frame;
            }
            return null;
        }
    }
}
