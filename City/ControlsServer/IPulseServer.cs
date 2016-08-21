using System.IO;
using City.Snapshot;
using City.Snapshot.Snapshot;

namespace City.ControlsServer
{
    public interface IPulseServer
    {
        void Initialize(PulseMasterServer pulseMasterServer);
        void LoadLevel(ControlInfo config);
        void UnloadLevel();
        ISnapshot Update();
        void FeedCommand(string id, ICommandSnapshot userCommand);
        string ServerInfo();
    }

//    public interface IPulseServerInteractive : IPulseServer
//    {
//        void Initialize();
//        void LoadLevel(ControlInfo config);
//        void UnloadLevel();
//        byte[] Update();
//        void FeedCommand(string id, byte[] userCommand);
//        string ServerInfo();
//    }
}
