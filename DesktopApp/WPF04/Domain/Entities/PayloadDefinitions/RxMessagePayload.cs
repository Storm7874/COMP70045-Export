using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.PayloadDefinitions
{
    /// <summary>
    /// Payload definition for a received message.
    /// </summary>
    public class RxMessagePayload
    {
        public string messagePayload;
        public int messageSize;
        public int messageRssi;
        public int messageSnr;

        public RxMessagePayload(string messagePayload, int messageSize, int messageRssi, int messageSnr)
        {
            this.messagePayload = messagePayload;
            this.messageSize = messageSize;
            this.messageRssi = messageRssi;
            this.messageSnr = messageSnr;
        }
    }


}
