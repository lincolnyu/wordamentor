using System.IO;
using System.Linq;

namespace WordamentSolver
{
    /// <summary>
    ///  A helper class that eases the process of collecting words to trie
    /// </summary>
    /// <remarks>
    ///  Some comprehensive english word lists such as provided by
    ///   http://mieliestronk.com/wordlist.html
    ///  are pretty good that we don't need to do any effort-taking and fruitless form changing
    /// </remarks>
    public static class TrieHelper
    {
        #region Fields

        /// <summary>
        ///  The vowel colleciton used to test if a letter is a vowel or not (a consonant)
        /// </summary>
        private static readonly char[] Vowels = new[] {'A', 'E', 'I', 'O', 'U'};

        #endregion

        #region Methods

        /// <summary>
        ///  Grabs words from the stream and adds them to the word trie
        /// </summary>
        /// <param name="trie">The trie that the words to add to</param>
        /// <param name="reader">The stream to read words from</param>
        public static void Grab(this Trie trie, StreamReader reader)
        {
            var word = "";
            while (!reader.EndOfStream)
            {
                var ich = reader.Read();
                var ch = (char) ich;
                // TODO some words with 's at the end might be considered valid by Wordament
                if (char.IsLetter(ch))
                {
                    word += ch;
                }
                else if (word != "")
                {
                    trie.AddWord(word);
                    word = "";
                }
            }
        }

        /// <summary>
        ///  Adds a regular noun to the trie
        /// </summary>
        /// <param name="trie">The trie to add the noun to</param>
        /// <param name="word">The noun to add</param>
        public static void AddRegularNoun(this Trie trie, string word)
        {
            trie.AddWord(word);
            var plural = AddS(word);
            trie.AddWord(plural);
        }

        /// <summary>
        ///  Adds a regular verb to the trie
        /// </summary>
        /// <param name="trie">The trie to add the verb to</param>
        /// <param name="word">The verb to add</param>
        public static void AddRegularVerb(this Trie trie, string word)
        {
            trie.AddWord(word);
            var addS = AddS(word);
            trie.AddWord(addS);
            var addIng = AddIng(word);
            trie.AddWord(addIng);
            var addD = AddD(word);
            trie.AddWord(addD);
        }

        // TODO review/test this for any uncovered cases
        /// <summary>
        ///  Adds 's' to a word with regular change rules
        /// </summary>
        /// <param name="word">The word to add 's' to; assumed to have at least 2 characters</param>
        /// <returns>The word with 's' added</returns>
        public static string AddS(string word)
        {
            var lastCh = word[word.Length - 1];
            var secondLast = word[word.Length - 2];
            var addS = word;
            switch (lastCh)
            {
                case 'f':
                    if (secondLast == 'f')
                    {
                        addS += "s";
                    }
                    else
                    {
                        addS = addS.Substring(addS.Length - 1) + "ves";
                    }
                    break;
                case 's':
                case 'h':
                    addS += "es";
                    break;
                case 'y':
                    if (Vowels.Contains(secondLast))
                    {
                        addS = addS.Substring(addS.Length - 1) + "ies";
                    }
                    else
                    {
                        addS += "s";
                    }
                    break;
                default:
                    addS += "s";
                    break;
            }
            return addS;
        }

        // TODO review/test this for any uncovered cases
        /// <summary>
        ///  Adds 'ing' to a word using regular change rules
        /// </summary>
        /// <param name="word">The word to add 'ing' to; assumed to have at least 2 characters</param>
        /// <returns>The word with 'ing' added</returns>
        public static string AddIng(string word)
        {
            var lastCh = word[word.Length - 1];
            var secondLast = word[word.Length - 2];
            var addIng = word;

            if (!Vowels.Contains(lastCh))
            {
                if (!Vowels.Contains(secondLast))
                {
                    addIng += "ing";    // click -> clicking; stuff -> stuffing
                }
                else
                {
                    System.Diagnostics.Trace.Assert(word.Length > 2);
                    // coat, cut
                    var thirdLast = word[word.Length - 3];
                    if (Vowels.Contains(thirdLast))
                    {
                        addIng += "ing"; // coat -> coating
                    }
                    else
                    {
                        addIng += lastCh + "ing";   // cut -> cutting
                    }
                }
            }
            else if (lastCh == 'e')
            {
                if (!Vowels.Contains(secondLast))
                {
                    addIng = addIng.Substring(addIng.Length - 1) + "ing";   // crave -> craving
                }
                else
                {
                    addIng += "ing";    // shoe -> shoeing
                }
            }

            return addIng;
        }

        // TODO review/test this for any uncovered cases
        /// <summary>
        ///  Adds 'd' to a word using regular change rules
        /// </summary>
        /// <param name="word">The word to add 'd' to; assumed to have at least 2 characters</param>
        /// <returns>The word with 'd' added</returns>
        public static string AddD(string word)
        {
            var lastCh = word[word.Length - 1];
            var secondLast = word[word.Length - 2];
            var addD = word;

            if (!Vowels.Contains(lastCh))
            {
                if (!Vowels.Contains(secondLast))
                {
                    addD += "ed";    // click -> clicked; stuffed -> stuffed
                }
                else if (word.Length > 2)
                {
                    System.Diagnostics.Trace.Assert(word.Length > 2);
                    // coat, cut
                    var thirdLast = word[word.Length - 3];
                    if (Vowels.Contains(thirdLast))
                    {
                        addD += "ed"; // coat -> coating
                    }
                    else
                    {
                        addD += lastCh + "ed"; // transfer -> transferred
                    }
                }
            }
            else
            {
                addD += "d";    // continue -> continued
            }
            return addD;
        }

        #endregion
    }
}
