using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.ControlsClient;
using City.Models;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Utils;

namespace City.UIFrames.FrameElement
{
    public class ViewFrame : Frame
    {
        public RenderLayer viewLayers;
        private SpriteLayer sLayer;

        private readonly FrameProcessor ui;
        private readonly Texture beamTexture;
        private bool IsDrag;
        private bool IsIn;

        public Action<List<Polygon>> SendPolygon { get; set; }

        public bool IsTool = false;
        public GlobeCamera camera;
        private Vector2 previousMousePosition;

        private List<Polygon> listPolygons; 

        public ViewFrame(FrameProcessor ui) : base(ui)
        {
        }

        public ViewFrame(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor, Color borderColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            Border = 1;
            BorderColor = borderColor;
            beamTexture = Game.Content.Load<DiscTexture>(@"ui\beam.tga");
        }

        public void init(GlobeCamera camera, RenderLayer rl)
        {
            viewLayers = rl;
            sLayer = new SpriteLayer(Game.RenderSystem, 128);
            viewLayers.SpriteLayers.Add(sLayer);

            this.camera = camera;
            camera.Viewport = new Viewport(0, 0, Width, Height);
            camera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);

            this.Game.RenderSystem.DisplayBoundsChanged +=
                (sender, args) =>
                    camera.Viewport =
                        new Viewport(0, 0, Width,Height);
            

            this.MouseDown += (sender, args) =>
            {
                IsDrag = true;
            };
            this.MouseUp += (sender, args) =>
            {
                IsDrag = false;
            };

            this.MouseIn += (sender, args) =>
            {
                IsIn = true;
            };

            this.MouseOut += (sender, args) =>
            {
                IsIn = false;
            };


            // Input bindings
            this.MouseWheel += (sender, args) => {
                if (!IsIn) return;
                if (args.Wheel > 0)
                    camera.CameraZoom(-0.05f);
                else if (args.Wheel < 0)
                    camera.CameraZoom(0.05f);
            };

            this.MouseMove += (sender, args) =>
            {
                if (!IsIn) return;
                if (IsDrag)
                {
                    camera.MoveCamera(previousMousePosition, new Vector2(args.X, args.Y));
                }
                previousMousePosition = new Vector2(args.X, args.Y);
            };

            listPolygons = new List<Polygon>() {};
            this.Click += (sender, args) =>
            {
                if (MainFrameUI.Tool.Option == MainFrameUI.Tool.ToolOption.DrawingPolygon && !IsTool)
                {
                    MainFrameUI.Tool.Option = MainFrameUI.Tool.ToolOption.None;
                    IsTool = true;
                    listPolygons.Add(new Polygon(generateId()));
                }
                DVector2 mousePositionSpherical;
                camera.ScreenToSpherical(args.X, args.Y, out mousePositionSpherical);
                if (MainFrameUI.Tool.Option == MainFrameUI.Tool.ToolOption.DeletePolygon)
                {
                    MainFrameUI.Tool.Option = MainFrameUI.Tool.ToolOption.None;

                    foreach (var polygon in listPolygons)
                    {
                        if (polygon.IsPointInPolygon(mousePositionSpherical))
                        {
                            listPolygons.Remove(polygon);
                            SendPolygon?.Invoke(listPolygons);
                            break;
                        }
                            
                    }
                }
                if (!IsTool) return;
                handleClick(args.Key, mousePositionSpherical);
            };
        }

        private int generateId()
        {
            var listId = listPolygons.Select(e => e.Id).ToList();
            listId.Sort();
            int newId = 0;
            foreach (var id in listId)
            {
                while (newId==id)
                {
                    newId++;
                }
            }
            return newId;
        }

        public void handleClick(Keys key, DVector2 mousePositionSpherical)
        {
            // undo last point
            if (key == Keys.RightButton)
            {
                if (listPolygons.Last().listPoint.Count > 0)
                    listPolygons.Last().cancelLastPoint();
                else
                {
                    listPolygons.RemoveAt(listPolygons.Count - 1);
                    IsTool = false;
                }
                return;
            }

            if (!listPolygons.Last().IsСontour(mousePositionSpherical))
            {
                listPolygons.Last().AddPoint(mousePositionSpherical);
            }
            else
            {
                listPolygons.Last().IsLast = false;
                IsTool = false;
                // TODO: Delete last polygon?
                SendPolygon?.Invoke(listPolygons);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            InvokeCameraLogic(gameTime);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            base.DrawFrame(gameTime, sb, clipRectIndex);

            foreach (var polygon in listPolygons)
            {
                polygon.Draw(sLayer, beamTexture, this.Game.Mouse.Position - new Vector2(GlobalRectangle.X, GlobalRectangle.Y), camera);
            }

            if (MainFrameUI.Tool.Option!=MainFrameUI.Tool.ToolOption.None || IsTool)
            {
                this.Font.DrawString(sb, "*", this.Game.Mouse.Position.X, this.Game.Mouse.Position.Y, Color.White, clipRectIndex);
            }
        }

        private void InvokeCameraLogic(GameTime gameTime)
        {
            // moving
            if (camera == null || !IsIn) return;
            var dir = DVector3.Zero;
            if (Game.Keyboard.IsKeyDown(Keys.W)) { dir.X += 1.0; }
            if (Game.Keyboard.IsKeyDown(Keys.S)) { dir.X -= 1.0; }
            if (Game.Keyboard.IsKeyDown(Keys.A)) { dir.Z += 1.0; }
            if (Game.Keyboard.IsKeyDown(Keys.D)) { dir.Z -= 1.0; }
            if (Game.Keyboard.IsKeyDown(Keys.Space)) { dir.Y += 3.0; }
            if (Game.Keyboard.IsKeyDown(Keys.C)) { dir.Y -= 3.0; }
            if (dir.Length() != 0.0)
                dir.Normalize();
            camera.MoveFreeSurfaceCamera(dir);


            // rotation
            double fy = 0;
            double fp = 0;

            if (Game.Keyboard.IsKeyDown(Keys.Left)) fy -= gameTime.ElapsedSec * 0.7;
            if (Game.Keyboard.IsKeyDown(Keys.Right)) fy += gameTime.ElapsedSec * 0.7;
            if (Game.Keyboard.IsKeyDown(Keys.Up)) fp -= gameTime.ElapsedSec * 0.7;
            if (Game.Keyboard.IsKeyDown(Keys.Down)) fp += gameTime.ElapsedSec * 0.7;
            camera.RotateFreeSurfaceCamera(fy, fp);

            // switch mode of camera
            if (Game.Keyboard.IsKeyDown(Keys.LeftShift)) { camera.CameraState = GlobeCamera.CameraStates.ViewToPoint; }
            if (Game.Keyboard.IsKeyDown(Keys.RightShift)) { camera.CameraState = GlobeCamera.CameraStates.TopDown; }
            if (Game.Keyboard.IsKeyDown(Keys.LeftControl))
            {
                camera.ToggleFreeSurfaceCamera();
            }

            // rotation
            if (Game.Keyboard.IsKeyDown(Keys.MiddleButton) &&
                camera.CameraState == GlobeCamera.CameraStates.ViewToPoint)
            {
                camera.RotateViewToPointCamera(Game.Mouse.PositionDelta);
            }
            if (Game.Keyboard.IsKeyDown(Keys.RightButton) &&
                camera.CameraState == GlobeCamera.CameraStates.FreeSurface)
            {
                camera.RotateFreeSurfaceCamera(Game.Mouse.PositionDelta);
            }
        }
    }
}
