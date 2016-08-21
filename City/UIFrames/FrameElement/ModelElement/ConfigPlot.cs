using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion.Core.Mathematics;
using System.Threading.Tasks;

namespace City.UIFrames.FrameElement.ModelElement
{
    public class ConfigPlot {

        public ConfigPlot(Color color, string text)
        {
            this.color = color;
            this.legend = text;
            this.visible = true;
        }

        public bool visible { get; set; }
        public Color color { get; set; }
        public string legend { get; set; }
    }
}
