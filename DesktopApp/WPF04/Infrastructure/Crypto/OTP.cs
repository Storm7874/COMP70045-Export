using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace WPF04.Infrastructure.Crypto
{
    /// <summary>
    /// Handler class for interacting with the OTP pad file
    /// </summary>
    public class OTP
    {
        //Metadata handler for the OTP
        public PadMetadata PadMetadata;

        //Paths to the binary and metadata files
        public string PadMetadataPath { get; set; }
        public string PadBinaryPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the OTP class with the specified binary and metadata file paths.
        /// </summary>
        /// <param name="binPath"></param>
        /// <param name="padMetadataJsonPath"></param>
        public OTP(string binPath, string padMetadataJsonPath)
        {
            PadBinaryPath = binPath;
            PadMetadataPath = padMetadataJsonPath;

            PadMetadata = PadMetadata.Load(padMetadataJsonPath);
        }

        /// <summary>
        /// Retrieves a specific block of data from the OTP file based on the provided block ID. Advances the pointer if the next clean block is retrieved.
        /// </summary>
        /// <param name="blockID"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public List<byte> GetBlockByID(int blockID)
        {
            //Create a placeholder list
            List<byte> blockData = new List<byte>();

            //Check to see whether the requested block ID is valid
            if (blockID > PadMetadata.BlockCount - 1 || blockID < 0)
            {
                throw new ArgumentOutOfRangeException("Block ID is out of range");
            }
            else
            {
                //Read the block data from the binary file
                using (FileStream fs = new FileStream(PadMetadata.PadFile, FileMode.Open, FileAccess.Read))
                {
                    //Calculate the offset based on the blockID, casting to long to prevent overflow
                    long offset = (long)blockID * PadMetadata.BlockSize;

                    //Check to see whether the requested offset exceeds pad limits
                    if (offset + PadMetadata.BlockSize <= fs.Length)
                    {
                        //Seek to the offset and read the block data
                        fs.Seek(offset, SeekOrigin.Begin);

                        //Create appropriate buffer
                        byte[] buffer = new byte[PadMetadata.BlockSize];

                        //Read
                        int bytesRead = fs.Read(buffer, 0, PadMetadata.BlockSize);

                        //Add the read bytes to the placeholder
                        blockData.AddRange(buffer.Take(bytesRead));
                    }

                    //Offset exceeds limits
                    else
                    {
                        throw new InvalidOperationException("Requested block ID exceeds OTP Pad bounds.");
                    }
                }
            }

            //Return the block data
            return blockData;
        }

        /// <summary>
        /// Instructs the metadata handler to advance the block pointer
        /// </summary>
        public void Advance()
        {
            PadMetadata.AdvanceBlock();
        }

        /// <summary>
        /// Retrieves the next clean block of data from the OTP file and advances the internal pointer.
        /// </summary>
        /// <returns></returns>
        public List<Byte> GetNextCleanBlock()
        {
            return GetBlockByID(PadMetadata.CurrentBlockID);
        }

        //Method to retrieve the 2-byte ID of the current clean block
        public int GetNextCleanBlockID()
        {
            return PadMetadata.CurrentBlockID;
        }
    }
}
