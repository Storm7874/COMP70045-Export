using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Message
{
    /// <summary>
    /// Class representing a received message
    /// </summary>
    public class RxMessage
    {
        // Message payload
        public string messagePayload { get; set; }

        //Message Metadata
        public int messageSize { get; set; }
        public int messageRssi { get; set; }
        public int messageSnr { get; set; }

        /// <summary>
        /// Constructor for RxMessage
        /// </summary>
        /// <param name="messagePayload"></param>
        /// <param name="messageSize"></param>
        /// <param name="messageRssi"></param>
        /// <param name="messageSnr"></param>
        public RxMessage(string messagePayload, int messageSize, int messageRssi, int messageSnr)
        {
            this.messagePayload = messagePayload;
            this.messageSize = messageSize;
            this.messageRssi = messageRssi;
            this.messageSnr = messageSnr;
        }
    }
}
