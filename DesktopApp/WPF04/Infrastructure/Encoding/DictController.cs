using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF04.Domain.Entities.Dictionaries;

namespace WPF04.Infrastructure.Encoding
{
    /// <summary>
    /// Controller class to manage loading and accessing dictionary system
    /// </summary>
    public class DictController
    {
        //Load status flag
        public bool DictLoadStatus = false;

        //Directory location of dictionary files
        private string DictLocations;

        //List of loaded dictionaries
        public List<Dict> Shelf = new List<Dict>();

        //Constructor
        public DictController(string dictLocations)
        {
            this.DictLocations = dictLocations;
        }

        /// <summary>
        /// Performs the initial configuration steps for this controller.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            LoadDictionaryFiles();
            DictLoadStatus = true;
            return true;
        }

        /// <summary>
        /// Loads all dictionary files from the specified directory and assembles them into Dict objects.
        /// </summary>
        public void LoadDictionaryFiles()
        {
            //Dict ref counter
            int dictReference = 0;

            //Create, and iterate through, list of files in dictionary directory
            string[] fileEntries = Directory.GetFiles(this.DictLocations);
            foreach  (string fileName in fileEntries)
            {
                //If file is valid, assemble dict object based on contents
                if (_ValidateFile(fileName))
                {
                    //Create dictionary file + iterate reference counter
                    AssembleDict(fileName, dictReference);
                    dictReference++;
                }

                //If file is invalid, display error message and set load flag to false
                else
                {
                    MessageBox.Show($"Invalid dictionary file: {fileName}");
                    DictLoadStatus = false;
                }
            }

            
        }

        /// <summary>
        /// Assembles a Dict object from a raw wordlist file and adds it to the Shelf.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="dictRef"></param>
        public void AssembleDict(string filepath, int dictRef)
        {
            //Temporary list of WordRef objects to append to Dict object
            List<WordRef> dictWords = new List<WordRef>();

            //WordText WordID counter
            int wordReference = 0;

            //Load all lines from file
            string[] wordList = File.ReadAllLines(filepath);

            //Iterate
            foreach(string word in wordList)
            {
                //Strip initial characters
                if (word != "[" && word != "]")
                {
                    //Clean WordText
                    var cleanWord = word.Replace("\"", "");
                    cleanWord = cleanWord.Trim(',');
                    cleanWord = cleanWord.TrimStart();
                    
                    //Assemble new WordRef object and add to list
                    dictWords.Add(new WordRef(wordReference, cleanWord));
                    
                    //Increment counter
                    wordReference++;
                }
            }

            //Create new dictionary object
            Dict tempDict = new Dict(dictWords, dictRef);

            //Add to dictionary shelf
            Shelf.Add(tempDict);
        }

        // TODO: Implement this logic
        /// <summary>
        /// Validates the structure of a dictionary file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool _ValidateFile(string filePath)
        {
            return true;
        }

        /// <summary>
        /// Retrieves a WordRef object that matches the provided input word from the loaded dictionaries.
        /// </summary>
        /// <param name="msgWord"></param>
        /// <returns></returns>
        public WordRef? GetWordReference(string msgWord)
        {
            //Lowercase-ify
            msgWord = msgWord.ToLower();

            //Iterate through all dictionaries
            foreach(Dict dictref in Shelf)
            {
                //Iterate through all WordRefs  
                foreach(WordRef wordRef in dictref.Words)
                {
                    //Lowercase-ify
                    string currentWord = wordRef.WordText.ToString().ToLower();

                    //If WordText matches, return WordRef with associated dictID
                    if (currentWord == msgWord)
                    {
                        wordRef.DictionaryID = dictref.DictRef;
                        return wordRef;
                    }
                }
            }

            //If no match found, return null
            return null;
        }

        /// <summary>
        /// Retrieves the associated Word from a dictionary ID and word ID, if it exists.
        /// </summary>
        /// <param name="DictID"></param>
        /// <param name="WordID"></param>
        /// <returns></returns>
        public string GetWordTextByRef(int DictID, int WordID)
        {
            //Iterate through all dictionaries
            foreach (Dict dict in Shelf)
            {
                //If dictionary ID matches
                if (dict.DictRef == DictID)
                {
                    //Iterate through all WordRefs in the dictionary
                    foreach (WordRef wordRef in dict.Words)
                    {
                        //If WordID matches, return WordText
                        if (wordRef.WordID == WordID)
                        {
                            return wordRef.WordText;
                        }
                    }
                }
            }
            //If no match found, return obvious placeholder
            return "???";
        }
    }
}
