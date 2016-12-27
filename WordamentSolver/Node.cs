namespace WordamentSolver
{
    /// <summary>
    ///  The base class of all ndoes in the trie
    /// </summary>
    public abstract class Node
    {
        #region Methods

        /// <summary>
        ///  Returns the next node if the node allows the specified character at the 
        ///  respective position in the word
        /// </summary>
        /// <param name="ch">The character to test</param>
        /// <returns>The next node if it allows or null</returns>
        public abstract Node Allows(char ch);

        /// <summary>
        ///  Returns if the node allows the word to terminate at the respective position
        /// </summary>
        /// <returns>True if it allows stop</returns>
        public abstract bool AllowsStop();

        #endregion
    }
}
