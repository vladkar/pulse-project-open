using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement.ModelElement;
using Fusion.Core.Mathematics;

namespace City.UIFrames
{
    public class ColorConstant
    {
        // dark color theme
        public static readonly Color BlueAzure = new Color(0, 120, 215, 255);  // #0099bc
        public static readonly Color Blue = new Color(0, 120, 215, 255);       // #0078d7
        public static readonly Color Red = new Color(209, 52, 56, 255);        // #d13438
        public static readonly Color DarkRed = Color.DarkRed;
        public static readonly Color Perl = new Color(105, 105, 126, 255);     // #69697e
        public static readonly Color Turquoise = new Color(86, 124, 115, 255); // #567c73
        public static readonly Color Gray = new Color(122, 117, 116, 255);     // #7a7574
        public static readonly Color Violet = new Color(227, 0, 188, 255);     // #e300bc
        public static readonly Color Orange = new Color(218, 59, 1, 255);      // #da3b01
        public static readonly Color LiteGreen = new Color(0, 204, 106, 255);  // #00cc6a
        public static readonly Color LiteBlue = new Color(0, 183, 195, 255);   // #00b7c3 Egg Thrush
        public static readonly Color GrayBlue = new Color(126, 115, 95, 255);  // #7e735f
        public static readonly Color DarkBlue = new Color(104, 118, 138, 255); // #68768a
        public static readonly Color Crimson = new Color(223, 17, 35, 255);    // #e81123
        
        public static readonly Color Zero = Color.Zero;    // #e81123

        // scenario frame
        public static readonly Color BackColorScenario = new Color(23, 23, 23);
        public static readonly Color ScenarioBackColorButton = new Color(31, 31, 31);

        // common color
        public static readonly Color BackColor = Color.Black;
        public static readonly Color BorderColor = new Color(0, 150, 225, 255);
        public static readonly Color BorderColorActive = Color.Orange;
        public static readonly Color ForeColor = Color.White;
        public static readonly Color HoverForeColor = new Color(0, 150, 225, 255);
        public static readonly Color HoverColor = new Color(64, 64, 64, 255);
        public static readonly Color ActiveElement = BlueAzure;
        
        // status frame
        public static readonly Color TextStatus = new Color(255, 255, 255, 0.7f);
        //slider
        public static readonly Color BackColorForSlider = Color.Gray;
        // editbox
        public static readonly Color BackColorEditBox = new Color(0, 0, 0, 20);
        //Button
        public static readonly Color BackColorButton = new Color(64, 64, 64, 255);
        // Header
        public static readonly Color HeaderColor = Color.Orange;

        //Form
        public static readonly Color BackColorForm = new Color(23, 23, 23, 255);

        //plot and barchart 
        public static readonly Color AxisColor = Color.White;
        public static readonly Color TextColor = Color.White;
        
        //barchart
        public static readonly Color[] BarChartColor =
        {
            new Color(0, 150, 225, 255),
            Color.Orange,
            Color.Red,
            Color.Bisque
        };

        //default color map
        public static readonly Dictionary<int, ConfigPlot> defaultConfigPlot = new Dictionary<int, ConfigPlot>()
        {
            { 0, new ConfigPlot(Color.Yellow, "text1")},
            { 1, new ConfigPlot(new Color(0, 150, 225, 255), "text2")},
            { 2, new ConfigPlot(Color.Orange, "text3")},
            { 3, new ConfigPlot(Color.Red, "text4")},
            { 4, new ConfigPlot(Color.Bisque, "text5")}
        };

        // plot
        public static readonly Color PlotColor = new Color(0, 150, 225, 255);
        public static readonly Color MainAxisColor = Color.White;

        // train 
        public static readonly Color Health = new Color(0, 161, 75);
        public static readonly Color Biohazard = new Color(237, 28, 36);
        public static readonly Color trainBorderColor = new Color(30, 37, 43);
        public static readonly Color trainBackColor = new Color(26, 26, 26);
        public static readonly Color trainBottomFrameBackColor = new Color(25, 25, 25);
        public static readonly Color Title = new Color(0, 150, 225, 255);
        public static readonly Color RichText = new Color(255, 255, 255, 200);
        public static readonly Color RichTextWhite = new Color(255, 255, 255, 255);
    }
}
