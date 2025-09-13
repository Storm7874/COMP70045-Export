using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF04.Domain.Entities;
using WPF04.Domain.Entities.Dictionaries;
using WPF04.Infrastructure.Crypto;
using WPF04.Infrastructure.Encoding;
using WPF04.Domain.Entities.Packet;
using System.Security.Cryptography;
using WPF04.Domain.Entities.Message;
namespace WPF04.Core.Services
{
    /// <summary>
    /// Central controller for message encoding, decoding, encryption, and decryption.
    /// </summary>
    public class MessageController
    {
        //Dictionary Controller
        private DictController _DictController;

        //Crypto Controller
        private CryptoController _CryptoController;

        //Packet builder
        private PacketBuilder _PacketBuilder = new PacketBuilder();

        //Status fields
        public bool DictLoadStatus;
        public bool CryptoLoadStatus;

        //Constructor
        public MessageController(string dictLocation, string padlocation)
        {
            //Supply configured dict locations to dict controller
            this._DictController = new DictController(dictLocation);

            //Supply configured pad locations to dict controller
            this._CryptoController = new CryptoController(padlocation);
        }

        /// <summary>
        /// Performs initial setup of the MessageController by initializing the DictController and CryptoController.
        /// </summary>
        public void PerformInitialSetup()
        {
            //Initialize controllers
            _DictController.Initialize();
            _CryptoController.Initialize();

            //Update load status fields
            this.DictLoadStatus = _DictController.DictLoadStatus;
            this.CryptoLoadStatus = _CryptoController.CryptoLoadStatus;
        }

        //TX Logic

        /// <summary>
        /// Generates a raw message string, consisting of packet header and the encoded message, based on the supplied parameters.
        /// </summary>
        /// <param name="rawMessageText"></param>
        /// <param name="encode"></param>
        /// <param name="encrypt"></param>
        /// <param name="ack"></param>
        /// <param name="rebroadcast"></param>
        /// <param name="TxID"></param>
        /// <returns></returns>
        public string? GenerateRawMessage(string rawMessageText, bool encode, bool encrypt, bool ack, bool rebroadcast, int TxID)
        {
            //Can only support encryption if DICT is enabled. Cannot encrypt RAW messages.
            string packetType = ""; //DICT/EDICT/RAW
            string packetHeader = "";
            string messageBody = "";
            int blockID = 0;

            //Determine packet type
            //If encode is true, set packet type to DICT or EDICT
            if (encode)
            {
                packetType = "DICT";

                //Check to see if successful dict loading has occurred
                if (!DictLoadStatus)
                {
                    MessageBox.Show("Dictionary not loaded. Cannot encode message.");
                    return string.Empty; //Return empty string if dict is not loaded
                }

                //If encryption param true, set packet type to EDICT
                if (encrypt)
                {
                    packetType = "EDICT";
                }
            }
            else
            {
                packetType = "RAW";
            }

            //Assemble packet header
            packetHeader = this._BuildPacketHeader(ack, rebroadcast, packetType);

            //Convert TxID to a hex string and append to header
            string TxIDHex = TxID.ToString("X4"); 

            //Need to encode the message if type DICT or EDICT
            if (packetType == "DICT" || packetType == "EDICT")
            {
                try
                {
                    //Generate the encoded message
                    messageBody = this._BuildEncodedMessage(rawMessageText);
                }
                catch(ArgumentException ex)
                {
                    MessageBox.Show($"Encoding failed. Word not found in dictionary: {ex.Message}", "Encoding Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return string.Empty; //Return empty string if encoding fails
                }
            }

            //Need to additionally encrypt message if EDICT
            if (packetType == "EDICT")
            {
                //Encrypt the message body
                messageBody = this._CryptoController.EncryptMessage(messageBody);
            }

            //If RAW message, convert the raw message text to a hex string
            if (packetType == "RAW")
            {
                //Convert raw message to text byte array using the system default encoding mechanism
                byte[] messageBytes = Encoding.Default.GetBytes(rawMessageText);

                //Convert byte array to hex string
                messageBody = Convert.ToHexString(messageBytes);
            }

            //Return the complete raw message string
            if(packetType == "EDICT")
            {
                //LoRa payload will consist of packet header, TxID, BlockID, and encrypted message body
                return (packetHeader + TxIDHex + messageBody);
            }

            return (packetHeader+ TxIDHex + messageBody);
        }

        /// <summary>
        /// Method to construct the packet header based on the input parameters
        /// </summary>
        /// <param name="ack"></param>
        /// <param name="rebroadcast"></param>
        /// <param name="packetType"></param>
        /// <returns></returns>
        private string _BuildPacketHeader(bool ack, bool rebroadcast, string packetType)
        {
            //Build the packet header byte
            byte header = _PacketBuilder.BuildPacketHeader(packetType, ack, rebroadcast);
            
            //Convert header byte to hex string & return
            string headerHex = header.ToString("X2");
            return headerHex;
        }

        /// <summary>
        /// Method to construct an encoded message string from the provided raw message text.
        /// </summary>
        /// <param name="messageBody"></param>
        /// <returns></returns>
        private string _BuildEncodedMessage(string messageBody)
        {
            //Need to take in the raw message string, with spaces + punctuation, and
            //convert it to a format that can be encoded into a list of WordRef objects.

            //Generate placeholders
            List<WordRef> wordRefs = new List<WordRef>();
            string encodedMessage = string.Empty;

            //Split the message body into words using any whitespace as delimiter
            string[] messageWords = messageBody.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            //Iterate through each word in the message
            foreach (string word in messageWords)
            {
                //Pass each WordText to dict controller to get WordRef object
                WordRef? wordRef = _DictController.GetWordReference(word);

                //If wordRef is not null, dictionary lookup success - add it to the list of wordRefs
                if (wordRef != null)
                {
                    wordRefs.Add(wordRef);
                }
                //Dictionary lookup cannot find word. Throw an error to indicate encoding failure
                else
                {
                    throw new ArgumentException(word);
                }
            }

            //Once a list of WordRef objects has been created, need to serialise into hex.
            foreach (WordRef wordRef in wordRefs)
            {
                //Use uint to hold the packed WordRef, as it fits the 20-bit value
                uint packedRef = _PacketBuilder.BuildWordRef(wordRef);

                //Convert packedRef to hex string and append to message body
                encodedMessage += packedRef.ToString("X5");
            }

            //Finally, return the encoded message string.
            return encodedMessage;
        }


        //RX Logic

        /// Extracts a Packet object from the raw message string
        /// </summary>
        /// <param name="rawMessageText"></param>
        /// <returns></returns>
        public Packet? ExtractPacket(string rawMessageText) //For example, 0113B9000BC0043C00603001F5005E3
        {
            //Return object
            Packet tempPacket = new Packet();
            //Header
            PacketHeader? tempPacketHeader;
            //WordRef list
            List<WordRef> wordRefs = new List<WordRef>();

            //Check if rawMessageText is empty or null
            if (string.IsNullOrEmpty(rawMessageText))
            {
                //Return null if this is the case, as no packet can be extracted
                return null;
            }

            //Get packet header
            tempPacketHeader = _PacketBuilder.ExtractPacketHeader(rawMessageText);
            if(tempPacketHeader != null)
            {
                //Assign decoded header to tempPacket object
                tempPacket.PacketHeader = tempPacketHeader;

                //If the raw message is too short to contain a valid packet, return null
                if(rawMessageText.Length < 6)
                {
                    return null;
                }

                //Append the raw message to the packet
                tempPacket.RawMessage = rawMessageText;

                //Remove the header and TxID from the raw message to get the payload
                string rawPayload = _StripAdditionalPadding(tempPacket);

                //Packet header decoded successfully, can proceed with decoding packet body
                if (tempPacketHeader.PacketType == "RAW")
                {
                    //Dump raw Hex message to RawMessage property for decoding
                    tempPacket.RawMessage = rawPayload;
                    return tempPacket;
                }

                //Message is an encoded one
                else if(tempPacketHeader.PacketType == "DICT")
                {
                    //Extract WordRefs from the raw message text
                    tempPacket.DictPayload = _PacketBuilder.ExtractWordRefs(rawPayload);

                    //Decoding finished - Return
                    return tempPacket;
                }

                //Message is an encrypted encoded one
                else if (tempPacketHeader.PacketType == "EDICT")
                {
                    //Additional BlockID extraction and decryption required
                    tempPacket.BlockID = _CryptoController.ExtractBlockIDFromString(rawPayload);

                    //Message body will need to be decrypted before extraction takes place - Append the decrypted raw message to the packet
                    tempPacket.RawMessage = rawPayload;
                    return tempPacket;
                }
            }

            //If packet header extraction failed, return null
            return tempPacket;
        }


        /// <summary>
        /// Strips any additional padding added to the message payload during reception.
        /// </summary>
        /// <param name="extractedPacket"></param>
        /// <returns></returns>
        private string _StripAdditionalPadding(Packet extractedPacket)
        {
            //Generate placeholder string
            string strippedPayload = string.Empty;

            //Strip the header (2 chars) + TxID (4 chars) from the rest of the packet.
            strippedPayload = extractedPacket.RawMessage.Substring(6);

            //if DICT or EDICT, need to extract the raw payload, minus any additional header fields
            // DICT has no additional fields, and therefore can be handled as-is
            // EDICT has an additional BlockID that needs to be removed.
            if (extractedPacket.PacketHeader.PacketType == "DICT")
            {
                //Check if the stripped payload length is a multiple of 5 (Each WordRef is 5 hex characters long)
                if ((strippedPayload.Length) % 5 != 0)
                {
                    //If not, remove the trailing 0's added at RX
                    strippedPayload = strippedPayload.Remove((strippedPayload.Length - 1), 1);
                }
            }
            else if (extractedPacket.PacketHeader.PacketType == "EDICT")
            {
                //Check if the stripped payload length is a multiple of 5
                //Take into consideration the additional BlockID field (4 chars)
                if ((strippedPayload.Length - 4) % 5 != 0)
                {
                    //If not, remove the trailing 0's added at RX
                    strippedPayload = strippedPayload.Remove((strippedPayload.Length - 1), 1);
                }
            }
            //Return the verified payload
            return strippedPayload;
        }

        /// <summary>
        /// Extracts the source address from the provided raw message string.
        /// </summary>
        /// <param name="messagePayload"></param>
        /// <returns></returns>
        private int _ExtractSourceAddress(string messagePayload)
        {
            int sourceAddress = 0;
            //Ensure messagePayload is not empty
            if (!string.IsNullOrEmpty(messagePayload))
            {
                //Remove header
                string rawPayload = messagePayload.Substring(2);

                //Check if rawPayload is long enough to contain a source address
                if(rawPayload.Length >= 4)
                {
                    //Extract the first 4 characters of the raw payload
                    string sourceAddressHex = rawPayload.Substring(0,4);

                    //Convert hex string to integer
                    sourceAddress = Convert.ToInt32(sourceAddressHex, 16);
                }
            }

            //Return 0 if extraction fails
            return sourceAddress;
        }

        /// <summary>
        /// Converts a hex-encoded string to a regular string.
        /// </summary>
        /// <param name="rawMessageText"></param>
        /// <returns></returns>
        private string _HexToString(string rawMessageText)
        {
            //Convert hex to byte array (1byte = 2 hex characters)
            byte[] raw = new byte[rawMessageText.Length / 2];

            //Iterate through the hex string and convert each pair of characters to a byte
            for (int hexByteIndex = 0; hexByteIndex < raw.Length; hexByteIndex++)
            {
                //For each character pair, convert to byte and append to byte array
                raw[hexByteIndex] = Convert.ToByte(rawMessageText.Substring(hexByteIndex * 2, 2), 16);
            }

            //Convert byte array to string
            string result = Encoding.Default.GetString(raw);

            //Return string
            return result;
        }

        /// <summary>
        /// Converts a list of WordRef objects to a string.
        /// </summary>
        /// <param name="wordRefs"></param>
        /// <returns></returns>
        private string _ConvertWordRefsToString(List<WordRef> wordRefs)
        {
            //Generate placeholder string
            string result = string.Empty;

            //Iterate through each WordRef object, and perform local dictionary lookup
            foreach(WordRef wordRef in wordRefs)
            {
                //Append the WordText to the result string
                result += _DictController.GetWordTextByRef(wordRef.DictionaryID, wordRef.WordID);
                //space
                result += " ";
            }

            //Remove spaces from end of string
            return result.Trim();
        }



        //Main method to decode a received message
        /// <summary>
        /// Primary method to decode a received message, returning a DisplayMessage object for the UI
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <returns></returns>
        public DisplayMessage DecodeMessage(RxMessage receivedMessage)
        {           
            //Extract the LoRa packet from the RxMessage object
            Packet? newPacket = this.ExtractPacket(receivedMessage.messagePayload);

            //If the packet is valid (Not null)
            if(newPacket != null)
            {
                //Extract the source address from the received message
                int sourceAddress = this._ExtractSourceAddress(receivedMessage.messagePayload);

                //Return a new display message having converted the raw hex text to string.
                if (newPacket.PacketHeader.PacketType == "RAW")
                {
                    DisplayMessage tempDisplayMessage = new DisplayMessage(
                        newPacket.PacketHeader.PacketType,   //RAW
                        sourceAddress,                       //Source address (int)
                        _HexToString(newPacket.RawMessage),  //STRING
                        newPacket.RawMessage.Length,         //Length of raw hex string
                        receivedMessage.messageRssi,         //RSSI of the message
                        receivedMessage.messageSnr,          //SNR of the message
                        DateTime.UtcNow);                    //Time received

                    //Append ack and rebroadcast flags to the display message
                    if (newPacket.PacketHeader.Acknowledgement)
                    {
                        tempDisplayMessage.ack = true;
                    }

                    if (newPacket.PacketHeader.Rebroadcast)
                    {
                        tempDisplayMessage.rebroadcast = true;
                    }

                    //Return final object
                    return tempDisplayMessage;

                }

                //Encoded message return
                else if (newPacket.PacketHeader.PacketType == "DICT")
                {
                    DisplayMessage tempDisplayMessage = new DisplayMessage(
                        newPacket.PacketHeader.PacketType,                  //DICT/EDICT
                        sourceAddress,                                      //Source address (int)
                        _ConvertWordRefsToString(newPacket.DictPayload),    //STRING - Converted from list of WordRef objects
                        receivedMessage.messagePayload.Length,              // Length of raw hex string
                        receivedMessage.messageRssi,                        // RSSI of the message
                        receivedMessage.messageSnr,                         // SNR of the message
                        DateTime.UtcNow                                     // Time received
                        );

                    //Append ack and rebroadcast flags to the display message
                    if (newPacket.PacketHeader.Acknowledgement)
                    {
                        tempDisplayMessage.ack = true;
                    }

                    if (newPacket.PacketHeader.Rebroadcast)
                    {
                        tempDisplayMessage.rebroadcast = true;
                    }

                    //Return final object
                    return tempDisplayMessage;
                }

                //Encrypted Message Return
                else if(newPacket.PacketHeader.PacketType == "EDICT")
                {
                    //Extract the BlockID from the raw message
                    int ciphertextBlockID = _CryptoController.ExtractBlockIDFromString(newPacket.RawMessage);

                    //Decrypt the message, returning a raw encoded string
                    string rawPlaintext = _CryptoController.DecryptMessage((newPacket.RawMessage.Substring(4)), ciphertextBlockID);
                    
                    //Extract WordRefs from the raw plaintext
                    List<WordRef> decodedPlaintextList = _PacketBuilder.ExtractWordRefs(rawPlaintext);

                    DisplayMessage tempDisplayMessage = new DisplayMessage(
                        newPacket.PacketHeader.PacketType,
                        sourceAddress,
                        _ConvertWordRefsToString(decodedPlaintextList),
                        receivedMessage.messagePayload.Length,
                        receivedMessage.messageRssi,
                        receivedMessage.messageSnr,
                        DateTime.UtcNow
                        );

                    //Append ack and rebroadcast flags to the display message
                    if (newPacket.PacketHeader.Acknowledgement)
                    {
                        tempDisplayMessage.ack = true;
                    }

                    if (newPacket.PacketHeader.Rebroadcast)
                    {
                        tempDisplayMessage.rebroadcast = true;
                    }

                    return tempDisplayMessage;
                }


                //Catch unknown packet type
                else
                {
                    //Return the raw message, with "Unknown" type
                    return new DisplayMessage("Unknown", 0, receivedMessage.messagePayload, 0, receivedMessage.messageRssi, receivedMessage.messageSnr, DateTime.UtcNow);
                }
            }
            else
            {
                //Return the raw messgae, with "Corrupted" type
                return new DisplayMessage("Corrupted",0, receivedMessage.messagePayload, 0, receivedMessage.messageRssi, receivedMessage.messageSnr, DateTime.UtcNow);
            }
        }
    }
}
