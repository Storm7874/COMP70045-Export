using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities
{
    /// <summary>
    /// Configuration file class to hold application + transmitted message settings.
    /// </summary>
    public class ConfigurationFile
    {
        //Radio settings
        public string RadioPort { get; set; } // COM Port
        public int RadioBaud { get; set; } = 115200; // Default baud rate

        // Message settings
        public int TxID { get; set; }// Default TxID, integer in range 0-65,535 (0x0000-0xFFFF)
        public bool RespondToAck { get; set; } = true; // Whether the application responds to ack requests
        public bool RespondToRebroadcast { get; set; } = true; // Whether the application responds to rebroadcast requests

        // File locations
        public string DictionaryFileLocation { get; set; }
        public string PadFileLocation { get; set; }

        // Constructor
        public ConfigurationFile(string RadioPort, int RadioBaud, int TxID, bool RespondToAck, bool RespondToRebroadcast, string DictionaryFileLocation, string PadFileLocation)
        {
            this.RadioPort = RadioPort;
            this.RadioBaud = RadioBaud;
            this.TxID = TxID;
            this.RespondToAck = RespondToAck;
            this.RespondToRebroadcast = RespondToRebroadcast;
            this.DictionaryFileLocation = DictionaryFileLocation;
            this.PadFileLocation = PadFileLocation;
        }

        /// <summary>
        /// Method to verify if the configuration file contains valid settings.
        /// </summary>
        /// <returns></returns>
        public bool VerifyConfig()
        {
            // Check if RadioPort is not null or empty
            if (string.IsNullOrEmpty(RadioPort))
            {
                return false;
            }
            // Check if RadioBaud is a valid value (e.g., greater than 0)
            if (RadioBaud <= 0)
            {
                return false;
            }
            // Check if TxID is valid
            if (this.TxID < 0 || this.TxID > 65535)
            {
                //Generate a new, random TxID if the current one is invalid
                this.TxID = GenerateNewTxID();
            }
            // Check if DictionaryFileLocation and PadFileLocation are not null or empty
            if (string.IsNullOrEmpty(DictionaryFileLocation) || string.IsNullOrEmpty(PadFileLocation))
            {
                return false;
            }

            //Valid configuration
            return true;
        }

        /// <summary>
        /// Method to generate a new random TxID between 0 and 65535.
        /// </summary>
        /// <returns></returns>
        public int GenerateNewTxID()
        {
            //Generate random number between 0 and 65535
            Random random = new Random();
            int newTxID = random.Next(0, 65536);

            // Update the TxID property
            return newTxID;
        }
    }
}
