using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF04.Domain.Entities.Dictionaries;

namespace WPF04.Domain.Entities.Packet
{
    /// <summary>
    /// Class to represent a message Packet
    /// </summary>
    public class Packet
    {
        //Packet header
        public PacketHeader PacketHeader { get; set; }

        //Source address of the packet
        public string SrcAddress { get; set; }

        //List of word references that make up the non-encoded payload
        public List<WordRef> DictPayload { get; set; }

        //Raw message payload
        public string RawMessage { get; set; }

        //BlockID for encrypted messages
        public int BlockID { get; set; }
    }
}
