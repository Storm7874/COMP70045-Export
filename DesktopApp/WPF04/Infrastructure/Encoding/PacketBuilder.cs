using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WPF04.Domain.Entities.Dictionaries;
using WPF04.Domain.Entities.Packet;

namespace WPF04.Infrastructure.Encoding
{
    /// <summary>
    /// Class to handle the encoding, decoding, and other packet assembly processes
    /// </summary>
    public class PacketBuilder
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PacketBuilder() { }

        /// <summary>
        /// Method to build a packet header byte from the given parameters
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="ack"></param>
        /// <param name="reb"></param>
        /// <returns></returns>
        public byte BuildPacketHeader(string packetType, bool ack, bool reb)
        {
            //Get packet type byte
            byte packetTypeByte = this._GetPacketTypeByte(packetType);

            //Assemble header byte
            byte header = 0;                //B:???RACMD//
            header |= (byte)(packetTypeByte & 0b00000111);
            if (ack)
            {
                header |= 0b00001000; // Set ACK bit
            }
            if (reb)
            {
                header |= 0b00010000; // Set REB bit
            }

            //Return assembled header byte
            return header;
        }

        /// <summary>
        /// Method to get the byte representation of the packet type
        /// </summary>
        /// <param name="packetType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private byte _GetPacketTypeByte(string packetType)
        {
            //Map packet type strings to their corresponding byte values
            return packetType switch
            {
                "DICT" => 0b00000001, //   01 | Dictionary-Encoded
                "EDICT" => 0b00000010, //  02 | Encrypted, Dictionary-Encoded
                "RAW" => 0b00000011, //    03 | Raw Hex
                "CMD" => 0b00000100, //    04 | Command (CURRENTLY UNUSED) TODO: Implement this logic
                _ => throw new ArgumentException("Invalid packet type") // Unknown/Corrupt
            };
        }

        /// <summary>
        /// Method to build a packed 20-bit dictID:wordID reference from a WordRef object
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public uint BuildWordRef(WordRef word)
        {
            //Validate input
            if (word is null)
            {
                throw new ArgumentNullException(nameof(word), "WordText WordID cannot be null");
            }

            //Generate dict WordID (4 bits)
            byte dictRef = (byte)(word.DictionaryID & 0b00001111);

            //Generate WordText WordID (16 bits)
            ushort wordRef = (ushort)(word.WordID & 0b1111111111111111);

            //Pack dictRef and wordRef into a single 20-bit unsigned integer
            uint packedRef = ((uint)dictRef << 16) | wordRef;

            //Mask additional bits to ensure 20-bit structure
            packedRef &= 0xFFFFF;


            //Return assembled 20-bit reference
            return packedRef;
        }

        /// <summary>
        /// Extracts the header from a raw message string
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <returns></returns>
        public PacketHeader? ExtractPacketHeader(string rawMessage)
        {
            //Validate input
            if (string.IsNullOrEmpty(rawMessage) || rawMessage.Length < 2)
            {
                return null;
            }

            //Get header from string (First 2 characters)
            string hexHeader = rawMessage.Substring(0, 2);

            //Convert hex header to byte
            byte headerByte = Convert.ToByte(hexHeader, 16);

            //Extract packet type, ack, and Rebroadcast flags from the header byte
            //Following the format        B:???RACMD
            int packetType = headerByte & 0b00000111;
            bool ack = (headerByte & 0b00001000) != 0;
            bool reb = (headerByte & 0b00010000) != 0;

            //Map packet type to packet type string
            string packetTypeString = packetType switch
            {
                0b00000001 => "DICT",
                0b00000010 => "EDICT",
                0b00000011 => "RAW",
                0b00000100 => "CMD",
                _ => "RAW" //Default, in case of corrupt packets
            };

            //Return new PacketHeader object
            return new PacketHeader(packetTypeString, ack, reb);


        }

        /// <summary>
        /// Method to extract a list of WordRef objects from a raw message payload
        /// </summary>
        /// <param name="rawPayload"></param>
        /// <returns></returns>
        public List<WordRef> ExtractWordRefs(string rawPayload)
        {
            //Placeholder list
            List<WordRef> wordRefs = new List<WordRef>();

            //Placeholder for serialised WordRef objects
            List<uint> packedWords = new();

            //Iterate through the message in chunks of 5 characters. Split the message into 5-character chunks, as each represents one 20-bit WordRef.
            for (int i = 0; i < rawPayload.Length; i += 5)
            {
                //Extract 5-character hex WordText from the raw payload - Use 5 character chunks or the remaining characters if less than 5
                string hexWord = rawPayload.Substring(i, Math.Min(5, rawPayload.Length - i));

                //Convert hex string to uint
                uint packedWord = Convert.ToUInt32(hexWord, 16);

                //Add to packed word list
                packedWords.Add(packedWord);
            }

            //Iterate through the packed words and convert them to WordRef objects
            foreach (var packedWord in packedWords)
            {
                //Extract WordRef from packed WordText & convert to 5-character hex string
                WordRef wordRef = this._DecodeSegment(packedWord.ToString("X5"));

                //Add to WordRef list
                wordRefs.Add(wordRef);
            }

            //Return list of WordRef objects
            return wordRefs;
        }

        /// <summary>
        /// Method to decode a single 5-character hex segment into a WordRef object
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private WordRef _DecodeSegment(string segment)
        {
            //Validate input
            if (segment.Length != 5)
            {
                throw new ArgumentException("Segment must be exactly 5 characters long.");
            }

            //Convert segment to uint
            uint packedWord = Convert.ToUInt32(segment, 16);

            //Extract dictID (4 bits) and WordID (16 bits)
            byte dictRef = (byte)((packedWord >> 16) & 0b00001111);
            ushort wordRef = (ushort)(packedWord & 0b1111111111111111);

            //Create WordRef object with wordRef
            WordRef tempWordRef = new WordRef(wordRef);

            //Set the dict ID
            tempWordRef.DictionaryID = dictRef;

            //Return WordRef object with Word ID and Dict ID, but NO WORD TEXT YET
            return tempWordRef;
        }
    }
}
