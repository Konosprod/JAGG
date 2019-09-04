using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;



[System.Serializable]
public class ReplayIndexFile : ISerializable
{
    [System.Serializable]
    public class ReplayFileMetaData : ISerializable
    {
        public string fileName;
        public string mapName;
        public string replayName;
        public string date;
        public List<string> playerNames;

        public ReplayFileMetaData(string fn, string mn, string rn, string d, List<string> pn)
        {
            fileName = fn;
            mapName = mn;
            replayName = rn;
            date = d;
            playerNames = pn;
        }

        protected ReplayFileMetaData(SerializationInfo info, StreamingContext context)
        {
            fileName = info.GetString("fileName");
            mapName = info.GetString("mapName");
            replayName = info.GetString("replayName");
            date = info.GetString("date");
            playerNames = (List<string>)info.GetValue("playerNames", typeof(List<string>));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("fileName", fileName);
            info.AddValue("mapName", mapName);
            info.AddValue("replayName", replayName);
            info.AddValue("date", date);
            info.AddValue("playerNames", playerNames);
        }
    }

    public int replayFileCount = 0;
    public List<ReplayFileMetaData> replayInfos;

    public ReplayIndexFile()
    {
        replayInfos = new List<ReplayFileMetaData>();
    }

    protected ReplayIndexFile(SerializationInfo info, StreamingContext context)
    {
        replayFileCount = info.GetInt32("fileCount");
        replayInfos = (List<ReplayFileMetaData>)info.GetValue("replayInfos", typeof(List<ReplayFileMetaData>));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("fileCount", replayFileCount);
        info.AddValue("replayInfos", replayInfos);
    }
}
