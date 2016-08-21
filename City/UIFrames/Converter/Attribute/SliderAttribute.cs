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

    public class SliderAttribute : UIAttribute
    {
        public readonly string Name;
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly float InitValue;

        public SliderAttribute(string name, float minV, float maxV)
        {
            this.Name = name;
            this.MinValue = minV;
            this.MaxValue = maxV;
        }

        public SliderAttribute(string name, float minV, float maxV, float initV)
        {
            this.Name = name;
            this.MinValue = minV;
            this.MaxValue = maxV;
            this.InitValue = initV;
        }



        public override Frame CreateFrame(FrameProcessor ui, MemberInfo mi, object uiObj, int x, int y, int w, int h)
        {
            var pi = (PropertyInfo) mi;
            var slider  = FrameHelper.createSlider(ui, x, y, w, h, "", this.MinValue, this.MaxValue, InitValue, (f) => { pi.SetValue(uiObj, f);});
            slider.Font = ConstantFrameUI.segoeSemiBold15;
            pi.SetValue(uiObj, InitValue);
            slider.PropertyChanged += (sender, args) =>
            {
                pi.SetValue(uiObj, slider.Value);
            };
            return slider;
        }
    }
}
