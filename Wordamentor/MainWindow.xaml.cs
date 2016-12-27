using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WordamentSolver;

namespace Wordamentor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private const string AppTitle = "WORDAMENTOR";

        /// <summary>
        ///  The word trie to use to solve the puzzles
        /// </summary>
        private Trie _trie;

        /// <summary>
        ///  A flag indicates if the current puzzle has been solved
        /// </summary>
        private bool _solved;

        /// <summary>
        ///  The list of touch-slide sequences
        /// </summary>
        private List<Solver.Sequence> _solutions;

        /// <summary>
        ///  The index of the solution to show currently
        /// </summary>
        private int _indexOfCurrent;

        /// <summary>
        ///  If it's displaying solutions
        /// </summary>
        private bool _isPlaying;

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates and initializes a window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeTrie();
        }

        #endregion

        #region Methods

        #region Event handlers

        private void MainWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            SetTitle();
            //MainGrid.Focus();
            //FastInput.Focus();
        }

        /// <summary>
        ///  Handles preview-key-down event from the grid
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="e">The key down event argumnts</param>
        private void MainGridOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEnter = e.Key == Key.Enter;
            var shiftIsDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var startKey = isEnter && !shiftIsDown || e.Key == Key.Space || e.Key == Key.PageDown;
            var pageUp = e.Key == Key.PageUp || isEnter && shiftIsDown;
            var resetKey = e.Key == Key.Escape;
            var homeKey = e.Key == Key.Home;
            var clearKey = e.Key == Key.Delete;
            if (startKey)
            {
                SetIsPlaying(true); // set this before calling ShowSolution()
                if (!_solved)
                {
                    Solve();
                }
                else
                {
                    Next();
                }
                e.Handled = true;
            }
            else if (pageUp && _solved)
            {
                SetIsPlaying(true);
                Prev();
                e.Handled = true;
            }
            else if (homeKey && _solved)
            {
                SetIsPlaying(true);
                First();
                e.Handled = true;
            }
            else if (clearKey)
            {
                Clear();
                _solved = false;
                e.Handled = true;
                SetIsPlaying(false);
            }
            else if (resetKey)
            {
                _solved = false;
                e.Handled = true;
                SetIsPlaying(false);
            }
        }

        private void SetIsPlaying(bool isPlaying)
        {
            _isPlaying = isPlaying;
            if (isPlaying)
            {
                MainGrid.Focus();
            }
            else
            {
                FastInput.Focus();
            }
        }

        private void FastInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPlaying)
            {
                return;
            }
            var children = MainGrid.Children;
            var i = 0;
            foreach (var c in FastInput.Text)
            {
                if (i >= 16)
                {
                    break;
                }
                ((TextBox)children[i++]).Text = c.ToString();
            }
            for (; i < 16; i++)
            {
                ((TextBox)children[i]).Text = "";
            }
        }

        private void FastInputMouseEnter(object sender, MouseEventArgs e)
        {
            _isPlaying = false;
        }

        /// <summary>
        ///  Handles size-changed event from the grid
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="e">The key down event argumnts</param>
        private void MainGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ShowSolution();
        }

        #endregion

        /// <summary>
        ///  Sets app title as per app name and version
        /// </summary>
        /// <remarks>
        ///  References:
        ///  1. http://stackoverflow.com/questions/22527830/how-to-get-the-publish-version-of-a-wpf-application
        /// </remarks>
        private void SetTitle()
        {
            try
            {
                var ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.
                    CurrentVersion;
                Title = string.Format("{0} (Ver {1}.{2})", AppTitle, ver.Major, ver.Minor);
            }
            catch (System.Deployment.Application.InvalidDeploymentException)
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                Title = string.Format("{0} (Asm Ver {1}.{2})", AppTitle, ver.Major, ver.Minor);
            }
        }

        /// <summary>
        ///  Initializes the trie
        /// </summary>
        /// <remarks>
        ///  References:
        ///  1. Accessing embedded resourcs: https://support.microsoft.com/en-us/kb/319292
        /// </remarks>
        private void InitializeTrie()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("Wordamentor.corncob_lowercase.txt");

            if (stream == null)
            {
                MessageBox.Show(this, "Dictionary Missing", AppTitle);
                return;
            }

            _trie = new Trie();
            using (var reader = new StreamReader(stream))
            {
                _trie.Grab(reader);
            }
        }

        /// <summary>
        ///  Solves the puzzle (non-reentrant)
        /// </summary>
        private void Solve()
        {
            var solver = new Solver(_trie);
            var grid = new string[4,4];
            var children = MainGrid.Children;

            var values = new int[4, 4];

            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    var s = grid[i, j] = ((TextBox) children[i*4 + j]).Text;
                    if (string.IsNullOrEmpty(s))
                    {
                        // empty cell
                        return;
                    }
                    values[i, j] = s.Length > 1 ? 8 : 1; // default award is 1 for normal cell and 8 for special
                }
            }

            _solutions = solver.Solve(grid, values);
            for (var i = _solutions.Count - 1; i >= 0; i-- )
            {
                var sol = _solutions[i];
                if (sol.Word.Length < 3)
                {
                    _solutions.RemoveAt(i);
                }
            }

            First();

            _solved = true;
        }

        /// <summary>
        ///  Shows the first solution
        /// </summary>
        private void First()
        {
            _indexOfCurrent = 0;

            if (_indexOfCurrent < _solutions.Count)
            {
                ShowSolution();
            }
            else
            {
                MessageBox.Show(this, "No solution", AppTitle);
            }
        }

        /// <summary>
        ///  Shows the next solution
        /// </summary>
        private void Next()
        {
            if (_solutions.Count == 0)
            {
                MessageBox.Show(this, "No solution", AppTitle);
            }
            else if (_indexOfCurrent < _solutions.Count - 1)
            {
                _indexOfCurrent++;
                ShowSolution();
            }
            else
            {
                MessageBox.Show(this, "Last solution", AppTitle);
            }
        }

        /// <summary>
        ///  Shows the previous solution
        /// </summary>
        private void Prev()
        {
            if (_solutions.Count == 0)
            {
                MessageBox.Show(this, "No solution", AppTitle);
            }
            else if (_indexOfCurrent > 0)
            {
                _indexOfCurrent--;
                ShowSolution();
            }
            else
            {
                MessageBox.Show(this, "First solution", AppTitle);
            }
        }

        /// <summary>
        ///  Displays the current solution
        /// </summary>
        private void ShowSolution()
        {
            SolCanvas.Children.Clear();

            if (_solutions == null || _indexOfCurrent >= _solutions.Count)
            {
                return;
            }

            var sol = _solutions[_indexOfCurrent];
            const int sideLength = 4;

            Trace.Assert(sol.Rows.Length > 0, "sol.Rows.Length > 0");
            Trace.Assert(sol.Cols.Length > 0, "sol.Cols.Length > 0");

            var row = sol.Rows[0];
            var col = sol.Cols[0];

            var lastX = MainGrid.ActualWidth*(col + 0.5)/sideLength;
            var lastY = MainGrid.ActualHeight*(row + 0.5)/sideLength;

            var r = Math.Min(MainGrid.ActualWidth, MainGrid.ActualHeight);
            r *= 0.1 / sideLength;   // radius of the circle that marks the first cell
            var d = r*2;
            var circle = new Ellipse
                             {
                                 Width = d,
                                 Height = d,
                                 Fill = new SolidColorBrush(Colors.Red)                                
                             };
            circle.SetValue(Canvas.LeftProperty, lastX-r);
            circle.SetValue(Canvas.TopProperty, lastY-r);

            SolCanvas.Children.Add(circle);
            
            for (var i = 1; i < sol.Rows.Length; i++)
            {
                row = sol.Rows[i];
                col = sol.Cols[i];

                var x = MainGrid.ActualWidth*(col + 0.5)/4;
                var y = MainGrid.ActualHeight*(row + 0.5)/4;

                var line = new Line
                    {
                        X1 = lastX,
                        X2 = x,
                        Y1 = lastY,
                        Y2 = y,
                        Stroke = new SolidColorBrush(Colors.Red)
                    };
                
                SolCanvas.Children.Add(line);

                lastX = x;
                lastY = y;
            }

            if (_isPlaying)
            {
                FastInput.Text = sol.Word;
            }
        }

        /// <summary>
        ///  Clears the screen for new input
        /// </summary>
        private void Clear()
        {
            SolCanvas.Children.Clear();

            FastInput.Text = "";

            var children = MainGrid.Children;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ((TextBox) children[i*4 + j]).Text = "";
                }
            }
        }

        #endregion
    }
}
