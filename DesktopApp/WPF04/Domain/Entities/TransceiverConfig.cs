using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities
{
    /// <summary>
    /// A representation of the main Transceiver Configuration struct from device firmware
    /// </summary>
    public class TransceiverConfig
    {
        //Main transceiver frequency
        public UInt32 RF_FREQUENCY { get; set; }

        //Invert RF IQ
        public int RF_IQ_INVERSION { get; set; }

        //Output power in dBm
        public int TX_OUTPUT_POWER { get; set; }

        //LoRa Bandwidth
        public int LORA_BANDWIDTH { get; set; }

        //Spreading Factor/SF
        public int LORA_SPREADING_FACTOR { get; set; }

        //Coding Rate
        public int LORA_CODINGRATE { get; set; }

        //Length of LoRa packet preamble
        public int LORA_PREAMBLE_LENGTH { get; set; }

        //???
        public int LORA_SYMBOL_TIMEOUT { get; set; }

        //Fix the payload length of LoRa Tx? packets
        public bool LORA_FIX_LENGTH_PAYLOAD_ON { get; set; }

        //Invert LoRa packets? (See line 16?)
        public bool LORA_IQ_INVERSION_ON { get; set; }

        //Symbol timeout on RX
        public int RX_SYM_TIMEOUT { get; set; }

        //Rx fixed or variable packet length
        public int RX_FIXED_LENGTH_PACKETS { get; set; }

        //If fixed, how long?
        public int RX_FIXED_PAYLOAD_LENGTH { get; set; }

        //Enable CRC for RX packet decoding
        public int RX_CRC_ENABLED { get; set; }

        //RX Intra-packet frequency hopping control
        public int RX_INTRA_PACKET_FREQ_HOP_ENABLED { get; set; }
        public int RX_INTRA_PACKET_HOP_PERIOD { get; set; }

        //RX IQ Inversion
        public int RX_IQ_INVERSION { get; set; }

        //Continued RX mode
        public bool RX_RECEP_CONT { get; set; }

        public UInt32 RX_CS_MS { get; set; }

        /// <summary>
        /// Constructor for TransceiverConfig
        /// </summary>
        /// <param name="rF_FREQUENCY"></param>
        /// <param name="rF_IQ_INVERSION"></param>
        /// <param name="tX_OUTPUT_POWER"></param>
        /// <param name="lORA_BANDWIDTH"></param>
        /// <param name="lORA_SPREADING_FACTOR"></param>
        /// <param name="lORA_CODINGRATE"></param>
        /// <param name="lORA_PREAMBLE_LENGTH"></param>
        /// <param name="lORA_SYMBOL_TIMEOUT"></param>
        /// <param name="lORA_FIX_LENGTH_PAYLOAD_ON"></param>
        /// <param name="lORA_IQ_INVERSION_ON"></param>
        /// <param name="rX_SYM_TIMEOUT"></param>
        /// <param name="rX_FIXED_LENGTH_PACKETS"></param>
        /// <param name="rX_FIXED_PAYLOAD_LENGTH"></param>
        /// <param name="rX_CRC_ENABLED"></param>
        /// <param name="rX_INTRA_PACKET_FREQ_HOP_ENABLED"></param>
        /// <param name="rX_INTRA_PACKET_HOP_PERIOD"></param>
        /// <param name="rX_IQ_INVERSION"></param>
        /// <param name="rX_RECEP_CONT"></param>
        /// <param name="rX_CS_MS"></param>
        public TransceiverConfig(uint rF_FREQUENCY, int rF_IQ_INVERSION, int tX_OUTPUT_POWER, int lORA_BANDWIDTH, int lORA_SPREADING_FACTOR, int lORA_CODINGRATE, int lORA_PREAMBLE_LENGTH, int lORA_SYMBOL_TIMEOUT, bool lORA_FIX_LENGTH_PAYLOAD_ON, bool lORA_IQ_INVERSION_ON, int rX_SYM_TIMEOUT, int rX_FIXED_LENGTH_PACKETS, int rX_FIXED_PAYLOAD_LENGTH, int rX_CRC_ENABLED, int rX_INTRA_PACKET_FREQ_HOP_ENABLED, int rX_INTRA_PACKET_HOP_PERIOD, int rX_IQ_INVERSION, bool rX_RECEP_CONT, UInt32 rX_CS_MS)
        {
            RF_FREQUENCY = rF_FREQUENCY;
            RF_IQ_INVERSION = rF_IQ_INVERSION;
            TX_OUTPUT_POWER = tX_OUTPUT_POWER;
            LORA_BANDWIDTH = lORA_BANDWIDTH;
            LORA_SPREADING_FACTOR = lORA_SPREADING_FACTOR;
            LORA_CODINGRATE = lORA_CODINGRATE;
            LORA_PREAMBLE_LENGTH = lORA_PREAMBLE_LENGTH;
            LORA_SYMBOL_TIMEOUT = lORA_SYMBOL_TIMEOUT;
            LORA_FIX_LENGTH_PAYLOAD_ON = lORA_FIX_LENGTH_PAYLOAD_ON;
            LORA_IQ_INVERSION_ON = lORA_IQ_INVERSION_ON;
            RX_SYM_TIMEOUT = rX_SYM_TIMEOUT;
            RX_FIXED_LENGTH_PACKETS = rX_FIXED_LENGTH_PACKETS;
            RX_FIXED_PAYLOAD_LENGTH = rX_FIXED_PAYLOAD_LENGTH;
            RX_CRC_ENABLED = rX_CRC_ENABLED;
            RX_INTRA_PACKET_FREQ_HOP_ENABLED = rX_INTRA_PACKET_FREQ_HOP_ENABLED;
            RX_INTRA_PACKET_HOP_PERIOD = rX_INTRA_PACKET_HOP_PERIOD;
            RX_IQ_INVERSION = rX_IQ_INVERSION;
            RX_RECEP_CONT = rX_RECEP_CONT;
            RX_CS_MS = rX_CS_MS;
        }




    }
    
}
