using Pulse.Common.Model.Environment.Map;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model
{
    public interface IUniqueObject
    {
        string Name { set; get; }
        string ObjectId { set; get; }
        string Description { set; get; }
    }

    public interface ISpatialObject
    {
        PulseVector2 Point { set; get; }
    }

    public interface IPseudo3DObject : ISpatialObject
    {
        int Level { set; get; }
    }

    public interface IZonable
    {
        Zone Zone { set; get; }
    }
}