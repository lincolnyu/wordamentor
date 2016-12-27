namespace WordamentSolver
{
    /// <summary>
    ///  A class that represents a non-leaf node in the trie
    /// </summary>
    public class NonleafNode : Node
    {
        #region Fields

        /// <summary>
        ///  All subnodes
        /// </summary>
        public readonly Node[] Subnodes = new Node[26];

        #endregion

        #region Properties

        /// <summary>
        ///  If a possible word terminates at this node
        /// </summary>
        public bool CanStop { get; set; }

        #endregion

        #region Methods

        #region Node memebers

        /// <summary>
        ///  Returns if the node allows the specified character at the respective position in the word
        /// </summary>
        /// <param name="ch">The character to test</param>
        /// <returns>The next node if it allows or null</returns>
        public override Node Allows(char ch)
        {
            ch = char.ToLower(ch);
            return Subnodes[ch-'a'];
        }

        /// <summary>
        ///  Returns if the node allows the word to terminate at the respective position
        /// </summary>
        /// <returns>True if it allows</returns>
        public override bool AllowsStop()
        {
            return CanStop;
        }

        #endregion

        #endregion
    }
}
