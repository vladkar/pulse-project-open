
namespace Pulse.Common.Scenery.Loaders
{
    public abstract class AbstractFileDataReader : AbstractDataBroker
    {
        public string DataDirectory { get; protected set; }
        public string DataFile { get; protected set; }

        public string DataFullPath
        {
            get { return DataDirectory + DataFile; }
        }

        protected AbstractFileDataReader(string direPath) : base()
        {
            DataDirectory = direPath;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, DataFullPath);
        }
    }
}