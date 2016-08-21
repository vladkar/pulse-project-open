using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames
{
    public class ConstantFrameUI
    {
        
        public static float scale = 1.0f;
        public static int gridUnits = (int)(4 * scale);

        public static int sizeIcon = 5 * gridUnits;

        //status frame
        public const int statusFrameUIX = 0;
        public static readonly int statusFrameUIHeight = 10 * gridUnits;
        public static readonly int specBottomFrameUIHeight = 16 * gridUnits;
        public const string statusPanelName = "StatusPanel";
        //status: for langauge
        public const int languageWidth = 50;

        //controls Panel
        public const int controlsFrameUIY = 0;
        public static readonly int controlsFrameUIWidth = 98*gridUnits;
        public const string controlsPanelName = "ControlsPanel";
        public static readonly int controlsButtonWidth = 18*gridUnits;
        public static readonly int controlsButtonHeight = 17 * gridUnits;
        public static readonly int controlsPaddingTop = 18 * gridUnits;
        public static readonly int controlsPaddingLeft = 8 * gridUnits;

        //Menu panel
        public const int menuFrameUIX = 0;
        public const int menuFrameUIY = 0;
        public static readonly int menuFrameUIWidth = 12 * gridUnits;
        public const string menuPanelName = "MenuPanel";
        public static int menuListElementHeight = 11 * gridUnits;

        // additinal panel for menu 
        public static readonly int ListLayerPanelWidth = 90 * gridUnits;
        public static int menuPanelButtonWidth = (ListLayerPanelWidth - 13 * gridUnits)/2;
        public static int menuPanelButtonHeight = 8 * gridUnits;

        //legend
        public const int legendFrameUIX = 0;
        public const int legendFrameUIWidth = 100;
        public const int legendFrameUIHeight = 100;
        public const string legendPanelName = "LegendPanel";

        // top panel 
        public static readonly int topFrameUIHeight = 8 * gridUnits;
        public const string topPanelName = "LegendPanel";


        // checkbox
        public static readonly int sizeCheckbox = 5*gridUnits;

        // Header
        public const int HeaderHeight = 10;

        //TreeNode 
        public static readonly int sizeExpandButton = 2 * gridUnits;
        public static int offsetXChild = 4 * gridUnits;
        public static int controlsTreeNodeHeight = 10*gridUnits;

        // train
        public static readonly int offsetMapButtonList = 8 * gridUnits;
        public static readonly int mapButtonSize = 12*gridUnits;
        public static readonly int mapButtonImageSize = 5 * gridUnits;
        //  legend
        public static readonly int mapLegendMapHeight = 12*gridUnits;
        public static readonly int mapLegendMapOffset = 2 * gridUnits;
        //  button
        public static readonly int trainButtonWidth  = 12 * gridUnits;
        public static readonly int trainButtonHeight = 12 * gridUnits;
        public static readonly int offsetTrainButtonList =  4 * gridUnits;

        // train graphic 
        public static readonly int offsetGraphic = 8 * gridUnits;
        public static readonly int offsetRichText = 5*gridUnits;
        public static readonly int offsetYRichText = offsetGraphic + 17 * gridUnits;
        public static readonly int ofsetXRightRichText = 13*gridUnits;
        public static readonly int offsetBetweenText = 6 * gridUnits;
        public static readonly int trainGraphicHeight = 60*gridUnits;
        public static readonly int WorkSpaceGraphicHeight = 80 * gridUnits;


        // dictionary
        public const char LimiterFile = ';';
        public const string FileDictionary = "dictionary.txt";
        public const string DefaultDictionary = "en";

        // font
        public static SpriteFont segoeReg12;
        public static SpriteFont segoeReg15;            
        public static SpriteFont segoeSemiBold15;
        public static SpriteFont segoeReg20;
        public static SpriteFont segoeSemiLight24;
        public static SpriteFont segoeLight34;
        public static SpriteFont segoeLight46;

		public static SpriteFont sfUltraLight32;
		public static SpriteFont sfBold15;
        public static SpriteFont sfReg12;
        public static SpriteFont sfReg15;
		public static SpriteFont sfLight18;
		public static SpriteFont sfLight20;
		public static SpriteFont sfLight50;
		public static SpriteFont sfThin54;
		public static SpriteFont sfLight75;
        public static SpriteFont sfLight72;
        public static SpriteFont sfLight25;


        public static Frame GetHoveredFrame(Frame root, Point position)
		{
			Frame hoverFrame = null;
			UpdateHoverRecursive(root, position, ref hoverFrame);
			return hoverFrame;
		}


		static void UpdateHoverRecursive(Frame frame, Point p, ref Frame mouseHoverFrame)
		{
			if (frame == null) {
				return;
			}

			if (frame.GlobalRectangle.Contains(p)) {
				mouseHoverFrame = frame;
				foreach (var child in frame.Children) {
					UpdateHoverRecursive(child, p, ref mouseHoverFrame);
				}
			}
		}
	}
}
