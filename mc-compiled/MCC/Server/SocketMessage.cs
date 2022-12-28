﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server
{
    /// <summary>
    /// Implementation of an MCC socket message.
    /// </summary>
    public class SocketMessage
    {
        const string ACTION_FIELD = "action";

        internal string actionID;
        internal Dictionary<string, JToken> dataBlocks;

        public SocketMessage(string actionID, Dictionary<string, JToken> dataBlocks)
        {
            this.actionID = actionID;
            this.dataBlocks = dataBlocks;
        }
        public SocketMessage(JObject parse)
        {
            this.actionID = parse[ACTION_FIELD].ToString();
            this.dataBlocks = new Dictionary<string, JToken>();

            foreach(JProperty property in parse.Properties())
            {
                string name = property.Name;
                if (name.Equals(ACTION_FIELD))
                    continue;

                this.dataBlocks[name] = property.Value;
            }
        }
    }
}
