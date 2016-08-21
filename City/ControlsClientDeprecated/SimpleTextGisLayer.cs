using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;

namespace City.ControlsClient
{
    public class SimpleTextGisLayer : Gis.GisLayer
    {
        public SimpleTextGisLayer(Game engine) : base(engine)
        {
        }

        public override void Draw(GameTime gameTime, ConstantBuffer constBuffer)
        {
        }
    }
}
