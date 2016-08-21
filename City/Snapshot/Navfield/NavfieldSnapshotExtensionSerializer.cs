using System;
using System.IO;
using System.Linq;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using NavigField;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.NavField;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace City.Snapshot.Navfield
{
    public class NavfieldSnapshotExtensionSerializer : IPulseSnapshotControlExtensionSerializer
    {
        public ISnapshotExtension Deserialize(BinaryReader br)
        {
            var ext = new NavfieldSnapshotExtension();

            ext.Size = br.ReadDouble();
            ext.BottomLeft = new PulseVector2 {X = br.ReadDouble(), Y = br.ReadDouble()};
            ext.Level = br.ReadByte();

            var cols = br.ReadInt32();
            var rows = br.ReadInt32();

            ext.Grid = new double[cols][];

            for (int i = 0; i < cols; i++)
            {
                ext.Grid[i] = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    ext.Grid[i][j] = br.ReadDouble();
                }
            }

            return ext;
        }

        public void Serialize(ISnapshotExtension extension, BinaryWriter sw)
        {
            var ext = extension as NavfieldSnapshotExtension;
            if (ext == null) throw new Exception("Wrong snapshot exntension");

            sw.Write(ext.Size);
            sw.Write(ext.BottomLeft.X);
            sw.Write(ext.BottomLeft.Y);
            sw.Write(ext.Level);

            sw.Write(ext.Grid.Length);
            sw.Write(ext.Grid[0].Length);

            for (int i = 0; i < ext.Grid.Length; i++)
            {
                for (int j = 0; j < ext.Grid[0].Length; j++)
                {
                    sw.Write(ext.Grid[i][j]);
                }
            }
        }

        //TODO very bad: static, hardcode & some other shit
        public ISnapshotExtension GetSnapshotExtension(IPulseMap map)
        {
            if (PoiTreeNavfieldNavigator.NavFieldRegister == null || PoiTreeNavfieldNavigator.NavFieldRegister.Count <= 0) return new NavfieldSnapshotExtension { Grid = new []{new double[0]}};

            var allpois = map.Levels.Values.SelectMany(l => l.PointsOfInterest);

//            var field = (PoiTreeNavfieldNavigator.NavFieldRegister.First().Value as NavfieldCalc).NF;
            var fpoi = allpois.RandomChoise();
            var field = (PoiTreeNavfieldNavigator.NavFieldRegister[fpoi.ObjectId] as NavfieldCalc).NF;

            var grid = new double[field.NavigFieldArray.GetLength(0)][];

            for (var i = 0; i < field.NavigFieldArray.GetLength(0); i++)
            {
                grid[i] = new double[field.NavigFieldArray.GetLength(1)];

                for (var j = 0; j < field.NavigFieldArray.GetLength(1); j++)
                {
                    grid[i][j] = field.NavigFieldArray[i, j].angle;
                }
            }

            var ext = new NavfieldSnapshotExtension
            {
                Id = 1,
                Size = 0.25,
                BottomLeft = new PulseVector2(0, 0),
                Grid = grid,
                Level = (byte)fpoi.Level
            };

            return ext;
        }
    }
}