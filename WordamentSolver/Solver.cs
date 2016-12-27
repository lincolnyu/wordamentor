using System;
using System.Collections.Generic;

namespace WordamentSolver
{
    /// <summary>
    ///  A Wordament solver
    /// </summary>
    public class Solver
    {
        #region Enumerations

        /// <summary>
        ///  Types of a card that indicate where the substring has to be placed
        /// </summary>
        private enum CardTypes
        {
            Normal, // the string can be present anywhere
            Head,   // the string has to be a leading string
            Tail,   // the string has to be a tailing string
        };

        #endregion
        
        #region Nested types

        /// <summary>
        ///  A tentative or validated sequence of cells that can form a valid spelling
        /// </summary>
        public class Sequence : IComparable<Sequence>
        {
            #region Fields

            /// <summary>
            ///  The sequence of row indices of the picked cells, must have the same lengh as Cols
            /// </summary>
            public int[] Rows;

            /// <summary>
            ///  The sequence of col indices of the picked cells, must have the same lengh as Rows
            /// </summary>
            public int[] Cols;

            /// <summary>
            ///  The total value of the sequence if available or a value of no use
            /// </summary>
            public int TotalValue;

            /// <summary>
            ///  The word this touch-link sequence forms
            /// </summary>
            public string Word;

            #endregion

            #region Methods

            #region IComparable<Sequence>

            /// <summary>
            ///  Compares the current object with the specified one
            /// </summary>
            /// <param name="other">The object the current one to compare to</param>
            /// <returns>The comparison result indicator</returns>
            public int CompareTo(Sequence other)
            {
                return String.Compare(Word, other.Word, StringComparison.Ordinal);
            }

            #endregion

            /// <summary>
            ///  Compares two sequences by value
            /// </summary>
            /// <param name="other">The other sequqence to compare to</param>
            /// <returns>The comparison result indicator</returns>
            public int CompareByValue(Sequence other)
            {
                var comp = TotalValue.CompareTo(other.TotalValue);
                return comp != 0 ? comp : Rows.Length.CompareTo(other.Rows.Length);
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        ///   word trie to use to solve Wordament
        /// </summary>
        public Trie Trie { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a solver with a word trie
        /// </summary>
        /// <param name="trie">The word trie to use to solve Wordament</param>
        public Solver(Trie trie)
        {
            Trie = trie;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Solves a Wordament grid
        /// </summary>
        /// <param name="grid">The grid to solve</param>
        /// <param name="values">The values of the grid cells</param>
        /// <returns>All the valid sequences sorted by value</returns>
        public List<Sequence> Solve(string[,] grid, int[,] values = null)
        {
            var maxRow = grid.GetUpperBound(0);
            var maxCol = grid.GetUpperBound(1);
            var used = new bool[maxRow + 1,maxCol + 1];
            var node = Trie.Root;
            var rowseq = new List<int>();
            var colseq = new List<int>();
            var result = new List<Sequence>();

            for (var row = 0; row <= maxRow; row++)
            {
                for (var col = 0; col <= maxCol; col++)
                {
                    var card = grid[row, col];
                    // tests if it's a multi card
                    var multi = card.Split('/');
                    var saved = grid[row, col];
                    foreach (var subcard in multi)
                    {
                        grid[row, col] = subcard;
                        Step(grid, values, row, col, used, node, rowseq, colseq, result);    
                    }
                    grid[row, col] = saved;
                }
            }

            result.Sort((x, y) => -x.CompareByValue(y));

            return result;
        }

        /// <summary>
        ///  Steps up one card
        /// </summary>
        /// <param name="grid">The grid</param>
        /// <param name="values">The values of the grid cells</param>
        /// <param name="row">The row of the cell to step into</param>
        /// <param name="col">The column of the cell to step into</param>
        /// <param name="used">The array that indicates if a certain cell has been used</param>
        /// <param name="node">The current node (before step up) in the trie</param>
        /// <param name="rowseq">The sequence of the row components of the cells used</param>
        /// <param name="colseq">The sequence of the column components of the cells used</param>
        /// <param name="list">The list that contains all the valid sequences found so far</param>
        private static void Step(string[,] grid, int[,] values, int row, int col, bool[,] used,
                                 Node node, List<int> rowseq, List<int> colseq, List<Sequence> list)
        {
            var card = grid[row, col];
            string s;
            var cardType = ProcessCard(card, out s);

            if (cardType == CardTypes.Head && rowseq.Count > 0)
            {
                return;
            }

            foreach (var ch in s)
            {
                node = node.Allows(ch);
                if (node == null)
                {
                    return;
                }
            }

            if (cardType == CardTypes.Tail && !node.AllowsStop())
            {
                return;
            }

            rowseq.Add(row);
            colseq.Add(col);

            if (node.AllowsStop())
            {
                var totalValue = 0;
                var word = "";
                for (var i = 0; i < rowseq.Count; i++)
                {
                    var r = rowseq[i];
                    var c = colseq[i];
                    if (values != null)
                    {
                        totalValue += values[r, c];
                    }
                    ProcessCard(grid[r, c], out s);
                    word += s;
                }

                var seqobj = new Sequence
                    {
                        Rows = rowseq.ToArray(),
                        Cols = colseq.ToArray(),
                        TotalValue = totalValue,
                        Word = word
                    };
                var index = list.BinarySearch(seqobj);
                // NOTE duplicate won't be counted in Wordament
                if (index < 0)
                {
                    index = -index - 1;
                    list.Insert(index, seqobj);
                }
                else
                {
                    var existing = list[index];
                    if (seqobj.CompareByValue(existing) > 0)
                    {
                        list[index] = seqobj;
                    }
                }
            }
            else if (rowseq.Count == grid.Length)
            {
                rowseq.RemoveAt(rowseq.Count - 1);
                colseq.RemoveAt(colseq.Count - 1);
                return;
            }

            var maxRow = grid.GetUpperBound(0);
            var maxCol = grid.GetUpperBound(1);

            used[row, col] = true;

            for (var i = Math.Max(0, row - 1); i <= Math.Min(maxRow, row + 1); i++)
            {
                for (var j = Math.Max(0, col - 1); j <= Math.Min(maxCol, col + 1); j++)
                {
                    if (i == row && j == col) continue;
                    if (used[i, j]) continue;

                    var c = grid[i, j];
                    var multi = c.Split('/');
                    var saved = grid[i, j];
                    foreach (var subc in multi)
                    {
                        grid[i, j] = subc;
                        Step(grid, values, i, j, used, node, rowseq, colseq, list);
                        grid[i, j] = saved;
                    }
                }
            }

            used[row, col] = false;

            rowseq.RemoveAt(rowseq.Count - 1);
            colseq.RemoveAt(colseq.Count - 1);
        }


        /// <summary>
        ///  Gets from the card the substring to be in the word and where it will be
        /// </summary>
        /// <param name="card">The original string on the card</param>
        /// <param name="processed">The substring to be in the word</param>
        /// <returns>Whether it will be at the head or the tail or any place</returns>
        private static CardTypes ProcessCard(string card, out string processed)
        {
            if (card[0] == '-')
            {
                processed = card.Substring(1);
                return CardTypes.Tail;
            }
            if (card[card.Length - 1] == '-')
            {
                processed = card.Substring(0, card.Length - 1);
                return CardTypes.Head;
            }
            
            processed = card;
            return CardTypes.Normal;
        }

        #endregion
    }
}
