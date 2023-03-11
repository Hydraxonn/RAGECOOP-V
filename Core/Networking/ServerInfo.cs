﻿using System;

namespace RageCoop.Core
{
    /// <summary>
    ///     A json object representing a server's information as annouced to master server.
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        public string address { get; set; }
        public int port { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public int players { get; set; }
        public int maxPlayers { get; set; }
        public string country { get; set; }
        public string description { get; set; }
        public string website { get; set; }
        public string gameMode { get; set; }
        public string language { get; set; }

        public bool useP2P { get; set; }

        public bool useZT { get; set; }

        public string ztID { get; set; }

        public string ztAddress { get; set; }
        public string publicKeyModulus { get; set; }
        public string publicKeyExponent { get; set; }
    }
}