using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Message
{
    /// <summary>
    /// Class representing a message displayed in the UI
    /// </summary>
    public class DisplayMessage
    {
        // Message payload
        public string messagePayload { get; set; }

        //Transmitter ID
        public int TxID { get; set; }

        //Message Metadata
        public int messageSize { get; set; }
        public int messageRssi { get; set; }
        public int messageSnr { get; set; }
        public string messageType { get; set; }
        public string messageTime { get; set; }
        public string messageSource { get; set; }

        // Flags for message status
        public bool rebroadcast { get; set; } = false;
        public bool ack { get; set; } = false;

        /// <summary>
        /// Constructor for DisplayMessage
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="TxID"></param>
        /// <param name="messagePayload"></param>
        /// <param name="messageSize"></param>
        /// <param name="messageRssi"></param>
        /// <param name="messageSnr"></param>
        /// <param name="messageTime"></param>
        public DisplayMessage(string messageType, int TxID, string messagePayload, int messageSize, int messageRssi, int messageSnr, DateTime messageTime)
        {
            this.messageType = messageType;
            this.TxID = TxID;
            this.messagePayload = messagePayload;
            this.messageSize = messageSize;
            this.messageRssi = messageRssi;
            this.messageSnr = messageSnr;
            this.messageTime = messageTime.ToString("HH:mm:ss"); // Format the time as HH:mm:ss
        }
    }
}
