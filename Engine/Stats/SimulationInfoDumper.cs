using System.IO;
using MongoDB.Bson;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.MultiagentEngine.Stats
{
    public class SimulationInfoDumper : IFinalStatistics
    {
        public string FileName { get; set; }

        public SimulationInfoDumper(string fileName)
        {
            FileName = fileName;
        }

        public void Finalization()
        {

        }

        public void Update(SimulationEngine engine)
        {
            var f = File.CreateText(FileName);

            f.Write(engine.Info.ToJson());

            f.Close();
        }
    }
}
