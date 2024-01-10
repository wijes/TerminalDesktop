using System;
using Unity.Netcode;
using UnityEngine;

namespace TerminalDesktopMod.Sync
{
    public class WindowSync : INetworkSerializable
    {
        public bool ChangeCollapsed;
        public bool IsCollapsed;
        
        public bool SyncPosition;
        public Vector3 Position;
        
        public bool SyncScale;
        public Vector2 Scale;

        public bool SyncCustomInt;
        public int CustomInt;
        
        public bool SyncCustomBool;
        public bool CustomBool;
        
        public bool SyncCustomFloat;
        public float CustomFloat;
        
        public bool SyncCustomString;
        public string CustomString;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ChangeCollapsed);
            if (ChangeCollapsed)
                serializer.SerializeValue(ref IsCollapsed);
            
            serializer.SerializeValue(ref SyncPosition);
            if (SyncPosition)
                serializer.SerializeValue(ref Position);
            
            serializer.SerializeValue(ref SyncScale);
            if (SyncScale)
                serializer.SerializeValue(ref Scale);
            
            serializer.SerializeValue(ref SyncCustomInt);
            if (SyncCustomInt)
                serializer.SerializeValue(ref CustomInt);
            
            serializer.SerializeValue(ref SyncCustomFloat);
            if (SyncCustomFloat)
                serializer.SerializeValue(ref CustomFloat);
            
            serializer.SerializeValue(ref SyncCustomBool);
            if (SyncCustomBool)
                serializer.SerializeValue(ref CustomBool);
            
            serializer.SerializeValue(ref SyncCustomString);
            if (SyncCustomString)
                serializer.SerializeValue(ref CustomString);
        }
    }
}