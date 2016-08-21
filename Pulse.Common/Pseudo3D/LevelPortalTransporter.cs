using System.Collections.Generic;
using Pulse.Common.Model;

namespace Pulse.Common.Pseudo3D
{
    public class LevelPortalTransporter : IUniqueObject
    {
        public IList<ILevelPortal> Exits { set; get; }
        public IList<ILevelPortal> Enters { set; get; }

        public IDictionary<int, IList<ILevelPortal>> Exits1 { set; get; }
        public IDictionary<int, IList<ILevelPortal>> Enters1 { set; get; }

        #region IUniqueObject
        public string Name { get; set; }
        public string ObjectId { get; set; }
        public string Description { get; set; }
        #endregion
    }
}
