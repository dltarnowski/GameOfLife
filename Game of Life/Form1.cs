using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_of_Life
{
    public partial class Form1 : Form
    {
        // Total number of living cells
        int alive = 0;

        // The universe array
        bool[,] universe = new bool[30, 30];

        // The scratchpad array
        bool[,] scratchPad = new bool[30, 30];

        // Drawing colors
        Color gridColor;
        Color outerGridColor;
        Color cellColor;
        Color rGBBack;
        Color rGBCell;

        //Random used for RGB
        Random rnd = new Random();

        // The Timer class
        Timer timer = new Timer();

        // Used for seeding Random Universe
        int seed = new Random().Next();

        // Generation count
        int generations = 0;

        // View Menu bools
        bool isHudVisible;
        bool isGridVisible;
        bool isNeighborVisible;
        bool isFinite;
        bool isToroidal;
        bool isRGB;

        public Form1()
        {
            InitializeComponent();

            // Initialize variables with default settings
            graphicsPanel1.BackColor = Properties.Settings.Default.backColor;
            gridColor = Properties.Settings.Default.gridColor;
            outerGridColor = Properties.Settings.Default.outGridColor;
            isRGB = Properties.Settings.Default.RGB;
            cellColor = Properties.Settings.Default.cellColor;
            rGBCell = Properties.Settings.Default.cellColor;
            rGBBack = Properties.Settings.Default.backColor;
            timer.Interval = Properties.Settings.Default.timerInterval;
            universe = new bool [Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
            scratchPad = new bool[Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
            isToroidal = Properties.Settings.Default.isToroidal;
            isFinite = Properties.Settings.Default.isFinite;
            isNeighborVisible = Properties.Settings.Default.isNeighbor;
            isGridVisible = Properties.Settings.Default.isGrid;
            isHudVisible = Properties.Settings.Default.isHud;

            // Setup the timer
            timer.Tick += Timer_Tick;
            timer.Enabled = false;

            // Updates status strip Interval
            toolStripStatusLabelInterval.Text = "Interval: " + timer.Interval.ToString();
            toolStripStatusLabelSeed.Text = "Seed: " + seed.ToString();
        }

        #region Count Neighbors Finite
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;
            float xLen = universe.GetLength(0);
            float yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    if (yOffset == 0 && xOffset == 0) continue;
                    if (xCheck < 0) continue;
                    if (yCheck < 0) continue;
                    if (xCheck >= xLen) continue;
                    if (yCheck >= yLen) continue;

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }
        #endregion

        #region Count Neighbors Torodial
        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    if (yOffset == 0 && xOffset == 0) continue;
                    if (xCheck < 0) xCheck = xLen - 1;
                    if (yCheck < 0) yCheck = yLen - 1;
                    if (xCheck >= xLen) xCheck = 0;
                    if (yCheck >= yLen) yCheck = 0;

                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }
        #endregion

        #region Next Generation
        // Calculate the next generation of cells
        private void NextGeneration()
        {
            if (isRGB == true)
            {
                cellColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }

            scratchPad = new bool[universe.GetLength(0), universe.GetLength(1)];

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int count = 0;

                    if (isFinite)
                    {
                        count = CountNeighborsFinite(x, y);
                    }
                    if (isToroidal)
                    {
                        count = CountNeighborsToroidal(x, y);
                    }
                    // APPLY THE RULES
                    if (universe[x, y] == true && count < 2)
                    {
                        scratchPad[x, y] = false;
                    }
                    else if (universe[x, y] == true && count > 3)
                    {
                        scratchPad[x, y] = false;
                    }
                    else if (universe[x, y] == true && (count == 2 || count == 3))
                    {
                        scratchPad[x, y] = true;
                    }
                    else if (universe[x, y] == false && count == 3)
                    {
                        scratchPad[x, y] = true;
                    }
                    // TURN ON/OFF in ScratchPad
                }
            }
            bool[,] temp = universe;
            universe = scratchPad;
            scratchPad = temp;

            graphicsPanel1.Invalidate();

            AliveCells();

            // Increment generation count
            generations++;

            // Update status strip generations
            // NEEDS DATA FOR: Seed
            toolStripStatusLabelGenerations.Text = "Generations: " + generations.ToString();
        }
        #endregion

        #region Living Cell Status Update
        private void AliveCells()
        {
            alive = 0;
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y] == true)
                    {
                        alive++;
                    }
                    toolStripStatusLabelAlive.Text = "Alive: " + alive.ToString();
                }
            }
        }
        #endregion

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e) //CONVER TO FLOATS
        {
            // Enables RGB color when clicking if active
            if (isRGB == true)
            {
                cellColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }

            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth = graphicsPanel1.ClientSize.Width / (float)universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = graphicsPanel1.ClientSize.Height / (float)universe.GetLength(1);
            // Width and Height for x10 Grid
            float gridWidth = cellWidth * 10;
            float gridHeight = cellHeight * 10;

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);
            // A Pen for drawing the x10 grid lines
            Pen outerGridPen = new Pen(outerGridColor, 2);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);
            // A Brush for coloring the HUD
            SolidBrush hudBrush = new SolidBrush(Color.FromArgb(64, 255, 192, 203));

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = RectangleF.Empty; //RectangleF uses floats instead of ints for parameters
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // A larger rectangle to seperate universe by 10x10
                    RectangleF outerCellRect = RectangleF.Empty;
                    outerCellRect.X = x * gridWidth;
                    outerCellRect.Y = y * gridHeight;
                    outerCellRect.Width = gridWidth;
                    outerCellRect.Height = gridHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // if View > Grid is checked
                    if (isGridVisible)
                    {
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                        e.Graphics.DrawRectangle(outerGridPen, outerCellRect.X, outerCellRect.Y, outerCellRect.Width, outerCellRect.Height);
                    }


                    // Display number of neighbors
                    // Dynamically resize font size according to window size
                    float size = (float)(.4 * ((cellRect.Width + cellRect.Height) / 2)); 
                    Font font = new Font("Arial", size);
                    
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    if (isNeighborVisible)
                    {
                        int neighbors = 0;
                        RectangleF rect = new RectangleF(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                        if (isFinite)
                        {
                            neighbors = CountNeighborsFinite(x, y);
                        }
                        else if (isToroidal)
                        {
                            neighbors = CountNeighborsToroidal(x, y);
                        }
                        
                        if (neighbors == 0)
                        {
                            continue;
                        }

                        // Adjust Neighbor Count color based on whether the cell will die or live next generation
                        else if (neighbors > 3 || (universe[x, y] == false && neighbors < 3) || (universe[x, y] == true && neighbors < 2))
                        {
                            e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Red, rect, stringFormat);
                        }
                        else
                        {
                            e.Graphics.DrawString(neighbors.ToString(), font, Brushes.Green, rect, stringFormat);
                        }
                    }
                }

                if (isHudVisible)
                {
                    Font hudFont = new Font("Arial", 15f);
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Near;
                    stringFormat.LineAlignment = StringAlignment.Far;
                    string boundary = "";
                    if (isToroidal == true)
                    {
                        boundary = "Toroidal";
                    }
                    else if (isFinite == true)
                    {
                        boundary = "Finite";
                    }
                    // Draws the HUD onto the screen
                    e.Graphics.DrawString("Generations: " + generations.ToString() + "\nCell Count: " + alive.ToString() + "\nBoundary Type: " + boundary + "\nUniverse Size: {Width: " + universe.GetLength(0).ToString() + ", Height: " + universe.GetLength(1).ToString() + "}", hudFont, hudBrush, graphicsPanel1.ClientRectangle, stringFormat);
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            outerGridPen.Dispose();
            cellBrush.Dispose();
            hudBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                float cellWidth = graphicsPanel1.ClientSize.Width / (float)universe.GetLength(0);
                float cellHeight = graphicsPanel1.ClientSize.Height / (float)universe.GetLength(1);

                // Calculate the cell that was clicked in (SCALING DOWN)
                // CELL X = MOUSE X / CELL WIDTH
                int x = (int)(e.X / cellWidth);
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = (int)(e.Y / cellHeight);

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                // Without... will clip and only paint what's out of the window
                // NEVER INCLUDE INVALIDATE INSIDE PAINT
                graphicsPanel1.Invalidate();

                // Adds or subtracts to Alive count based on new click data
                if (universe[x, y] == true)
                {
                    alive++;
                    toolStripStatusLabelAlive.Text = "Alive: " + alive.ToString();
                }
                else if (universe[x, y] == false)
                {
                    alive--;
                    toolStripStatusLabelAlive.Text = "Alive: " + alive.ToString();
                }
            }
        }

        #region Save Document Button
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "txt";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!Game of Life Save");

                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    String currentRow = string.Empty;

                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        if (universe[x, y] == true)
                        {
                            currentRow += 'O';
                        }
                        else
                        {
                            currentRow += '.';
                        }
                    }
                    writer.WriteLine(currentRow);
                }
                writer.Close();
            }
        }
        #endregion

        #region Open Document Button
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        maxHeight++;
                    }

                    maxWidth = row.Length;
                }
                universe = new bool[maxHeight, maxWidth];
                scratchPad = new bool[maxHeight, maxWidth];

                int y = 0;
                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O')
                            {
                                universe[xPos, y] = true;
                            }
                            else if (row[xPos] == '.')
                            {
                                universe[xPos, y] = false;
                            }
                            else
                            {
                                universe[xPos, y] = false;
                            }
                        }
                        y++;
                    }
                }
                reader.Close();
            }
            AliveCells();
            graphicsPanel1.Invalidate();
        }
        #endregion

        #region New Document Button
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // resets generations and living cells
            generations = 0;
            alive = 0;

            // Stops the timer if running
            if (timer.Enabled == true)
            {
                timer.Enabled = false;
            }
            // iterates through the universe and turns every cell off
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                    scratchPad[x, y] = false;
                }
            }
            graphicsPanel1.Invalidate();

            // updates the status labels for both
            toolStripStatusLabelGenerations.Text = "Generations: " + generations.ToString();
            toolStripStatusLabelAlive.Text = "Alive: " + alive.ToString();
        }
        #endregion

        #region Pause Button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;

            toolStripButton1.Enabled = true;
            Pause.Enabled = false;
            Increment.Enabled = true;
        }
        #endregion

        #region Start Button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;

            toolStripButton1.Enabled = false;
            Pause.Enabled = true;
            Increment.Enabled = false;
        }
        #endregion

        #region Increment Button
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }
        #endregion

        #region Help Button
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpDialog dlg = new HelpDialog();
            dlg.ShowDialog();
        }
        #endregion

        #region FILE MENU
        #region File > Exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region File > New
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // resets generations and living cells
            generations = 0;
            alive = 0;

            // Stops the timer if running
            if (timer.Enabled == true)
            {
                timer.Enabled = false;
            }
            // iterates through the universe and turns every cell off
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                    scratchPad[x, y] = false;
                }
            }
            graphicsPanel1.Invalidate();

            // updates the status labels for both
            toolStripStatusLabelGenerations.Text = "Generations: " + generations.ToString();
            toolStripStatusLabelAlive.Text = "Alive: " + alive.ToString();
        }
        #endregion

        #region File > Open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        maxHeight++;
                    }

                    maxWidth = row.Length;
                }
                universe = new bool[maxHeight, maxWidth];
                scratchPad = new bool[maxHeight, maxWidth];

                int y = 0;
                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O')
                            {
                                universe[xPos, y] = true;
                            }
                            if (row[xPos] == '.')
                            {
                                universe[xPos, y] = false;
                            }
                        }
                        y++;
                    }
                }
                reader.Close();
            }
            AliveCells();
            graphicsPanel1.Invalidate();
        }
        #endregion

        #region File > Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "txt";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!Game of Life Save");

                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    String currentRow = string.Empty;

                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        if (universe[x, y] == true)
                        {
                            currentRow += 'O';
                        }
                        else
                        {
                            currentRow += '.';
                        }
                    }
                    writer.WriteLine(currentRow);
                }
                writer.Close();
            }
        }
        #endregion

        #region File > Import
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                int maxWidth = 0;
                int maxHeight = 0;

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        maxHeight++;
                    }

                    maxWidth = row.Length;
                }
                if (maxWidth > universe.GetLength(0) || maxHeight > universe.GetLength(1))
                {
                    universe = new bool[maxHeight, maxWidth];
                    scratchPad = new bool[maxHeight, maxWidth];
                }

                int y = 0;
                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();

                    if (row.StartsWith("!") == true)
                    {
                        continue;
                    }
                    else
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O')
                            {
                                universe[xPos, y] = true;
                            }
                            if (row[xPos] == '.')
                            {
                                universe[xPos, y] = false;
                            }
                        }
                        y++;
                    }
                }
                reader.Close();
            }
            AliveCells();
            graphicsPanel1.Invalidate();
        }
        #endregion
        #endregion

        #region RUN MENU
        #region Run > Start
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }
        #endregion

        #region Run > Pause
        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }
        #endregion

        #region Run > Increment
        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }
        #endregion

        #region Run > To
        private void toToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Stops the timer if it is running
            if(timer.Enabled == true)
            {
                timer.Enabled = false;
            }

            RunToDialog dlg = new RunToDialog();

            // The number displayed is the next generation.
            dlg.Number = generations + 1;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                while (generations < dlg.Number)
                {
                    NextGeneration();
                }
            }
        }
        #endregion
        #endregion

        #region VIEW MENU
        #region View > Toroidal
        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toroidalToolStripMenuItem.Checked == false)
            {
                toroidalToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem1.Checked = true;
                finiteToolStripMenuItem.Checked = false;
                finiteToolStripMenuItem1.Checked = false;
                isToroidal = true;
                isFinite = false;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region View > Finite
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (finiteToolStripMenuItem.Checked == false)
            {
                toroidalToolStripMenuItem.Checked = false;
                toroidalToolStripMenuItem1.Checked = false;
                finiteToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem1.Checked = true;
                isToroidal = false;
                isFinite = true;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region View > Neighbor Count
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(neighborCountToolStripMenuItem.Checked == false)
            {
                neighborCountToolStripMenuItem.Checked = true;
                neighborsCountToolStripMenuItem.Checked = true;
                isNeighborVisible = true;
                graphicsPanel1.Invalidate();
            }
            else if(neighborCountToolStripMenuItem.Checked == true)
            {
                neighborCountToolStripMenuItem.Checked = false;
                neighborsCountToolStripMenuItem.Checked = false;
                isNeighborVisible = false;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region View > Grid
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(gridToolStripMenuItem.Checked == false)
            {
                gridToolStripMenuItem.Checked = true;
                gridToolStripMenuItem2.Checked = true;
                isGridVisible = true;
                graphicsPanel1.Invalidate();
            }
            else if(gridToolStripMenuItem.Checked == true)
            {
                gridToolStripMenuItem.Checked = false;
                gridToolStripMenuItem2.Checked = false;
                isGridVisible = false;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region View > HUD
        private void hUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hUDToolStripMenuItem.Checked == false)
            {
                hUDToolStripMenuItem.Checked = true;
                viewHUDToolStripMenuItem.Checked = true;
                isHudVisible = true;
                graphicsPanel1.Invalidate();
            }
            else if(hUDToolStripMenuItem.Checked == true)
            {
                hUDToolStripMenuItem.Checked = false;
                viewHUDToolStripMenuItem.Checked = false;
                isHudVisible = false;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region View > RGB Mode
        private void rGBModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rGBModeToolStripMenuItem.Checked == false)
            {
                rGBModeToolStripMenuItem.Checked = true;
                isRGB = true;

                gridToolStripMenuItem.Checked = false;
                gridToolStripMenuItem2.Checked = false;
                isGridVisible = false;

                neighborCountToolStripMenuItem.Checked = false;
                neighborsCountToolStripMenuItem.Checked = false;
                isNeighborVisible = false;

                graphicsPanel1.BackColor = Color.Black;

                graphicsPanel1.Invalidate();
            }
            else if (rGBModeToolStripMenuItem.Checked == true)
            {
                rGBModeToolStripMenuItem.Checked = false;
                isRGB = false;

                gridToolStripMenuItem.Checked = true;
                gridToolStripMenuItem2.Checked = true;
                isGridVisible = true;

                neighborCountToolStripMenuItem.Checked = true;
                neighborsCountToolStripMenuItem.Checked = true;
                isNeighborVisible = true;

                graphicsPanel1.BackColor = rGBBack;
                cellColor = rGBCell;

                graphicsPanel1.Invalidate();
            }

        }
        #endregion

        #endregion

        #region RANDOMIZE MENU
        #region Random Universe
        private void InitialRandomUniverse()
        {
            Random r = new Random();

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int rInt = r.Next(0, 2);

                    if (rInt == 0)
                    {
                        universe[x, y] = !universe[x, y];
                    }
                }
            }
            graphicsPanel1.Invalidate();

            AliveCells();
        }
        #endregion

        #region Seeded Random Universe
        private void SeededRandomUniverse(int seed = 0)
        {
            Random r = new Random(seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int rInt = r.Next(0, 2);

                    if (rInt == 0)
                    {
                        universe[x, y] = !universe[x, y];
                    }
                }
            }
            graphicsPanel1.Invalidate();

            AliveCells();
        }
        #endregion

        #region Random From Time
        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitialRandomUniverse();
        }
        #endregion

        #region Random From Seed
        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Seed_Dialog dlg = new Seed_Dialog();

            dlg.Seed = seed;

            if(DialogResult.OK == dlg.ShowDialog())
            {
                seed = dlg.Seed;
                toolStripStatusLabelSeed.Text = "Seed: " + seed.ToString();
                SeededRandomUniverse(seed);
            }
        }
        #endregion

        #region Random From Current Seed
        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeededRandomUniverse(seed);
        }
        #endregion
        #endregion

        #region SETTINGS MENU
        #region Settings Options
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog dlg = new OptionsDialog();

            dlg.Interval = timer.Interval;
            dlg.Width = universe.GetLength(0);
            dlg.Height = universe.GetLength(1);

            if(DialogResult.OK == dlg.ShowDialog())
            {
                timer.Interval = dlg.Interval;
                if (dlg.Width != universe.GetLength(0) || dlg.Height != universe.GetLength(1))
                {
                    universe = new bool[dlg.Width, dlg.Height];
                }
                graphicsPanel1.Invalidate();
                // Updates status strip Interval
                toolStripStatusLabelInterval.Text = "Interval: " + timer.Interval.ToString();

            }    
        }
        #endregion

        #region Background Color Change
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = graphicsPanel1.BackColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;
                rGBBack = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Cell Color
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = cellColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;
                rGBCell = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Grid Color
        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = gridColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Grid x10 Color
        private void gridX10ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = gridColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                outerGridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Form Closed Settings
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.backColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.gridColor = gridColor;
            Properties.Settings.Default.outGridColor = outerGridColor;
            Properties.Settings.Default.cellColor = cellColor;
            Properties.Settings.Default.timerInterval = timer.Interval;
            Properties.Settings.Default.universeWidth = universe.GetLength(0);
            Properties.Settings.Default.universeHeight = universe.GetLength(1);
            Properties.Settings.Default.RGB = isRGB;
            Properties.Settings.Default.isToroidal = isToroidal;
            Properties.Settings.Default.isFinite = isFinite;
            Properties.Settings.Default.isNeighbor = isNeighborVisible;
            Properties.Settings.Default.isGrid = isGridVisible;
            Properties.Settings.Default.isHud = isHudVisible;

            Properties.Settings.Default.Save();
        }
        #endregion

        #region Reset
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetDialog dlg = new ResetDialog();

            if (DialogResult.OK == dlg.ShowDialog())
            {
                Properties.Settings.Default.Reset();

                graphicsPanel1.BackColor = Properties.Settings.Default.backColor;
                gridColor = Properties.Settings.Default.gridColor;
                outerGridColor = Properties.Settings.Default.outGridColor;
                cellColor = Properties.Settings.Default.cellColor;
                timer.Interval = Properties.Settings.Default.timerInterval;
                toolStripStatusLabelInterval.Text = "Interval: " + timer.Interval.ToString();
                universe = new bool[Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
                scratchPad = new bool[Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
                isRGB = Properties.Settings.Default.RGB;
                isToroidal = Properties.Settings.Default.isToroidal;
                isFinite = Properties.Settings.Default.isFinite;
                isNeighborVisible = Properties.Settings.Default.isNeighbor;
                isGridVisible = Properties.Settings.Default.isGrid;
                isHudVisible = Properties.Settings.Default.isHud;

                AliveCells();
                MenuCheck();
                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        #region Reload
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            graphicsPanel1.BackColor = Properties.Settings.Default.backColor;
            gridColor = Properties.Settings.Default.gridColor;
            outerGridColor = Properties.Settings.Default.outGridColor;
            cellColor = Properties.Settings.Default.cellColor;
            timer.Interval = Properties.Settings.Default.timerInterval;
            toolStripStatusLabelInterval.Text = "Interval: " + timer.Interval.ToString();
            universe = new bool[Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
            scratchPad = new bool[Properties.Settings.Default.universeWidth, Properties.Settings.Default.universeHeight];
            isRGB = Properties.Settings.Default.RGB;
            isToroidal = Properties.Settings.Default.isToroidal;
            isFinite = Properties.Settings.Default.isFinite;
            isNeighborVisible = Properties.Settings.Default.isNeighbor;
            isGridVisible = Properties.Settings.Default.isGrid;
            isHudVisible = Properties.Settings.Default.isHud;
            graphicsPanel1.Invalidate();
        }
        #endregion

        #endregion

        #region CONTEXT MENU
        private void viewHUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewHUDToolStripMenuItem.Checked == false)
            {
                viewHUDToolStripMenuItem.Checked = true;
                hUDToolStripMenuItem.Checked = true;
                isHudVisible = true;
                graphicsPanel1.Invalidate();
            }
            else
            {
                viewHUDToolStripMenuItem.Checked = false;
                hUDToolStripMenuItem.Checked = false;
                isHudVisible = false;
                graphicsPanel1.Invalidate();
            }
        }

        private void neighborsCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (neighborsCountToolStripMenuItem.Checked == false)
            {
                neighborsCountToolStripMenuItem.Checked = true;
                neighborCountToolStripMenuItem.Checked = true;
                isNeighborVisible = true;
                graphicsPanel1.Invalidate();
            }
            else
            {
                neighborsCountToolStripMenuItem.Checked = false;
                neighborCountToolStripMenuItem.Checked = false;
                isNeighborVisible = false;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (gridToolStripMenuItem2.Checked == false)
            {
                gridToolStripMenuItem2.Checked = true;
                gridToolStripMenuItem.Checked = true;
                isGridVisible = true;
                graphicsPanel1.Invalidate();
            }
            else if(gridToolStripMenuItem2.Checked == true)
            {
                gridToolStripMenuItem2.Checked = false;
                gridToolStripMenuItem.Checked = false;
                isGridVisible = false;
                graphicsPanel1.Invalidate();
            }
        }

        private void toroidalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (toroidalToolStripMenuItem1.Checked == false)
            {
                toroidalToolStripMenuItem1.Checked = true;
                toroidalToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem1.Checked = false;
                finiteToolStripMenuItem.Checked = false;

                isToroidal = true;
                isFinite = false;
                graphicsPanel1.Invalidate();
            }
        }

        private void finiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (finiteToolStripMenuItem.Checked == false)
            {
                toroidalToolStripMenuItem1.Checked = false;
                toroidalToolStripMenuItem.Checked = false;
                finiteToolStripMenuItem1.Checked = true;
                finiteToolStripMenuItem.Checked = true;
                isToroidal = false;
                isFinite = true;
                graphicsPanel1.Invalidate();
            }
        }

        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = graphicsPanel1.BackColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void cellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = cellColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;
                rGBCell = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = gridColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void gridX10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // New instance of color dialog
            ColorDialog dlg = new ColorDialog();

            // Sets dialog box color to current background color
            dlg.Color = gridColor;

            // Shows modal color dialog and if OK, recolors background
            if (DialogResult.OK == dlg.ShowDialog())
            {
                outerGridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        private void rGBModeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (rGBModeToolStripMenuItem1.Checked == false)
            {
                rGBModeToolStripMenuItem1.Checked = true;
                rGBModeToolStripMenuItem.Checked = true;
                isRGB = true;

                gridToolStripMenuItem.Checked = false;
                gridToolStripMenuItem2.Checked = false;
                isGridVisible = false;

                neighborCountToolStripMenuItem.Checked = false;
                neighborsCountToolStripMenuItem.Checked = false;
                isNeighborVisible = false;

                graphicsPanel1.BackColor = Color.Black;

                graphicsPanel1.Invalidate();
            }
            else if (rGBModeToolStripMenuItem1.Checked == true)
            {
                rGBModeToolStripMenuItem1.Checked = false;
                rGBModeToolStripMenuItem.Checked = false;
                isRGB = false;

                gridToolStripMenuItem.Checked = true;
                gridToolStripMenuItem2.Checked = true;
                isGridVisible = true;

                neighborCountToolStripMenuItem.Checked = true;
                neighborsCountToolStripMenuItem.Checked = true;
                isNeighborVisible = true;

                graphicsPanel1.BackColor = rGBBack;
                cellColor = rGBCell;

                graphicsPanel1.Invalidate();
            }
        }
        #endregion

        // Checks to see if there are any live cells in the universe and checks with user before closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int count = 0;

            ClosingDialog dlg = new ClosingDialog();

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for(int x = 0; x < universe.GetLength(0); x++)
                {
                    if(universe[x,y] != false)
                    {
                        count++;
                    }
                }
            }

            if(count > 0)
            {
                if (DialogResult.OK == dlg.ShowDialog())
                {
                    SaveFileDialog saveDlg = new SaveFileDialog();
                    saveDlg.Filter = "All Files|*.*|Cells|*.cells";
                    saveDlg.FilterIndex = 2; saveDlg.DefaultExt = "txt";

                    if (DialogResult.OK == saveDlg.ShowDialog())
                    {
                        StreamWriter writer = new StreamWriter(saveDlg.FileName);

                        // Write any comments you want to include first.
                        // Prefix all comment strings with an exclamation point.
                        // Use WriteLine to write the strings to the file. 
                        // It appends a CRLF for you.
                        writer.WriteLine("!Game of Life Save");

                        for (int y = 0; y < universe.GetLength(1); y++)
                        {
                            String currentRow = string.Empty;

                            for (int x = 0; x < universe.GetLength(0); x++)
                            {
                                if (universe[x, y] == true)
                                {
                                    currentRow += 'O';
                                }
                                else
                                {
                                    currentRow += '.';
                                }
                            }
                            writer.WriteLine(currentRow);
                        }
                        writer.Close();
                    }
                }

                if(dlg.DialogResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MenuCheck();
        }

        // Checks the status of the checked menu options and updates according to previous saved settings
        private void MenuCheck()
        {
            // Checks to see previously saved Grid View setting and updates the menu checkmark accordingly
            if (isGridVisible == true)
            {
                gridToolStripMenuItem.Checked = true;
                gridToolStripMenuItem2.Checked = true;
            }
            else if (isGridVisible == false)
            {
                gridToolStripMenuItem.Checked = false;
                gridToolStripMenuItem2.Checked = false;
            }

            // Checks to see previously saved Neighbor View setting and updates the menu checkmark accordingly
            if (isNeighborVisible == true)
            {
                neighborCountToolStripMenuItem.Checked = true;
                neighborsCountToolStripMenuItem.Checked = true;
            }
            else if (isNeighborVisible == false)
            {
                neighborCountToolStripMenuItem.Checked = false;
                neighborsCountToolStripMenuItem.Checked = false;
            }

            // Checks to see previously saved HUD View setting and updates the menu checkmark accordingly
            
            if (isHudVisible == true)
            {
                hUDToolStripMenuItem.Checked = true;
                viewHUDToolStripMenuItem.Checked = true;
            }
            else if (isHudVisible == false)
            {
                 hUDToolStripMenuItem.Checked = false;
                 viewHUDToolStripMenuItem.Checked = false;
            }

            if(isRGB == true)
            {
                rGBModeToolStripMenuItem.Checked = true;
            }
            else if(isRGB == false)
            {
                rGBModeToolStripMenuItem.Checked = false;
            }

            if(isToroidal == true)
            {
                toroidalToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem1.Checked = true;
                finiteToolStripMenuItem.Checked = false;
                finiteToolStripMenuItem1.Checked = false;
            }
            else if(isFinite == true)
            {
                finiteToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem1.Checked = true;
                toroidalToolStripMenuItem.Checked = false;
                toroidalToolStripMenuItem1.Checked = false;
            }
            
        }
    }

}
