using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames.Impl
{
    class InstagramToolPanel : Frame {

        private FrameProcessor ui;
        private int WidthPanel = 105*ConstantFrameUI.gridUnits;

        private int offset = ConstantFrameUI.gridUnits;

		private readonly Color InstagramToolPanelColor = new Color(25, 25, 25, 180);

		public ListBox socialFrameList;
		public ListBox filterFrameList;
		public ListBox filterColorFrameList;
		public bool needsUpdate = true;


		public InstagramToolPanel(FrameProcessor ui) : base(ui)
		{
			init(ui);
		}

		public InstagramToolPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
		{
			init(ui);
		}

		private static readonly string[] socialList = new[] { "Вконтакте", "Twitter", "Instagram" };
		private static readonly string[] modeList = new[] { "Счастливое", "Нейтральное / Сложно оценить", "Плохое (грусть, агрессия и т.д.)" };
		private static readonly string[] filterColorList = new[] { "Выключить", "По социальным сетям", "По машинной оценке", "По пользовательской оценке" };
		public List<Checkbox> listFilterColorCheckbox = new List<Checkbox>();

		private void init(FrameProcessor ui)
		{
			this.ui = ui;
			this.Width = WidthPanel;
			this.Height = this.Game.RenderSystem.DisplayBounds.Height;
			this.X = ui.Game.RenderSystem.DisplayBounds.Width - WidthPanel;
			this.Y = 0;
			BackColor = InstagramToolPanelColor;
			Anchor = FrameAnchor.Bottom | FrameAnchor.Right | FrameAnchor.Top;
			var frame = new Frame(ui, (WidthPanel - ConstantFrameUI.sfLight50.MeasureString("Эмоциональный город").Width) / 2, offset * 8, 0, 0, "Эмоциональный город", Color.Zero)
			{
				Font = ConstantFrameUI.sfLight50,
				ForeColor = ColorConstant.BlueAzure,
			};
			var sizeText = frame.Font.MeasureString(frame.Text);
			frame.Width = sizeText.Width;
			frame.Height = sizeText.Height;
			this.Add(frame);

			var yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 9;
			this.Add(FrameHelper.createLable(ui, offset * 12, yLastElement, "Фильтр социальных сетей", ConstantFrameUI.sfLight20));

			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 3;
			socialFrameList = new ListBox(ui, offset * 12, yLastElement, 0, 0, "", Color.Zero);
			foreach (var social in socialList)
			{
				var checkbox = FrameHelper.createCheckbox(ui, 0, 0, this.Width - offset * 9,
					28, social, () => {
						needsUpdate = true;
					},
					ui.Game.Content.Load<DiscTexture>(@"ui\checkbox-checked"),
					ui.Game.Content.Load<DiscTexture>(@"ui\checkbox-free"), true, false);
				checkbox.PaddingLeft = 0;
				checkbox.Font = ConstantFrameUI.sfLight18;
				socialFrameList.addElement(checkbox);
			}
			this.Add(socialFrameList);

			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 9;
			this.Add(FrameHelper.createLable(ui, offset * 12, yLastElement, "Фильтр настроения", ConstantFrameUI.sfLight20));

			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 3;
			filterFrameList = new ListBox(ui, offset * 12, yLastElement, 0, 0, "", Color.Zero);
			foreach (var mode in modeList)
			{
				var checkbox = FrameHelper.createCheckbox(ui, 0, 0, this.Width - offset * 9,
					28, mode, () => {
						needsUpdate = true;
					},
					ui.Game.Content.Load<DiscTexture>(@"ui\checkbox-checked"),
					ui.Game.Content.Load<DiscTexture>(@"ui\checkbox-free"), true, false);
				checkbox.PaddingLeft = 0;
				checkbox.Font = ConstantFrameUI.sfLight18;
				filterFrameList.addElement(checkbox);
			}
			this.Add(filterFrameList);

			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 9;
			this.Add(FrameHelper.createLable(ui, offset * 12, yLastElement, "Цветовой фильтр", ConstantFrameUI.sfLight20));

			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 3;
			filterColorFrameList = new ListBox(ui, offset * 12, yLastElement, 0, 0, "", Color.Zero);
			bool startValue = true;
			foreach (var color in filterColorList)
			{
				var checkbox = FrameHelper.createCheckbox(ui, 0, 0, this.Width - offset * 9,
					28, color, () => {
						needsUpdate = true;
					},
					ui.Game.Content.Load<DiscTexture>(@"ui\radio-button-selected"),
					ui.Game.Content.Load<DiscTexture>(@"ui\radio-button-unselected"), startValue);
				checkbox.ActiveColor = ColorConstant.TextColor;
				checkbox.Font = ConstantFrameUI.sfLight18;
				checkbox.Click += (e, a) => {
					ActionCheckBox(checkbox, listFilterColorCheckbox);
				};
				checkbox.PaddingLeft = 0;
				listFilterColorCheckbox.Add(checkbox);
				filterColorFrameList.addElement(checkbox);
				startValue = false;
			}
			this.Add(filterColorFrameList);


			yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 8;
			//var richText1 = "Интерактивное приложение для изучения эмоциональной окраски различных мест в сочинском регионе по сообщениям в популярных социальных сетях. Разработка позволяет визуализировать реальный предварительно собранный и обработанный массив данных социальных сетей, по которым, опираясь на различные методики оценки эмоциональности (отдельно для текстовых сообщений и для фото-контента), система предоставляет «эмоциональную карту» местности.";

			//var richText2 = "Используйте сенсорный экран для манипуляций с картой (перемещение и масштабирование), исследуйте отзывы людей о различных достопримечательностях Сочи – узнайте, где больше всего хороших отзывов, а где много негатива.";
			//         this.Add(new RichTextBlock(ui, offset * 12, yLastElement, this.Width - offset * 24, 0, richText1, Color.Zero, ConstantFrameUI.sfLight18, ConstantFrameUI.sfLight18.CapHeight)
			//         {
			//	ForeColor = new Color(Color.White.ToVector3(), 0.8f),
			//         });

			var info_texture = ui.Game.Content.Load<DiscTexture>("Sochi/block_description-text");
			var info_text = new Frame(ui, offset * 12, yLastElement, info_texture.Width, info_texture.Height, "", Color.Zero)
			{
				//TextAlignment = Alignment.MiddleCenter,
				Image = info_texture,
				//ImageMode = FrameImageMode.Stretched,
				//ImageColor = circleColor,
			};
			this.Add(info_text);
			//yLastElement = this.Children.Last().Y + this.Children.Last().Height + offset * 5;
			//this.Add(new RichTextBlock(ui, offset * 12, yLastElement, this.Width - offset * 24, 0, richText2, Color.Zero, ConstantFrameUI.sfBold15, ConstantFrameUI.sfBold15.CapHeight));

			var logo = ui.Game.Content.Load<DiscTexture>(@"ui\itmologo");
			var xLogo = this.Width/2 - logo.Width/2;
            var yLogo = this.Height - offset * 8 - logo.Height;
            var logoFrame = new Frame(ui, xLogo, yLogo, logo.Width, logo.Height, "", Color.Zero)
            {
                Image=logo,
                Anchor = FrameAnchor.Bottom
            };
            this.Add(logoFrame);
        }


        public void ActionCheckBox(Checkbox checkbox, List<Checkbox> listCheckBox)
        {
            var checkedElement = listCheckBox.FindAll(e => e.IsChecked).ToList();
            if (checkedElement.Any())
            {
                checkedElement.ForEach(e => e.IsChecked = false);
            }
            checkbox.IsChecked = true;
        }
    }
}
