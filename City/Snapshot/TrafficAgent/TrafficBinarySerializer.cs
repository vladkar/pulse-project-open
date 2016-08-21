using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using DistributedTraffic.Emergency;
using MultiagentEngine.Agents;
using SimpleTraffic;

namespace City.Snapshot.TrafficAgent
{
    public class TrafficBinarySerializer : IPulseSnapshotControlSerializer
    {
        public TrafficAgentData GetTrafficAgentDto(AgentBase agent)
        {
            SimpleTrafficAgent currAgent = agent as SimpleTrafficAgent;
            return new TrafficAgentData
            {
                Id = UInt64.Parse(currAgent.Id.Replace("emc","")),
                X = currAgent.Position.Latitude,
                Y= currAgent.Position.Longitude,
                IsEmergency = (currAgent.Mode == BehaviorMode.Emergency),
                direction = GetDirection(currAgent.Position,currAgent.Edge.EndPoint.Position)
            };
        }

        public byte GetDirection(GeoCoordinate point1, GeoCoordinate point2)
        {
            
            //byte[] directions = new byte[2];
            var x = (point2.Latitude - point1.Latitude);
            var y = (point2.Longitude - point1.Longitude);
            /*
            double dist = Math.Sqrt(Math.Pow(x, 2)+ Math.Pow(y, 2));

            var xcos = x/dist;
            var ycos = y/dist;
            */
            
            double angle = Math.Atan2(y,x);
            double degree = angle <= 0 ? angle*(180/Math.PI) : 180 - angle * (180 / Math.PI);
            int bytedegree = (int) ((double) degree*((double)255/360));
            /*
            int bytexcos = (int) (xcos / (0.0390625));
            int byteycos = (int) (ycos/(0.0390625));

            if (bytexcos == 256) bytexcos = 255;
            if (byteycos == 256) byteycos = 255;
            */
            /*
            directions[0] = (byte) bytexcos;
            directions[1] = (byte) byteycos;

            return directions;
            */
            return (byte) bytedegree;
        }

        public TrafficEdgeData GetTrafficEdgeDto(Edge edge)
        {
            return new TrafficEdgeData
            {
                Id = edge.Id,
                AgentsCount = edge.AllAgentsOnTheEdge.Count
            };
        }

        public TrafficEmcPathData GetTrafficEmcPathDto (List<SimpleTrafficAgent> emc)
        {
            List<uint> emcPaths = new List<uint>();
            foreach(var e in emc)
            {
                foreach(var e_id in e.Path)
                {
                    emcPaths.Add(e_id);
                }
            }
            emcPaths = emcPaths.Distinct().ToList();

            return new TrafficEmcPathData
            {
                EmcPaths = emcPaths
            };
        }

        public TrafficCallData GetTrafficCallDto(Call call)
        {

            return new TrafficCallData
            {
                nodeId = call.Patient_Node.Id,
                callState = ConvertCallSateToInt(call.CurState)
            };
        }

        public TrafficSelectData GeTrafficSelectDto(Node node, Hospital hospital)
        {
            return new TrafficSelectData
            {
                emcLat = node.Position.Latitude,
                emcLon = node.Position.Longitude,
                hospLat = hospital.Nearest.Position.Latitude,
                hospLon = hospital.Nearest.Position.Longitude
            };
        }

        public int ConvertCallSateToInt(CallState state)
        {
            switch(state)
            {
                case (CallState.Expectations):
                    return 0;
                case (CallState.Service):
                    return 1;
                case (CallState.WaitingHospitalization):
                    return 2;
                case (CallState.Finish):
                    return 3;
                case (CallState.EMCIsService):
                    return 4;
                default:
                    return 5;
            }
        }


        public void WriteAgent(BinaryWriter sw, TrafficAgentData agent)
        {
            sw.Write(agent.Id);
            sw.Write(agent.X);
            sw.Write(agent.Y);
            sw.Write(agent.IsEmergency);
            sw.Write(agent.direction);
            
        }

        public void WriteEdge(BinaryWriter sw, TrafficEdgeData edge)
        {
            sw.Write(edge.Id);
            sw.Write(edge.AgentsCount);
        }

        public void WriteEmcPaths(BinaryWriter sw , TrafficEmcPathData tremcdata)
        {
            foreach(var edg_id in tremcdata.EmcPaths)
            {
                sw.Write(edg_id);
            }
        }

        public void WriteCall(BinaryWriter sw, TrafficCallData call)
        {
            sw.Write(call.nodeId);
            sw.Write(call.callState);
        }

        public void WriteSelect(BinaryWriter sw, TrafficSelectData line_select)
        {
            sw.Write(line_select.emcLat);
            sw.Write(line_select.emcLon);
            sw.Write(line_select.hospLat);
            sw.Write(line_select.hospLon);
        }

        public void WriteEmcStats(BinaryWriter sw, EmcStatictics emc_stats)
        {
            sw.Write(emc_stats.AvgArrivalTime);
            sw.Write(emc_stats.AvgFirstAidTime);
            sw.Write(emc_stats.AvgFinishTime);
            sw.Write(emc_stats.CurrCallsNumb);
            sw.Write(emc_stats.FinishCallsNumb);
            sw.Write(emc_stats.AllCallsNumber);
            sw.Write(emc_stats.IterationCount);
        }

        public ISnapshot DeserializeSnapshot(BinaryReader sr)
        {
            var snapshot = new TrafficSnapshot();
            if (sr.BaseStream.Length == 0)
            {
                snapshot.Agents = new TrafficAgentData[0];
                return snapshot;
            };

            snapshot.Number = sr.ReadInt32();
            var count = sr.ReadInt32();

            snapshot.Agents = new List<TrafficAgentData>();
            for (var i = 0; i < count; i++)
            {
                snapshot.Agents.Add(ReadAgent(sr));
            }

            var edgecount = sr.ReadInt32();

            snapshot.Edges = new List<TrafficEdgeData>();
            for (var i = 0; i<edgecount;i++)
            {
                snapshot.Edges.Add(ReadEdge(sr));
            }

            var edgesPathsCount = sr.ReadInt32();

            snapshot.EmcPaths = new TrafficEmcPathData();
            snapshot.EmcPaths.EmcPaths = new List<uint>();
            for (var i =0; i< edgesPathsCount; i++)
            {
                snapshot.EmcPaths.EmcPaths.Add(sr.ReadUInt32());
            }

            var callsCount = sr.ReadInt32();

            snapshot.Calls = new List<TrafficCallData>();

            for(var i=0; i< callsCount; i ++ )
            {
                snapshot.Calls.Add(ReadCall(sr));
            }

            snapshot.SelectLines = new List<TrafficSelectData>();

            var slinescount = sr.ReadInt32();

            for (var i = 0; i < slinescount; i++)
            {
                snapshot.SelectLines.Add((ReadSelect(sr)));
            }

            snapshot.emc_Stats = ReadEmcStats(sr);

            return snapshot;
        }

        public byte[] SerializeSnapshot(ISnapshot snapshot)
        {
            var sht = snapshot as TrafficSnapshot;
            if (sht?.Agents == null && sht?.Edges == null) return new byte[0];

            var ms = new MemoryStream();
            var sw = new BinaryWriter(ms);

            sw.Write(sht.Number);
            sw.Write(sht.Agents.Count);

            foreach (var agent in sht.Agents)
            {
                WriteAgent(sw, agent);
            }

            sw.Write(sht.Edges.Count);

            foreach(var edge in sht.Edges)
            {
                WriteEdge(sw,edge);
            }

            sw.Write(sht.EmcPaths.EmcPaths.Count);

            WriteEmcPaths(sw, sht.EmcPaths);

            sw.Write(sht.Calls.Count);

            foreach(var call in sht.Calls)
            {
                WriteCall(sw, call);
            }

            sw.Write(sht.SelectLines.Count);

            foreach (var selectData in sht.SelectLines)
            {
                WriteSelect(sw,selectData);
            }

            WriteEmcStats(sw,sht.emc_Stats);

            return ms.ToArray();
        }

        public ICommandSnapshot DeserializeCommand(BinaryReader sr)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeCommand(ICommandSnapshot snapshot)
        {
            return new byte[0];
        }

        public ISnapshot DeserializeSnapShot(byte[] rawsnapshot)
        {
            using (var sr = new BinaryReader(new MemoryStream(rawsnapshot)))
                return DeserializeSnapshot(sr);
        }

        public TrafficAgentData ReadAgent(BinaryReader sr)
        {
            return new TrafficAgentData
            {
                Id = sr.ReadUInt64(),
                X = sr.ReadDouble(),
                Y = sr.ReadDouble(),
                IsEmergency = sr.ReadBoolean(),
                direction = sr.ReadByte(),
            };
        }

        public TrafficEdgeData ReadEdge(BinaryReader sr)
        {
            return new TrafficEdgeData
            {
                Id = sr.ReadUInt32(),
                AgentsCount = sr.ReadInt32()
            };
        }

        public TrafficCallData ReadCall(BinaryReader sr)
        {
            return new TrafficCallData
            {
                nodeId = sr.ReadUInt64(),
                callState = sr.ReadInt32()
            };
        }

        public TrafficSelectData ReadSelect(BinaryReader sr)
        {
            return new TrafficSelectData
            {
                emcLat = sr.ReadDouble(),
                emcLon = sr.ReadDouble(),
                hospLat = sr.ReadDouble(),
                hospLon = sr.ReadDouble()
            };
        }

        public EmcStatictics ReadEmcStats(BinaryReader sr)
        {
            return new EmcStatictics()
            {
                AvgArrivalTime =  sr.ReadDouble(),
                AvgFirstAidTime = sr.ReadDouble(),
                AvgFinishTime = sr.ReadDouble(),
                CurrCallsNumb = sr.ReadInt32(),
                AllCallsNumber = sr.ReadInt32(),
                FinishCallsNumb = sr.ReadInt32(),
                IterationCount = sr.ReadInt32(),
            };
        }

    }
}

