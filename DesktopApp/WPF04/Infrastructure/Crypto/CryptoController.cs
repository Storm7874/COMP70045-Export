using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF04.Domain.Entities.Dictionaries;

namespace WPF04.Infrastructure.Crypto
{
    /// <summary>
    /// Main controller class for handling OTP encryption and decryption operations.
    /// </summary>
    public class CryptoController
    {
        // TODO: Make these configuration parameters adjustable
        //Pad file directory and filenames
        private string _PadFileDir;
        private string _PadFileName = "OTP.otp";
        private string _PadMetadataName = "OTPMeta.json";

        //Flag to indicate successful loading of crypto components
        public bool CryptoLoadStatus = false;

        //Instance of the OTP handler
        public OTP Pad;

        /// <summary>
        /// Initializes a new instance of the CryptoController class with the specified OTP file directory.
        /// </summary>
        /// <param name="PadFileDirectory"></param>
        public CryptoController(string PadFileDirectory)
        {
            this._PadFileDir = PadFileDirectory;
        }

        /// <summary>
        /// Initializes the CryptoController by validating the OTP file directory and loading the OTP files.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            //Check if the provided directory is valid
            if (string.IsNullOrEmpty(_PadFileDir) || !System.IO.Directory.Exists(_PadFileDir))
            {
                //Invalid directory supplied
                MessageBox.Show("Invalid directory provided for OTP files.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CryptoLoadStatus = false;

                //Set flag to false
                return CryptoLoadStatus;
            }

            //Valid directory
            else
            {
                //Get absolute filepath for OTP components
                string binPath = System.IO.Path.Combine(_PadFileDir, _PadFileName);
                string metaPath = System.IO.Path.Combine(_PadFileDir, _PadMetadataName);
                
                //Check if the required files exist
                if (!System.IO.File.Exists(binPath) || !System.IO.File.Exists(metaPath))
                {
                    //One or more required files are missing
                    MessageBox.Show("Required OTP files are missing in the specified directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CryptoLoadStatus = false;

                    //Set flag to false
                    return CryptoLoadStatus;
                }

                //Directory + Files exist
                else
                {
                    //Initialize the OTP handler
                    Pad = new OTP(binPath, metaPath);

                    //Return success
                    CryptoLoadStatus = true;
                    return CryptoLoadStatus;
                }
            }
        }

        /// <summary>
        /// Returns the 2-byte reference of the next unused OTP block.
        /// </summary>
        /// <returns></returns>
        public int GetNextPadAddress()
        {
            return Pad.GetNextCleanBlockID();
        }

        /// <summary>
        /// Extracts a 20-bit segment from the provided OTP byte list based on the specified word index.
        /// </summary>
        /// <param name="otp"></param>
        /// <param name="wordIndex"></param>
        /// <returns></returns>
        private int _GetPadSegmentValue(List<byte> otp, int wordIndex)
        {
            //Where in the byte array the 20-bit word segment starts
            int startBit = wordIndex * 20;

            //Which byte does this particular segment start in
            int startByte = startBit / 8;

            //Where in the byte does this particular segment start
            int bitOffset = startBit % 8;


            //Pack 4-bytes of data into single uint, to ensure enough bits to extract the full 20-bit segment
            uint window = ((uint)otp[startByte] << 24) 
                | ((uint)otp[startByte + 1] << 16) 
                | ((uint)otp[startByte + 2] << 8) 
                | otp[startByte + 3];

            //bit-shift into lower 20-bits and mask off the rest
            int shift = 12 - bitOffset;

            //Return the 20-bit segment data
            return (int)((window >> shift) & 0xFFFFF);
        }

        /// <summary>
        /// Extracts the Block ID from the start of a raw message string.
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int ExtractBlockIDFromString(string rawMessage)
        {
            //Verify parameters
            if(string.IsNullOrEmpty(rawMessage) || rawMessage.Length < 4)
            {
                //Input string is null or too short
                throw new ArgumentNullException("Raw message is null or too short");
            }

            //Placeholder for BlockID
            int BlockID = 0;

            //Parse the BlockID from the start of the raw message (first 4 hex chars)
            BlockID = int.Parse(rawMessage.Substring(0, 4));

            //Return the BlockID
            return BlockID;
        }

        /// <summary>
        /// Decodes the encrypted, serialised list of WordRef objects, using the OTP block specified by blockID.
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="blockID"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string DecryptMessage(string ciphertext, int blockID)
        {
            //Retrieve the full pad block from the OTP handler
            List<byte> padBytes = Pad.GetBlockByID(blockID);
            
            //Perform validity checks
            if(string.IsNullOrEmpty(ciphertext) || ciphertext.Length < 4)
            {
                //Input string is null or too short
                throw new ArgumentNullException("Ciphertext is null or too short");
            }

            //Check blockID validity
            if (blockID < 0 || blockID > Pad.PadMetadata.BlockCount - 1)
            {
                //Block ID is out of range
                throw new ArgumentOutOfRangeException("Block ID is out of range");
            }

            //Check to see if ciphertext appears to be a valid list of serialised wordRefs, by checking divisibility by 5
            if (ciphertext.Length % 5 != 0)
            {
                //ciphertext = ciphertext.Remove(ciphertext.Length -1, 1);
                //throw new ArgumentException("Ciphertext length must be multiple of 5 hex chars");
            }

            //Placeholder for the plaintext
            var stringbuilder = new StringBuilder(ciphertext.Length);

            //Loop through each 5-character chunk of the ciphertext
            for (int i = 0; i < (ciphertext.Length / 5); i++)
            {
                //Extract one potential wordRef from the ciphertext
                string chunk = ciphertext.Substring(i * 5, 5);

                //Convert the chunk to an integer
                int cipherVal = Convert.ToInt32(chunk, 16);

                //Extract the corresponding 20-bit segment from the pad
                int padVal = _GetPadSegmentValue(padBytes, i);

                //XOR the cipherVal with the padVal, mask to 20 bits
                int msgVal = (cipherVal ^ padVal) & 0xFFFFF;

                //Add the msgVal to the plaintext string
                stringbuilder.Append(msgVal.ToString("X5"));
            }

            //Return the plaintext
            return stringbuilder.ToString();
        }

        /// <summary>
        /// Encrypts the provided serialised body using the OTP block specified by blockID.
        /// </summary>
        /// <param name="hexWordRefString"></param>
        /// <param name="blockID"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string EncryptMessage(string hexWordRefString)
        {
            //Get the next clean block ID from the OTP handler
            int blockID = Pad.GetNextCleanBlockID();

            //Retrieve the full, clean, unused pad block from the OTP handler
            List<byte> padBytes = Pad.GetBlockByID(blockID);

            //Advance the pad pointer to the next clean block
            //Pad.Advance();

            //Check validity of input parameters
            if (string.IsNullOrEmpty(hexWordRefString))
            {
                throw new ArgumentNullException("Message body is null");
            }
            if(blockID < 0 || blockID > Pad.PadMetadata.BlockCount - 1)
            {
                throw new ArgumentOutOfRangeException("Block ID is out of range");
            }
            if(blockID < Pad.PadMetadata.CurrentBlockID)
            {
                throw new ArgumentException("Block ID has already been used");
            }
            if(hexWordRefString.Length % 5 != 0)
            {
                throw new ArgumentException("Message length must be multiple of 5 hex chars");
            }

            //Calculate the max possible wordcount based on the pad size & compare against message text

            //Each byte provides 8 bits, each wordRef requires 20 bits
            int maxMessageWords = (padBytes.Count * 8) / 20;

            //Calculate the current wordcount of the message by dividing the string length by 5
            int currentMessageWordcount = hexWordRefString.Length / 5;

            //Check to see whether the message exceeds the maximum wordcount for the selected pad block
            if (currentMessageWordcount > maxMessageWords)
            {
                throw new ArgumentException("Message exceeds maximum word count for selected pad block");
            }


            //Placeholder for the ciphertext
            var stringBuilder = new StringBuilder(hexWordRefString.Length);

            //Loop through each 5-character chunk of the message
            for (int i = 0; i < currentMessageWordcount; i++)
            {
                //Extract 5-character hex chunk from message string (One wordRef)
                string chunk = hexWordRefString.Substring(i * 5, 5);

                //Convert the chunk to an integer
                int msgVal = Convert.ToInt32(chunk, 16);

                //Extract the corresponding 20-bit segment from the pad
                int padVal = _GetPadSegmentValue(padBytes, i);

                //XOR the message value with the pad value, mask to 5 bits
                int cipherVal = (msgVal ^ padVal) & 0xFFFFF;

                //Add the cipherval to the ciphertext string
                stringBuilder.Append(cipherVal.ToString("X5"));
            }

            //Append the encryption header (BlockID) to the ciphertext, ready for transmission + return
            return blockID.ToString("X4") + stringBuilder.ToString();
        }

        /// <summary>
        /// Generates a new OTP pad file of the specified size in MB and saves it to the provided filepath.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="filepath"></param>
        public void GenerateNewPad(int size, string filepath)
        {
            //Generate filename
            string fileName = System.IO.Path.Combine(_PadFileDir, "OTP.bin");

            //Convert from MB to bytes
            int rawSize = size * 1000000;

            //Create a new byte array of the specified size
            byte[] newPad = new byte[rawSize];

            //Fill the byte array
            RandomNumberGenerator.Fill(newPad);

            //Write the byte array to file
            if (!string.IsNullOrEmpty(filepath))
            {
                File.WriteAllBytes(fileName, newPad);
            }
        }

    }
}
