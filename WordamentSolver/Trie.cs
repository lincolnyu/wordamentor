namespace WordamentSolver
{
    /// <summary>
    ///  A alphabetical trie
    /// </summary>
    public class Trie
    {
        #region Properties

        /// <summary>
        ///  The root of the trie
        /// </summary>
        public NonleafNode Root { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a trie
        /// </summary>
        public Trie()
        {
            Root = new NonleafNode();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Adds a word to the trie
        /// </summary>
        /// <param name="word">The word to add</param>
        public void AddWord(string word)
        {
            // why lower? because it's easier and more comfortable to read common 
            // case-insensitive English words in lower case
            word = word.ToLower();
            NonleafNode lastNode = null;
            Node node = Root;
            var lastCh = '\0';
            foreach (var ch in word)
            {
                var nonleaf = node as NonleafNode;
                if (nonleaf != null)
                {
                    node = nonleaf.Subnodes[ch - 'a'];
                    lastNode = nonleaf;
                    lastCh = ch;
                }
                else
                {
                    var tmp = new NonleafNode {CanStop = node is Leaf};
                    System.Diagnostics.Trace.Assert(lastNode != null);
                    lastNode.Subnodes[lastCh - 'a'] = tmp;
                    lastNode = tmp;
                    node = lastNode.Subnodes[ch - 'a'];
                    lastCh = ch;
                }
            }
            var lastNonLeaf = node as NonleafNode;
            if (lastNonLeaf != null)
            {
                lastNonLeaf.CanStop = true;
            }
            else if (node == null)
            {
                System.Diagnostics.Trace.Assert(lastNode != null);
                lastNode.Subnodes[lastCh - 'a'] = new Leaf();
            }
        }

        #endregion
    }
}
