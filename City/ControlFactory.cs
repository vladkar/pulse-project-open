using System;
using City.ControlsClient;
using City.ControlsClient.DomainClient;
using City.ControlsClient.DomainClient.Train;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Serialization;
using City.Snapshot.TrafficAgent;

namespace City
{
    public class ControlFactory
    {
        public static IPulseControl GetClientControl(string name, string scenario = "")
        {
            if (name == GlobalStrings.PulseAgentControl)
                return PulseClientFactory.GetClientControl(scenario);
            if (name == GlobalStrings.InstagramControl)
                return new InstagramControl();
			if (name == GlobalStrings.TrainControl)
				return new TrainControl();
            if (name == GlobalStrings.SochiSentControl)
                return new VKSentControl();
            if (name == GlobalStrings.VkFestControl)
                return new VkFestControl();
            else
                throw new Exception($"Missing control: {name}");
        }


        public static IPulseSnapshotControlSerializer GetReader(ControlInfo ci)
        {
			if (ci.Name == GlobalStrings.PulseAgentControl)
				return new PulseAgentBinarySerializer(ci);
			else if (ci.Name == GlobalStrings.TrafficAgentControl)
				return new TrafficBinarySerializer();
			else if (ci.Name == GlobalStrings.TrainControl)
				return new PulseAgentBinarySerializer(ci);
			else
				throw new Exception($"Missing serializer for control: {ci.Name}");
        }
    }

    public class PulseClientFactory
    {
        public static IPulseControl GetClientControl(string scenario)
        {
            switch (scenario.ToLower())
            {
                case "harsiddhitemple":
                case "mahakaltemple":
                case "simpletrain":
                    return new PulseMicroModelControl();
                case "vkfeststage":
                    return new VkFestAgentControl();
                default:
                    throw new Exception($"Missing pulse client control for scenario: {scenario}");
            }
        }
    }
}