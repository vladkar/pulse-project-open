using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using City.UIFrames.Impl;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Media;
using Fusion.Engine.Frames;
using System.Net;
using City.ControlsClient;
using City.UIFrames.FrameElement.ModelElement;
using Fusion;

namespace City.UIFrames
{
    public class FrameHelper
    {
        public static Button createButton(FrameProcessor ui, int x, int y, int w, int h, string text, Action action)
        {
            var button = new Button(ui, x, y, w, h, text, ColorConstant.BackColor)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                PaddingLeft = 0,         
            };

            if (action != null)
            {
                button.Click += (s, e) => action();
            }
            return button;
        }

        public static Frame createButtonI(FrameProcessor ui, int x, int y, int w, int h, string text, Texture image, int sizeImageX, int sizeImageY, Color colorImage, Action action)
        {
            var button = new ImageButton(ui, x, y, w, h, text, ColorConstant.Zero)
            {
                TextAlignment = Alignment.MiddleCenter,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                PaddingLeft = 0,
                sizeImageX = sizeImageX,
                sizeImageY = sizeImageY,
                Image = image,
                ImageMode = FrameImageMode.Stretched,
                ColorImage = colorImage
            };

            if (action != null)
            {
                button.Click += (s, e) => action();
            }
            return button;
        }
        
        public static Frame createImageButton(FrameProcessor ui, int x, int y, int w, int h, string img, string text, Action action, DiscTexture tex = null)
        {
            var button = new Button(ui, x, y, w, h, text, ColorConstant.Zero)
            {
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                Image = tex != null ? tex : ui.Game.Content.Load<DiscTexture>(img),
                ImageMode = FrameImageMode.Stretched,
                ImageOffsetX = 0,
                TextAlignment = Alignment.MiddleCenter,
                TextOffsetY = 0,
                TextOffsetX = 0,
                Anchor = FrameAnchor.Top | FrameAnchor.Left,
                TextEffect = TextEffect.Shadow
            };

            if (action != null)
            {
                button.Click += (s, e) => action();
            }
            return button;
        }

        public static Slider createSlider(FrameProcessor ui, int x, int y, int w, int h, string text, float min, float max, float current, Action<float> update)
        {
            var slider = new Slider(ui)
            {
                X = x,
                Y = y,
                Width = w,
                Height = h,
                backColorForSlider = ColorConstant.BackColorForSlider,
                ForeColor = ColorConstant.ForeColor,
                HoverColor = ColorConstant.HoverColor,
                HoverForeColor = ColorConstant.HoverForeColor,
                MinValue = min,
                MaxValue = max,
                Value = current,
                Text = text,
                SliderWidth = 5,
//                Anchor = FrameAnchor.Top | FrameAnchor.Left,
            };

            var r = slider.Font.MeasureString(text);
            var height = r.Height + 2 * slider.PaddingBottom + slider.SliderWidth;
            slider.Height = Math.Max(height, slider.Height);

            slider.Tick += (e, a) => {
                update(slider.Value);
            };
            return slider;
        }

        public static EditBox createEditBox(FrameProcessor ui, int x, int y, int w, int h, string text, string label=null)
        {
            var editbox = new EditBox(ui)
            {
                X = x,
                Y = y,
                Width = w,
                Height = h,
                HPadding = 10,
                VPadding = 5,
                HoverColor = ColorConstant.HoverColor,
                BackColor = ColorConstant.BackColorEditBox,
                ForeColor = ColorConstant.ForeColor,
                BorderColor = Color.Zero,
                BorderActive = ColorConstant.BorderColor,
                Text = text,
                Border = 1,
                Anchor = FrameAnchor.Top,
                Label = label
            };
            return editbox;
            //if (label == null) return editbox;

            //var frame = new Frame(ui, x, y, w, h + editbox.Font.LineHeight, label, ColorConstant.BackColor)
            //{
            //    TextAlignment = Alignment.TopLeft
            //};
            //frame.Add(editbox);
            //editbox.X = 0;
            //editbox.Y = frame.Font.LineHeight;
            //editbox.Height -= frame.Font.LineHeight;
            //return frame;
        }

		public static Checkbox createCheckbox(FrameProcessor ui, int x, int y, int w, int h, string text, Action action, Texture checkA = null, Texture checkN = null, bool IsChecked = false, bool blueColor = true)
		{
			Color active = (blueColor) ? ColorConstant.ActiveElement : Color.White;
			var checkbox = new Checkbox(ui)
			{
				X = x,
				Y = y,
				Width = w,
				Height = h,
				ColorCheckbox = ColorConstant.TextColor,
				ActiveColor = active,
				ForeColor = ColorConstant.ForeColor,
				Font = ConstantFrameUI.segoeReg15,
				IsChecked = IsChecked,
				Checked = checkA ?? ui.Game.Content.Load<DiscTexture>(@"ui\checked"),
				None = checkN ?? ui.Game.Content.Load<DiscTexture>(@"ui\none"),
				CheckedSolid = checkA ?? ui.Game.Content.Load<DiscTexture>(@"ui\checkedsolid"),
				NoneSolid = checkN ?? ui.Game.Content.Load<DiscTexture>(@"ui\nonesolid"),
				//                ActiveColor = ColorConstant.
				//                HoverColor = ColorConstant.HoverColor,
				//Anchor = FrameAnchor.Left | FrameAnchor.Top,
				PaddingLeft = 10,
				Text = text,
				//                BorderColor = Color.Orange,
				//                Border = 1,
			};
			checkbox.Click += (s, e) => action();
			return checkbox;
		}

		public static IconWithText createIconWithText(FrameProcessor ui, int x, int y, int w, int h, string text, string description, string command, Texture image, Action<Frame> action, Action doubleClick = null)
        {
            var icon = new IconWithText(ui, x, y, w, h, text, Color.Zero)
            {
                IsActive = false,
                sizePicture = 30,
                Image = image,
                Description = description,
                command = command,
                PaddingLeft = 10,
                HoverColor =  ColorConstant.HoverColor,
                ColorActiveButton = ColorConstant.ActiveElement,
            };

            icon.DoubleClick += (sender, args) =>
            {
                doubleClick?.Invoke();
            };

            icon.Click += (e, a) =>
            {
                action(icon);
            };
            return icon;
        }

        public static Frame createDropDownList(FrameProcessor ui, int x, int y, int w, int h, string selectedItem, List<string> itemList, Action<string> action)
        {
            var dropDownList = new DropDownList(ui)
            {
                X = x,
                Y = y,
                Width = w,
                Height = h,
                Text = selectedItem,
                itemList = itemList,
                TextAlignment = Alignment.MiddleCenter,
                BackColor = ColorConstant.BackColor,
                ForeColor = ColorConstant.ForeColor,
                Anchor = FrameAnchor.Top | FrameAnchor.Right,
                Border = 1,
                BorderColor = ColorConstant.BorderColor,
                Changed = action,
            };
            
            if (!"".Equals(selectedItem))
            {
                var sizeText = dropDownList.Font.MeasureString(selectedItem);
                dropDownList.Width = sizeText.Width+30;
                dropDownList.Height = sizeText.Height;
            }
            return dropDownList;
        }

        public static Frame createFrameWithHeader(FrameProcessor ui, int x, int y, int width, int height, string text)
        {
            var frame = new Frame(ui, x, y, width, height, text, ColorConstant.BackColor)
            {
                Padding = 10,
                TextAlignment = Alignment.TopCenter,
                Border = 1,
                BorderColor = ColorConstant.BorderColor,
            };
            frame.Add(new Header(ui,0,0,frame.Width, ConstantFrameUI.HeaderHeight,"", ColorConstant.BorderColor));
            return frame;
        }

        public static Frame createLabel(FrameProcessor ui, int x, int y, string text, bool flip=false)
        {
            var label = new Label(ui, x, y, 0, 0, text, Color.Zero) {IsFlip = flip};
            return label;
        }

        public static Frame createBarChart(FrameProcessor ui, int x, int y, int width, int height, List<List<BarChart.PairCoor>> data, string nameGraphic, string rowName, string columnName)
        {
            
            var barChart = new BarChart(ui, width / 10, height / 10, width * 4 / 5, height * 4 / 5, "", Color.Zero)
            {
            };
            barChart.addValues(data);
            var frame = new GraphicBase(ui, x, y, width, height, "", Color.Zero)
            {
                GraphicName = nameGraphic,
                rowName = rowName,
                columnName = columnName,
            };
            frame.init(barChart);
            return frame;
        }

        public static GraphicBase createPlot(FrameProcessor ui, int x, int y, int width, int height, List<List<Vector2>> data, string graphicName, string rowName, string columnName, Dictionary<int, ConfigPlot> configPlot)
        {
            var plot = new Plot(ui, ConstantFrameUI.gridUnits*3 , 10 * ConstantFrameUI.gridUnits, width - ConstantFrameUI.gridUnits * 5, height - 10 * ConstantFrameUI.gridUnits - 10*ConstantFrameUI.gridUnits, "", Color.Zero)
            {
                configPlots = configPlot
            };
            
            var frame = new GraphicBase(ui, x, y, width, height, "", Color.Zero)
            {
                GraphicName = graphicName,
                rowName = rowName,
                columnName = columnName,
                WorkSpaceNameFont = ConstantFrameUI.sfLight25
            };

            //            frame.Add(FrameHelper.createScroll(ui, 0, 0, width, 20, (f, f1) =>
            //            {
            //                plot.updateLimit(f,f1);
            //            }));

            //            frame.Add(FrameHelper.adderPlot(ui, 0, 50, 50, 50, plot));

            frame.init(plot);
            plot.addPlots(data);
            return frame;
        }

        public static Frame createScroll(FrameProcessor ui, int x, int y, int width, int height, Action<float, float> actionForMove, bool isHorizontal=true)
        {
            
            var scroll = new Scroll(ui, 0, 0, isHorizontal ? width/2 : height, isHorizontal ? height : width / 2, "", Color.Orange)
            {
                actionForMove = actionForMove,
            };
            var frame = new Frame(ui, x, y, width, height, "", Color.Zero)
            {
                BorderColor = ColorConstant.BorderColor,
                Border = 1
            };
            frame.Add(scroll);
            return frame;
        }

        public static Frame adderPlot(FrameProcessor ui, int x, int y, int width, int height, Plot plot)
        {
            int WidthInfoForm = 200;
            int HeightInfoForm = 100;
            int WidthInfoFormbutton = 100;
            int HeightInfoFormbutton = 30;
        
            var frame = createFrameWithHeader(ui, x,
                y, width, height, "");
        
        
            EditBox editboxX = (EditBox) createEditBox(ui, 20, 20, 70, 40, "");
            EditBox editboxY = (EditBox) createEditBox(ui, 20, 70, 70, 40, "");
            frame.Add(editboxX);
            frame.Add(editboxY);
        
            var button = createButton(ui, width/2,
                height * 7/10, width/2, 30, "Ok",
                () =>
                {
                    for (float i = -100; i < 100; i += 0.1f)
                    {
                        plot.addPoint(new Vector2(i, 2* (float)Math.Sin(i)));
                        plot.addPoint(new Vector2(i, 3* (float)Math.Cos(i)),1);
                    }
                });
            frame.Add(button);
            return frame;
        }

        public static Frame addHeaderToFrame(FrameProcessor ui, Frame frame)
        {
            var frameHeader = createFrameWithHeader(ui, frame.X, frame.Y, frame.Width, frame.Height, "");
            frame.X = 0;
            frame.Y = ConstantFrameUI.HeaderHeight;
            frame.Height -= ConstantFrameUI.HeaderHeight;
            frameHeader.Add(frame);
            return frameHeader;
        }

        public static TreeNode createTreeNode(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor)
        {
            var treeNode = new TreeNode(ui, x, y, w, h, text, Color.Zero)
            {
                backColorMainNode = backColor,
                CollapsePicture = ui.Game.Content.Load<DiscTexture>(@"ui\list_down"),
                ExpandPicture = ui.Game.Content.Load<DiscTexture>(@"ui\list_up"),
//                BorderColor = Color.Orange,
//                Border = 1
            };
            return treeNode;
        }

        public static VideoFrame createVideoFrame(FrameProcessor ui, string filename, Gis.GeoPoint geoPoint)
        {
            var frame = new VideoFrame(ui, 0, 0, 0, 0, "", Color.Zero, filename)
            {
                geoPoint = geoPoint,
            };
            return frame;
        }

        public static void createMessageForm(FrameProcessor ui, string text)
        {
            int WidthInfoForm = 200;
            int HeightInfoForm = 100;
            int WidthInfoFormbutton = 100;
            int HeightInfoFormbutton = 30;

            var frame = createFrameWithHeader(ui, ui.Game.RenderSystem.DisplayBounds.Width/2,
                ui.Game.RenderSystem.DisplayBounds.Height/2, WidthInfoForm, HeightInfoForm, text);

            var button = createButton(ui, WidthInfoForm/2 - WidthInfoFormbutton/2,
                HeightInfoForm - HeightInfoFormbutton - 10, WidthInfoFormbutton, HeightInfoFormbutton, "Ok",
                () => ui.RootFrame.Remove(frame));
            frame.Add(button);
            ui.RootFrame.Add(frame);
        }

        public static void createAcceptForm(FrameProcessor ui, Action action)
        {
            int WidthInfoForm = 200;
            int HeightInfoForm = 100;
            int WidthInfoFormbutton = 90;
            int HeightInfoFormbutton = 30;
            int shift = 5;

            string textInfo;
            FrameHelper.DictionaryLocal.TryGetValue("Information", out textInfo);
            var frame = createFrameWithHeader(ui, ui.Game.RenderSystem.DisplayBounds.Width / 2,
                ui.Game.RenderSystem.DisplayBounds.Height / 2, WidthInfoForm, HeightInfoForm, textInfo);

            string textOk;
            FrameHelper.DictionaryLocal.TryGetValue("Ok", out textOk);
            var buttonOk = createButton(ui, WidthInfoForm/2 - WidthInfoFormbutton - shift, HeightInfoForm - HeightInfoFormbutton - 10, WidthInfoFormbutton,
                HeightInfoFormbutton, "Ok", () => { ui.RootFrame.Remove(frame); action(); });

            string textCancel;
            FrameHelper.DictionaryLocal.TryGetValue("Cancel", out textCancel);
            var buttonCancel = createButton(ui, WidthInfoForm / 2 + shift, HeightInfoForm - HeightInfoFormbutton - 10,
                WidthInfoFormbutton, HeightInfoFormbutton, textCancel, () => { ui.RootFrame.Remove(frame); });

            frame.Add(buttonOk);
            frame.Add(buttonCancel);
            ui.RootFrame.Add(frame);
        }

        

        public static List<String> languageList; 
        public static Dictionary<string, string> DictionaryLocal;
        private static Dictionary<string, Dictionary<string, string>> Dictionary;


        public static void setDictionary(string local)
        {
            Dictionary.TryGetValue(local, out DictionaryLocal);
        }

        public static void LoadDictionary(ContentManager content)
        {
            var strings = content.Load<string>(ConstantFrameUI.FileDictionary).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            languageList = new List<string>();
            Dictionary = new Dictionary<string, Dictionary<string, string>>();

            var headers = strings[0].Split(ConstantFrameUI.LimiterFile);

            foreach (var s in strings.Skip(1))
            {
                if (s == String.Empty)
                    continue;
                Dictionary<string, string> dictLanguage = new Dictionary<string,string>();
                var values = s.Split(ConstantFrameUI.LimiterFile);
                var language = values[0];
                languageList.Add(language);
                for (int i = 1; i<values.Length; i++)
                {
                    dictLanguage.Add(headers[i], values[i]);
                }
                Dictionary.Add(language, dictLanguage);
            }
        }


		public static WebPostFrame createWebPhotoFrame(FrameProcessor ui, int x, int y, int w, int h, string img, string url, int sizeRightPanel, InstagramPost[] inst, Frame activity)
		{
			var frame = new WebPostFrame(ui, x, y, w, h, img, url, Color.Zero, "text sample", sizeRightPanel, inst, activity);
			return frame;
		}

		public static WebPostFrame createWebPhotoFrameGeoTag(FrameProcessor ui, int x, int y, int w, int h, string img, string url, int sizeRightPanel, InstagramPost[] inst, Gis.GeoPoint coords)
		{
			var frame = new WebPostFrame(ui, x, y, w, h, img, url, Color.Zero, "text sample", sizeRightPanel, inst)
			{
				geoPoint = coords,
			};
			return frame;
		}

		public static Frame createLable(FrameProcessor ui, int x, int y, string text, SpriteFont font)
        {
            var frame = new Frame(ui, x, y, 0, 0, text, Color.Zero)
            {
                Font = font
            };
            var sizeText = frame.Font.MeasureString(frame.Text);
            frame.Width = sizeText.Width;
            frame.Height = sizeText.Height;
            return frame;
        }

        public static Frame createMainButton(FrameProcessor ui, int x, int y, int width, int height, Action action)
        {
            var mainButton = FrameHelper.createButtonI(ui, x, y, width, height, "",
                ui.Game.Content.Load<DiscTexture>(@"ui\menu"),
                ConstantFrameUI.sizeIcon,
                ConstantFrameUI.sizeIcon,
                ColorConstant.TextColor,
                action
            );
            mainButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            return mainButton;
        }

        public static Frame createHelpButton(FrameProcessor ui, int x, int y, int width, int height, Action infoAction)
        {
            var helpButton = FrameHelper.createButtonI(ui, x, y, width, height, "",
                ui.Game.Content.Load<DiscTexture>(@"ui\info"),
                ConstantFrameUI.sizeIcon,
                ConstantFrameUI.sizeIcon,
                ColorConstant.TextColor,
                infoAction
            );
            helpButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            return helpButton;
        }

        public static Frame createTrainMapLabel(FrameProcessor ui, int x, int y, string text, string countBiozard, string countHealth)
        {
            return new TrainMapLabel(ui, x, y, 0, ConstantFrameUI.mapLegendMapHeight, text, Color.Zero)
            {
                biohazard = ui.Game.Content.Load<DiscTexture>(@"ui\biohazard"),
                health = ui.Game.Content.Load<DiscTexture>(@"ui\healthy"),
                countBiozard = countBiozard,
                countHealth = countHealth,
                Border = 1,
                BorderColor = ColorConstant.BorderColor
            };
        }


        public static void loadPhoto(RenderSystem rs, Frame photo, string filename, int size)
        {
            var task = new Task(() => {
                var webClient = new WebClient();
                var fileName = Path.GetTempFileName();
                try
                {
                    webClient.DownloadFile(filename, fileName);
                    photo.Image = new UserTexture(rs, File.OpenRead(fileName), false);
                    float coeff = (float)photo.Image.Height / photo.Image.Width;
                    photo.Width = (int)(size);
                    photo.Height = (int)(size * coeff);
                    photo.ImageMode = FrameImageMode.Stretched;
                    photo.Visible = true;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            });
            task.Start();

        }


        // TODO: RunTransition
        //        var panelMapOpen = true;
        //        var button = FrameHelper.createButton(ui, 100, 100, 100, 40, "Click me", () =>
        //        {
        //            panelMapOpen = !panelMapOpen;
        //            scenarioFrameUi.RunTransition("X", panelMapOpen ? this.Width - ConstantFrameUI.controlsFrameUIWidth : this.Width, 0, 500);
        //        });
        //         this.Add(button);


        // TODO LIST with HEADER

        //        var listBox = new ListBox(ui, 10, 10, 100, 100, "", Color.Zero)
        //        {
        //            BorderColor = ConstantFrameUI.HeaderColor,
        //            Border = 1,
        //        };
        //        var treeNode = new TreeNode(ui, 100, 100, 100, 30, "Text Node Main", Color.Gray);
        //        var treeNode1 = new TreeNode(ui, 100, 100, 100, 30, "Text Node 1", Color.Gray);
        //        var treeNode2 = new TreeNode(ui, 100, 100, 100, 30, "Text Node 2", Color.Gray);
        //        var treeNode3 = new TreeNode(ui, 100, 100, 100, 30, "Text Node", Color.Gray);
        //        var treeNode4 = new TreeNode(ui, 100, 100, 100, 30, "Text Node", Color.Gray);
        //        listBox.addElement(treeNode1);
        //            listBox.addElement(treeNode3);
        //            listBox.addElement(FrameHelper.createSlider(FrameProcessor, 100, 100, 100, this.Font.LineHeight+5,  "TEXT", 0, 1, 0, (e) => { }));
        //            listBox.addElement(treeNode2);
        //            this.Add(listBox);


    }
}
