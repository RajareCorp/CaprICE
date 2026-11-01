using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CaprICE.Services
{
    [DataContract]
    public class ChatMessageData
    {
        [DataMember(Name = "role")] public string Role { get; set; }
        [DataMember(Name = "content")] public string Content { get; set; }
    }

    [DataContract]
    public class SightingData
    {
        [DataMember(Name = "at", EmitDefaultValue = false)] public string At { get; set; }
        [DataMember(Name = "saw", EmitDefaultValue = false)] public string Saw { get; set; }
        [DataMember(Name = "said", EmitDefaultValue = false)] public string Said { get; set; }
        [DataMember(Name = "asked", EmitDefaultValue = false)] public bool Asked { get; set; }
    }

    [DataContract]
    public class SavedChatData
    {
        [DataMember(Name = "at")] public string At { get; set; }
        [DataMember(Name = "messages")] public List<ChatMessageData> Messages { get; set; }
        [DataMember(Name = "seen", EmitDefaultValue = false)] public List<SightingData> Seen { get; set; }
    }

    public static class ChatHistoryManager
    {
        public static string GetChatFilePath()
        {
            return Path.Combine(CapriDataPaths.DirectoryPath, "chat.json");
        }

        public static SavedChatData LoadData()
        {
            string path = GetChatFilePath();
            if (!File.Exists(path))
            {
                return new SavedChatData
                {
                    At = DateTime.Now.ToString("o"),
                    Messages = new List<ChatMessageData>(),
                    Seen = new List<SightingData>()
                };
            }

            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    var serializer = new DataContractJsonSerializer(typeof(SavedChatData));
                    SavedChatData data = (SavedChatData)serializer.ReadObject(fs);
                    if (data == null) data = new SavedChatData();
                    if (data.Messages == null) data.Messages = new List<ChatMessageData>();
                    if (data.Seen == null) data.Seen = new List<SightingData>();
                    return data;
                }
            }
            catch
            {
                return new SavedChatData
                {
                    At = DateTime.Now.ToString("o"),
                    Messages = new List<ChatMessageData>(),
                    Seen = new List<SightingData>()
                };
            }
        }

        public static bool SaveData(SavedChatData data, out string error)
        {
            error = null;
            try
            {
                string path = GetChatFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (MemoryStream ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(SavedChatData));
                    serializer.WriteObject(ms, data);
                    File.WriteAllBytes(path, ms.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}