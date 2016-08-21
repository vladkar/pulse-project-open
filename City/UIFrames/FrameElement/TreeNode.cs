using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames.FrameElement
{
    public class TreeNode : Frame
    {
        public bool IsExpand = false;

        public int OffsetChild;
        public Texture ExpandPicture;
        public Texture CollapsePicture;

        private int HeightCollaps;
        private int HeightExpand;

        public Color backColorMainNode;
        private List<Frame> listNode;

        public TreeNode(FrameProcessor ui) : base(ui)
        {
            //            init(ui);
            listNode = new List<Frame>();
        }

        public TreeNode(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        private void init(FrameProcessor ui)
        {

            // for resize
            HeightCollaps = this.Height;
            this.HeightExpand = this.Height;
            listNode = new List<Frame>();

            // for expand/collaps
            if (Text != "")
            {
                this.Click += (sender, args) => {
                                    IsExpand = !IsExpand;
                                    expandNodes(IsExpand);
                                };
            }
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex )
        {
            if (Text == "" || ExpandPicture == null || CollapsePicture == null)
                return;
            var xButton = this.GlobalRectangle.X + PaddingLeft + TextOffsetX/2 - ConstantFrameUI.sizeExpandButton / 2;
            var yButton = this.GlobalRectangle.Y + this.HeightCollaps/2 - ConstantFrameUI.sizeExpandButton / 2;

            var xText = this.GlobalRectangle.X + PaddingLeft + TextOffsetX;
            var yText = this.GlobalRectangle.Y + this.HeightCollaps / 2 + this.Font.CapHeight/2;
            var whiteTexture = this.Game.RenderSystem.WhiteTexture;
            sb.Draw(whiteTexture, new Rectangle(this.GlobalRectangle.X, this.GlobalRectangle.Y, this.Width, this.HeightCollaps), backColorMainNode, clipRectIndex);
            sb.Draw(IsExpand ? ExpandPicture : CollapsePicture, new Rectangle(xButton, yButton, ConstantFrameUI.sizeExpandButton, ConstantFrameUI.sizeExpandButton), Color.White, clipRectIndex);
            Font.DrawString(sb, Text, xText, yText, Color.White, clipRectIndex, 0, true);
        }

        public void addNode(Frame node)
        {
            initNode(node);
            listNode.Add(node);
            if (IsExpand)
            {
                this.Add(node);
                this.Height += node.Height;
            }
//                expandNodes(IsExpand);
            if (this.Width < node.Width + node.X)
                this.Width = node.Width + node.X;
        }

        public void removeNode(Frame node)
        {
            this.Remove(node);
            listNode.Remove(node);
            Height -= node.Height;
            HeightExpand -= node.Height;
            resizeTree();
        }

        public void removeAllNode()
        {
            foreach (var node in listNode.ToArray())
            {
                this.Remove(node);
                listNode.Remove(node);
            }
            HeightExpand = HeightCollaps;
            resizeTree();
        }

        private void initNode(Frame node)
        {
            node.X = PaddingLeft + TextOffsetX + OffsetChild;
            if (!IsExpand)
            {
                node.Y = this.HeightExpand;
                this.HeightExpand += node.Height;
            }
            else
            {
                node.Y = this.Height;
            }
        }

        public void expandNodes(bool isExpand)
        {
            foreach (var node in listNode)
            {
                if (isExpand)
                {
                    node.Y = this.Height;
                    this.Add(node);
                    this.Height += node.Height;
                }
                else
                {
                    this.Remove(node);
                    this.Height -= node.Height;
                }
            }

            if (this.Parent is TreeNode)
            {
                var treeNode = (TreeNode)this.Parent;
                treeNode.resizeTree();
            }
        }

        private void resizeTree()
        {
            this.Height = HeightCollaps;
            foreach (var node in listNode)
            {
                initNode(node);
                this.Height += node.Height;
            }
            if (this.Parent is TreeNode)
            {
                var treeNode = (TreeNode)this.Parent;
                treeNode.resizeTree();
            }
        }
    }
}
