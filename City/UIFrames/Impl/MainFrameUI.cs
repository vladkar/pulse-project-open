using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using City.ControlsClient.DomainClient;
using City.ControlsClient.DomainClient.VKFest.JsonReader;
using City.ControlsClient.DomainClient.VKFest.UI;
using City.Models;
using City.Panel;
using City.Snapshot;
using City.Snapshot.Snapshot;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using City.UIFrames.Impl;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Engine.Frames;
using Newtonsoft.Json;

namespace City.UIFrames
{
    public class MainFrameUI : Frame
    {
        private FrameProcessor FrameProcessor;

        // common
        private Frame menuFrameUI;
        private Frame bottomMenu;
        private Frame legendFrameUI;
        private Frame topPanel;
        private Frame rightPanel;
        // train


        private ControlsPanel controlPanel;
        private MultiViewPanel multiView;
        private PanelManager panelManager;
        private PulseMasterClient _pmcl;


        public MainFrameUI(FrameProcessor ui) : base(ui)
        {
            this.X = 0;
            this.Y = 0;
            this.Width = this.Game.RenderSystem.DisplayBounds.Width;
            this.Height = this.Game.RenderSystem.DisplayBounds.Height;
        }

        public MainFrameUI(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor, bool IsTrain = false) : base(ui, x, y, w, h, text, backColor)
        {
        }

        

        public void initElement(FrameProcessor ui)
        {
            this.FrameProcessor = ui;
            switch ((CustomGameInterface.UILayout))
            {
                case CustomGameInterface.UIenum.ht:
                    controlPanel = new ControlsPanel(FrameProcessor);
                    menuFrameUI = new MenuPanel(FrameProcessor);
                    bottomMenu = new StatusFrameUI(FrameProcessor);
                    topPanel = new TopPanel(ui);
                    legendFrameUI = new LegendFrameUI(FrameProcessor);
                    this.Add(menuFrameUI);
                    this.Add(bottomMenu);
                    this.Add(controlPanel);
                    this.Add(topPanel);
                    multiView = new MultiViewPanel(ui, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.topFrameUIHeight, this.Width - ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.controlsFrameUIWidth, menuFrameUI.Height,
                    "", ColorConstant.BackColor);
                    break;
                case CustomGameInterface.UIenum.train:
                    bottomMenu	= new SpecialBottomFrame(FrameProcessor);
                    multiView	= new MultiViewPanel(ui, 0, 0, this.Width, this.Height - bottomMenu.Height, "", ColorConstant.BackColor);
                    this.Add(bottomMenu);
                    if (CustomGameInterface.IsDemo)
                        (bottomMenu as SpecialBottomFrame).DefaultInitScenario();
                    break;
                case CustomGameInterface.UIenum.ins:
                    multiView = new MultiViewPanel(ui, 0, 0,
                    this.Width, this.Height,
                    "", ColorConstant.BackColor);
                    if (CustomGameInterface.IsDemo)
                        Game.Invoker.PushAndExecute("map ss");
                    break;
            }
            multiView.ConfiguratePanel(Orientation.One);
            this.Add(multiView);

            //            Regex r = new Regex(@"(?:(?<=\s)|^)#(\w*[A-Za-z_]+\w*)", RegexOptions.IgnoreCase);
            //            var posts = ControllerReader.GetPostsAndSaveFile("allpost.json", 0, 20000);
            //            System.IO.StreamWriter file = new System.IO.StreamWriter("ChainHash.txt");
            //            var mapTags = new Dictionary<string, int>();
            //            var id = 0;
            //            foreach (var post in posts.posts)
            //            {
            //                Console.WriteLine(++id);
            //                var matches = r.Matches(post.text);
            //                for (int i = 0; i < matches.Count; i++)
            //                {
            //                    if (!mapTags.ContainsKey(matches[i].Value))
            //                        mapTags.Add(matches[i].Value, mapTags.Count);
            //                    file.Write(mapTags[matches[i].Value] + ";");
            //                    //                    for (int j = i + 1; j < matches.Count; j++)
            //                    //                    {
            //                    //                        if (!mapTags.ContainsKey(matches[i].Value))
            //                    //                            mapTags.Add(matches[i].Value, mapTags.Count);
            //                    //                        if (!mapTags.ContainsKey(matches[j].Value))
            //                    //                            mapTags.Add(matches[j].Value, mapTags.Count);
            //                    //                        file.WriteLine(mapTags[matches[i].Value] + ";" + mapTags[matches[j].Value]);
            //                    //                    }
            //                }
            //                file.Write("\n");
            //            }
            //            file.Close();
            //
            //            System.IO.StreamWriter fileHashed = new System.IO.StreamWriter("HashTagsNameChain.txt");
            //            foreach (var tag in mapTags)
            //            {
            //                fileHashed.WriteLine(tag.Value + ";" + tag.Key);
            //            }
            //            fileHashed.Close();
//                        var plot = FrameHelper.createPlot(ui, 100, 30, 500, 300, null, "", "", "", ColorConstant.defaultConfigPlot);
//                        var i = 0;
//                        plot.setConfig(true, true, true, true, true);
//                        plot.setRangePointPlot(100);
//                        plot.Tick += (sender, args) =>
//                        {
//                            var point =
//                                new Vector2((float)DateTime.Now.Subtract(Plot.MinDate).TotalMilliseconds, (float) Math.Sin(i)/1000000-11);
//                            plot.addPointToPlot(point);
//                            i+=1;
//                        };
//                        this.Add(plot);
//                        var list = new ComplexLegend(ui, this.Width-ConstantFrameUI.controlsFrameUIWidth, 100, 0, 0, "", Color.Zero)
//                        {
//                            Border = 1,
//                            BorderColor = Color.Azure
//                        };
            //
            //            list.addNewELement(new Legenda(ui, 0, 0, 100, 100, "Legenda1", Color.Zero));
            //            var mapLegend = new MapLegend(ui, 0, 0, 100, 100, "", Color.Zero);
            //            mapLegend.addNewElement(ui.Game.Content.Load<DiscTexture>(@"ui\menu"), "Menu");
            //            mapLegend.addNewElement(ui.Game.Content.Load<DiscTexture>(@"ui\scenarios"), "Menu");
            //            mapLegend.addNewElement(ui.Game.Content.Load<DiscTexture>(@"ui\help"), "Menu");
            //            list.addNewELement(mapLegend);
            //            list.addNewELement(new Legenda(ui, 0, 0, 100, 100, "Legenda3", Color.Zero));
            //            this.Add(list);
            //            this.Add();

            //this.Add(FrameHelper.createEditBox(ui, 100, 100, 100, 40, "StartText", "Label"));
            //            var player = new PlayerControlPanel(ui, 100, 300, 1000, 200, "", ColorConstant.Zero);
            //            player.addSlider("s1", 0, 1, 0.2f, f => { });
            //            player.addSlider("s2", 0, 1, 0.4f, f => { });
            //
            //            this.Add(player);

            //            var slider = new Slider(ui, 100, 100, 200, 40, "Slider", Color.Zero)
            //            {
            //               // Image = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                backColorForSlider = ColorConstant.BackColorForSlider,
            //                ForeColor = ColorConstant.ForeColor,
            //                HoverColor = ColorConstant.HoverColor,
            //                HoverForeColor = ColorConstant.HoverForeColor,
            //                MinValue = 0,
            //                MaxValue = 1,
            //                Value = 0.5f,
            //                Border = 1,
            //                BorderColor = Color.Orange
            //            };
            //            this.Add(slider);
            //            var treeNode = new TreeNode(ui, 100, 100, 100, 40, "Text", Color.Zero)
            //            {
            //                backColorMainNode = ColorConstant.BlueAzure,
            //                CollapsePicture = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                ExpandPicture = this.Game.Content.Load<DiscTexture>(@"ui\menu"),
            //                BorderColor = Color.Orange,
            //                Border = 1
            //            };
            //            var treeNode1 = new TreeNode(ui, 100, 100, 100, 40, "Text", Color.Zero)
            //            {
            //                backColorMainNode = ColorConstant.BlueAzure,
            //                CollapsePicture = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                ExpandPicture = this.Game.Content.Load<DiscTexture>(@"ui\menu"),
            //                BorderColor = Color.Orange,
            //                Border = 1
            //            };
            //            var treeNode2 = new TreeNode(ui, 100, 100, 100, 40, "Text", Color.Zero)
            //            {
            //                backColorMainNode = ColorConstant.BlueAzure,
            //                CollapsePicture = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                ExpandPicture = this.Game.Content.Load<DiscTexture>(@"ui\menu"),
            //                BorderColor = Color.Orange,
            //                Border = 1
            //            };
            //            this.Add(treeNode);
            //            var icon = new IconWithText(ui, 100, 100, 200, 50, "Main text", Color.Zero)
            //            {
            //                sizePicture = 30,
            //                Image = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                Description = "description text",
            //                Border = 1,
            //                BorderColor = Color.Orange,
            //                PaddingLeft= 10,
            //            };
            //            var icon1 = new IconWithText(ui, 100, 100, 200, 50, "Main text", Color.Zero)
            //            {
            //                sizePicture = 30,
            //                Image = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                Description = "description text",
            //                Border = 1,
            //                BorderColor = Color.Orange,
            //                PaddingLeft = 10,
            //            };
            //            var icon2 = new IconWithText(ui, 100, 100, 200, 50, "Main text", Color.Zero)
            //            {
            //                sizePicture = 30,
            //                Image = this.Game.Content.Load<DiscTexture>(@"ui\palette"),
            //                Description = "description text",
            //                Border = 1,
            //                BorderColor = Color.Orange,
            //                PaddingLeft = 10,
            //            };
            //            treeNode.addNode(icon);
            //            treeNode.addNode(icon1);
            //            treeNode.addNode(icon2);
            //            treeNode.addNode(treeNode1);
            //            treeNode.addNode(treeNode2);
            //            this.Add(icon);
            //            var legenda = new Legenda(ui, 100, 100, 90, 200, "", Color.Zero)
            //            {
            //                Label = "Label",
            //                MinValue = "1",
            //                maxValue = "100",
            //            };
            //            this.Add(legenda);
        }

        public void resizePanel(CustomGameInterface.UIenum IsTrain)
        {
            this.multiView.resize(IsTrain);
            this.Width = this.Game.RenderSystem.DisplayBounds.Width;
            this.Height = this.Game.RenderSystem.DisplayBounds.Height;
        }

        public void ApplyClient(PulseMasterClient pmcl)
        {
            _pmcl = pmcl;
        }

        public void SetServerInfo(ServerInfo si)
        {
            panelManager = new PanelManager(Game);
            multiView.ConfiguratePanel(si.ViewLayout);

            //TODO invoke panel factry here (with server info config to determine which panes is gis)
            for (int i = 1; i <= multiView.listFrame.Count; i++)
            {
                var currentFrame = multiView.listFrame[i - 1];
                var panel = new GisPanel(this.Game, currentFrame.viewLayers, currentFrame);
                panel.Initialize();
                panelManager.Panels.Add(i, panel);
                currentFrame.SendPolygon += delegate(List<Polygon> polygons)
                {
                    foreach (var view in panel.Views)
                    {
                        (view as PulseMicroModelDeveloperView)?.SetPolygons(polygons, currentFrame);
                    }
                };  
            }

            var clients = panelManager.LoadOrUpdateControls(si);

            foreach (var panel in panelManager.Panels)
            {
                panel.Value.Load();
            }

            _pmcl.Clients = clients;

            controlPanel?.setPanelManager(panelManager);
        }

        private int GetCountPanel(ServerInfo si)
        {
            var listPanel = new HashSet<int>();

            foreach (var c in si.Controls)
            {
                foreach (var v in c.Value.Views)
                {
                    listPanel.Add(v.Value.Panel);
                }
            }

            return listPanel.Count;
        }

        public void FeedSnapshot(IDictionary<int, ISnapshot> snapshot)
        {
            foreach (var control in panelManager.Controls)
            {
                if (snapshot.ContainsKey(control.Key))
                    control.Value.FeedSnapshot(snapshot[control.Key]);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (panelManager != null)
            {
                panelManager.Update(gameTime);
//                foreach (var client in panelManager.Clients)
//                {
//                    client.Value.Update(gameTime);
//                }
            } 
        }

        public PanelManager GetPanelManager()
        {
            return panelManager;
        }


        public class Tool
        {
            public static ToolOption Option = ToolOption.None;

            public enum ToolOption
            {
                None,
                DrawingPolygon,
                DeletePolygon,
            }
        }
    }
}
