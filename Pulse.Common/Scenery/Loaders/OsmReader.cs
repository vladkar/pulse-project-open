using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Pulse.Common.Model.Environment.World;

namespace Pulse.Common.Scenery.Loaders
{
    public class OsmReader : AbstractFileDataReader
    {
        public  List<OsmNode> Nodes;
        public List<OsmWay> Ways; 
        
        public OsmReader(string direPath, string fileName) : base(direPath)
        {
            Name = "OSM reader";
            DataFile = fileName;
        }

        protected override void LoadData()
        {
            var doc = XDocument.Load(DataFullPath);
            Nodes = GetNodes(doc);
            Ways = GetWays(doc);
        }
        
        private List<OsmNode> GetNodes(XDocument doc)
        {
            Debug.Assert(doc.Root != null, "doc.Root != null");

            return doc.Root.Elements("node").Select(n => new OsmNode
                    {
                        Id = Int64.Parse(n.Attribute("id").Value),
                        Coords = new GeoCoords
                        {
                            Lat = Double.Parse(n.Attribute("lat").Value, CultureInfo.InvariantCulture),
                            Lon = Double.Parse(n.Attribute("lon").Value, CultureInfo.InvariantCulture)
                        },
                        Tags = GetTags(n)
                    }).ToList();
        }

        private IList<OsmTag> GetTags(XElement xElement)
        {
            return xElement.Elements("tag").Select(t => new OsmTag
            {
                Key = t.Attribute("k").Value,
                Value = t.Attribute("v").Value
            }).ToList();
        }

        private IList<OsmTag> GetRefs(XElement xElement)
        {
            return xElement.Elements("tag").Select(t => new OsmTag
            {
                Key = t.Attribute("k").Value,
                Value = t.Attribute("v").Value
            }).ToList();
        }

        private List<OsmWay> GetWays(XDocument doc)
        {
            Debug.Assert(doc.Root != null, "doc.Root != null");

            return doc.Root.Elements("way").Select(w => new OsmWay()
            {
                Id = Int64.Parse(w.Attribute("id").Value),
                Tags = GetTags(w),
                NodeFefs = w.Elements("nd").Select(n => Int64.Parse(n.Attribute("ref").Value)).ToList()
            }).ToList();
        }
    }

    public class OsmNode
    {
        public long Id { set; get; }
        public GeoCoords Coords { set; get; }
        public IList<OsmTag> Tags { set; get; } 
    }

    public class OsmTag
    {
        public string Key { set; get; }
        public string Value { set; get; }
    }

    public class OsmWay
    {
        public long Id { set; get; }
        public IList<long> NodeFefs { set; get; } 
        public IList<OsmTag> Tags { set; get; } 
    }
}
