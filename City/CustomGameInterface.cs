using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using City.Panel;
using Fusion;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using Fusion.Build;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Framework;
using Fusion.Engine.Frames;

namespace City
{
    public class CustomGameInterface : Fusion.Engine.Common.UserInterface
    {

        [GameModule("Console", "con", InitOrder.Before)]
        public GameConsole Console { get { return console; } }
        public GameConsole console;

        [GameModule("GUI", "gui", InitOrder.Before)]
        public FrameProcessor FrameProcessor { get { return ui; } }
        public FrameProcessor ui;

        SpriteLayer testLayer;
        DiscTexture texture;
        public RenderLayer MasterView { get; protected set; }

        float angle = 0;
        SpriteLayer uiLayer;

        public MainFrameUI mainFrame;



        /// <summary>
        /// Ctor
        /// </summary>
        /// <param Name="engine"></param>
        public CustomGameInterface(Game gameEngine) : base(gameEngine)
        {
            console = new GameConsole(gameEngine, "conchars");
            ui = new FrameProcessor(Game, @"fonts\segoeReg15");
            UILayout = UIenum.train;
            IsDemo = true;
        }

        [Config]
        public static UIenum UILayout { get; set; }

        [Config]
        public static bool IsDemo { get; set; }

        public enum UIenum {
            train,
            ht,
            ins
        }


	    public TouchTapEventHandler TapHandler;

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            // Font
            LoadFont();
            // Dictionary
            FrameHelper.LoadDictionary(ui.Game.Content);
            FrameHelper.setDictionary(ConstantFrameUI.DefaultDictionary);

			// Graphics
			MasterView = Game.RenderSystem.RenderWorld;// new RenderLayer(Game);//, 1024, 768);
            MasterView.SpriteLayers.Add(console.ConsoleSpriteLayer);
            Game.RenderSystem.AddLayer(MasterView);

            // create sprite layer
//            uiLayer = new SpriteLayer(Game.RenderSystem, 1024);
            uiLayer = ui.FramesSpriteLayer;
            MasterView.SpriteLayers.Add(uiLayer);

            // Created main frame
            mainFrame = new MainFrameUI(ui);
            ui.RootFrame = mainFrame;
            mainFrame.initElement(ui);
            Game.RenderSystem.DisplayBoundsChanged += (sender, args) => {
                mainFrame.resizePanel(UILayout);
            };
            
            ui.SettleControls();

			TapHandler = (p) => {
				var f = ConstantFrameUI.GetHoveredFrame(ui.RootFrame, p.Position);
				f?.OnClick(Keys.LeftButton, false);
			};

	        Game.Touch.Tap += TapHandler;

	        //            ui.RootFrame.ForEachChildren();
	        //            // test execution
	        //            View1 = new RenderLayer(Game)
	        //            {
	        //                Target = new TargetTexture(Game.RenderSystem, 300, 300, TargetFormat.LowDynamicRange),
	        //                Clear = true,
	        //            };
	        //
	        //            frame = new Frame(ui, 100, 100, 300, 300, "", Color.Zero)
	        //            {
	        //                Image = View1.Target,
	        //                Border = 1,
	        //                BorderColor = Color.Orange
	        //            };
	        //            mainPanel.Add(frame);
	        //            Game.RenderSystem.AddLayer(View1);
        }

        private void LoadFont()
        {
            ConstantFrameUI.segoeReg12 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeReg12");
            ConstantFrameUI.segoeReg15 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeReg15");
            ConstantFrameUI.segoeSemiBold15 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeSemiBold15");
            ConstantFrameUI.segoeReg20 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeReg20");
            ConstantFrameUI.segoeSemiLight24 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeSemiLight24");
            ConstantFrameUI.segoeLight34 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeLight34");
            ConstantFrameUI.segoeLight46 = this.Game.Content.Load<SpriteFont>(@"fonts\segoeLight46");
			ConstantFrameUI.sfUltraLight32 = this.Game.Content.Load<SpriteFont>(@"fonts\sfUltraLight32");
            ConstantFrameUI.sfReg12 = this.Game.Content.Load<SpriteFont>(@"fonts\sfReg12");
            ConstantFrameUI.sfReg15 = this.Game.Content.Load<SpriteFont>(@"fonts\sfReg15");
			ConstantFrameUI.sfBold15 = this.Game.Content.Load<SpriteFont>(@"fonts\sfBold16");
			ConstantFrameUI.sfLight18 = this.Game.Content.Load<SpriteFont>(@"fonts\sfReg16");
			ConstantFrameUI.sfLight20 = this.Game.Content.Load<SpriteFont>(@"fonts\sfLight20");
            ConstantFrameUI.sfLight25 = this.Game.Content.Load<SpriteFont>(@"fonts\sfLight25");
            ConstantFrameUI.sfLight50 = this.Game.Content.Load<SpriteFont>(@"fonts\sfLight50");
			ConstantFrameUI.sfThin54= this.Game.Content.Load<SpriteFont>(@"fonts\sfThin54");
			ConstantFrameUI.sfLight75 = this.Game.Content.Load<SpriteFont>(@"fonts\sfLight75");
			ConstantFrameUI.sfLight72 = this.Game.Content.Load<SpriteFont>(@"fonts\sfLight72");
        }

        void Keyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.F5)
            {

//                Builder.SafeBuild(@"..\..\..\Content", @"Content", @"..\..\..\Temp", "", false);

                Game.Reload();
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SafeDispose(ref testLayer);
            }
            base.Dispose(disposing);
        }


        /// <summary>
        /// Updates internal state of interface.
        /// </summary>
        /// <param Name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            console.Update(gameTime);
            ui.Update(gameTime);
//            ui.Draw(gameTime, uiLayer);
        }


		public override void RequestToExit()
		{
			Game.Exit();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public override void DiscoveryResponse(System.Net.IPEndPoint endPoint, string serverInfo)
		{
			Log.Message("DISCOVERY : {0} - {1}", endPoint.ToString(), serverInfo);
		}


//        public override void DiscoveryResponse(IPEndPoint endPoint, string serverInfo)
//        {
//            // throw new NotImplementedException();
//        }
    }
}
