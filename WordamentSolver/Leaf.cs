namespace WordamentSolver
{
    /// <summary>
    ///  A class that represents a leaf in the trie
    /// </summary>
    public class Leaf : Node
    {
        #region Methods

        #region Node members

        /// <summary>
        ///  Returns if the node allows the specified character at the respective position in the word
        /// </summary>
        /// <param name="ch">The character to test</param>
        /// <returns>The next node if it allows or null</returns>
        public override Node Allows(char ch)
        {
            return null;
        }

        /// <summary>
        ///  Returns if the node allows the word to terminate at the respective position
        /// </summary>
        /// <returns>True if it allows</returns>
        public override bool AllowsStop()
        {
            return true;
        }

        #endregion

        #endregion
    }
}
