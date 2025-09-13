using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Dictionaries
{
    /// <summary>
    /// Represents a reference to a word in a dictionary, including its ID and text.
    /// </summary>
    public class WordRef
    {
        // ID of the word
        public int WordID { get; set; }

        // Text of the word
        public string WordText { get; set; }

        // ID of the dictionary the word belongs to
        public int DictionaryID { get; set; }

        /// <summary>
        /// Initializes a new instance of the WordRef class with the specified word ID and text. - For Message Encoding
        /// </summary>
        /// <param name="wordId"></param>
        /// <param name="wordText"></param>
        public WordRef(int wordId, string wordText)
        {
            this.WordID = wordId;
            this.WordText = wordText;
        }

        /// <summary>
        /// Initializes a new instance of the WordRef class with the specified word ID and no text - For Message Decoder
        /// </summary>
        /// <param name="wordId"></param>
        public WordRef(int wordId)
        {
            this.WordID = wordId;
            this.WordText = string.Empty;
        }
    }
}
