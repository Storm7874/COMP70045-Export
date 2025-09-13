using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Message
{
    /// <summary>
    /// Class Representing a Transmit Message
    /// </summary>
    public class TxMessage
    {
        //Raw message payload
        public string messagePayload;

        //Message type
        public string messageType;

        //Constructor
        public TxMessage(string messagePayload, string messageType)
        {
            this.messagePayload = messagePayload;
            this.messageType = messageType;
        }


        /// <summary>
        /// Generates a JSON Object representing the message
        /// </summary>
        /// <returns></returns>
        public JsonObject GenerateMessageJson()
        {
            //Add message parameters as nodes to json object
            var messageObject = new JsonObject
            {
                ["messagePayload"] = messagePayload,
                ["messageType"] = messageType
            };

            return messageObject;
        }
    }
}
