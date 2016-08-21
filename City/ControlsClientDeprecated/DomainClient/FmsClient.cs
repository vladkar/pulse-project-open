using System.IO;
using City.Snapshot.Snapshot;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Input;
using Newtonsoft.Json;

namespace City.ControlsClient.DomainClient
{
    public class FmsClient : AbstractPulseClient
	{
		private HeatMapLayer	heatMap;
		private PointsGisLayer	buildings;

        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo serverInfo)
        {
            base.LoadLevel(serverInfo);
            heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 29.425507, 30.701294, 60.252843, 59.759508, 30, 512, 512, new MercatorProjection());
            heatMap.MaxHeatMapLevel = 1400;
            heatMap.InterpFactor = 1.0f;
            heatMap.HeatMapTransparency = 1.0f;

            //heatMap.ClearData();

            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            using (StreamReader file = File.OpenText(dir + @"SaintPetersburg\fms_spb_objects.json"))
            {
                var serializer = new JsonSerializer();
                var fmsObjects = (FmsObject[])serializer.Deserialize(file, typeof(FmsObject[]));

                for (int i = 0; i < fmsObjects.Length; i++)
                {
                    heatMap.AddValue(fmsObjects[i].Lon, fmsObjects[i].Lat, (float)fmsObjects[i].Amount);
                }
            }

            heatMap.UpdateHeatMap();
            GisLayers.Add(heatMap);
            heatMap.IsVisible = true;
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
			//if (Game.Keyboard.IsKeyDown(Keys.M))
			//	heatMap.MaxHeatMapLevel += 10;
			//if (Game.Keyboard.IsKeyDown(Keys.N))
			//	heatMap.MaxHeatMapLevel -= 10;

			if (Game.Keyboard.IsKeyDown(Keys.N))
				heatMap.IsVisible = false;
			if (Game.Keyboard.IsKeyDown(Keys.M))
				heatMap.IsVisible = true;

            return null;
        }

		public override string UserInfo()
		{
			return "";
		}

		public class FmsObject
		{
			[JsonProperty("OBJECTID")]
			public string Id { set; get; }

			[JsonProperty("ÒÈÏ_ÇÄÀÍÈß")]
			public string Type1 { set; get; }

			[JsonProperty("ÍÀÈÌÅÍÎÂÀÍ")]
			public string Type2 { set; get; }

			[JsonProperty("Y_WGS84")]
			public double Lat { set; get; }

			[JsonProperty("X_WGS84")]
			public double Lon { set; get; }

			[JsonProperty("AMOUNT_OF_")]
			public double Amount { set; get; }

			public FmsObject()
			{
			}
		}
	}
}