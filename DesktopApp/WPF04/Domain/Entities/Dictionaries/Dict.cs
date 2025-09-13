using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities.Dictionaries
{
    /// <summary>
    /// Represents a dictionary containing a list of word references and a reference ID.
    /// </summary>
    public class Dict
    {
        //List of WordRef objects in the dictionary
        public List<WordRef> Words;

        //Reference ID for the dictionary
        public int DictRef;

        /// <summary>
        /// Initializes a new instance of the Dict class with the specified list of word references and dictionary reference ID.
        /// </summary>
        /// <param name="words"></param>
        /// <param name="dictRef"></param>
        public Dict(List<WordRef> words, int dictRef)
        {
            this.Words = words;
            this.DictRef = dictRef;
        }

    }
}
