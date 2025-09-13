using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace WPF04.Infrastructure.Crypto
{
    /// <summary>
    /// Handler class for interacting with the OTP metadata file
    /// </summary>
    public class PadMetadata
    {
        //ID for the OTP instance
        public string? OTPID { get; set; } = "";

        //Size of each OTP Block, needs to be higher than the total max packet size (256 bytes)
        public int BlockSize { get; set; } = 256;

        //Total number of OTP blocks - Dictates how many encrypted messages can be sent with this pad
        public int BlockCount { get; set; } = 65536;

        //Pointer to the next clean block - Sent in the ciphertext header to prevent desync
        public int CurrentBlockID { get; set; } = 0;

        //Address of the raw .bin file
        public string PadFile { get; set; } = "";

        //Address of the metadata .json file
        public string MetaFile { get; set; } = "";

        public PadMetadata()
        {

        }

        /// <summary>
        /// Loads the PadMetadata from the specified JSON file.
        /// </summary>
        /// <param name="PadMetadataFile"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static PadMetadata Load(string PadMetadataFile)
        {
            //Check if file exists
            if (!File.Exists(PadMetadataFile))
            {
                throw new FileNotFoundException("Metadata file not found", PadMetadataFile);
            }

            //Read all text from file and deserialize into PadMetadata object
            string rawJson = File.ReadAllText(PadMetadataFile);
            PadMetadata? tempMeta = JsonSerializer.Deserialize<PadMetadata>(rawJson);

            //Check if deserialization was successful
            if (tempMeta == null)
            {
                throw new InvalidOperationException("Failed to deserialize metadata file");
            }

            //Set the corresponding filepath to the newly assembled object
            else
            {
                tempMeta.MetaFile = PadMetadataFile;
                return tempMeta;
            }
        }

        /// <summary>
        /// Saves the current state of the PadMetadataFile to the MetaFile path, overwriting the existing file.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Save()
        {
            //Ensure that the MetaFile path is set
            if (string.IsNullOrEmpty(MetaFile))
            {
                throw new InvalidOperationException("Metafile path not set");
            }

            //Serialize the current object to JSON and write to the MetaFile path
            string updatedJson = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(MetaFile, updatedJson);
        }

        /// <summary>
        /// Advances the CurrentBlockID to the next block and saves the metadata file.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void AdvanceBlock()
        {
            //Check to see whether the pad has been exhausted
            if (CurrentBlockID < BlockCount - 1)
            {
                //Advance the block pointer and save the updated metadata
                CurrentBlockID++;
                Save();
            }

            //Pad exhausted
            else
            {
                throw new Exception("OTP Pad exhausted. No more clean blocks available.");
            }
        }

        /// <summary>
        /// Debug method to display the current metadata details in a message box.
        /// </summary>
        public void DisplayDetails()
        {
            StringBuilder details = new StringBuilder();
            details.AppendLine($"OTP ID: {OTPID}");
            details.AppendLine($"Block Size: {BlockSize} bytes");
            details.AppendLine($"Total Blocks: {BlockCount}");
            details.AppendLine($"Current Block ID: {CurrentBlockID}");
            details.AppendLine($"Pad File: {PadFile}");
            details.AppendLine($"Metadata File: {MetaFile}");
            MessageBox.Show(details.ToString(), "OTP Pad Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
