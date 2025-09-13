using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Packet
{
    /// <summary>
    /// Class representation of the fields within a packet header
    /// </summary>
    public class PacketHeader
    {
        //The type of the packet - DICT, EDICT, or RAW.
        public string PacketType;

        //Ack flag
        public bool Acknowledgement;

        //Rebroadcast flag
        public bool Rebroadcast;

        //Raw, assembled header byte
        public byte AssembledHeader { get; set; }

        //Constructor
        public PacketHeader(string packetType, bool acknowledgement, bool rebroadcast)
        {
            this.PacketType = packetType;
            this.Acknowledgement = acknowledgement;
            this.Rebroadcast = rebroadcast;
        }
    }
}
