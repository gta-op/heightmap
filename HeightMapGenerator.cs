using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Windows.Forms;
using System.IO;

namespace HeightMap
{
    /// <summary>
    /// This is a simple GTA V mod that will extract create a height map for the game.
    /// It is released under the conditions of the MIT license
    /// </summary>
    /// <author>
    /// Draex (https://forum.gtanet.work/index.php?members/draex.4963/)
    /// </author>
    public class HeightMapGenerator : Script
    {
        // ==================== ADJUSTABLE PARAMETERS ==================== //

        // Columns (direction x), Rows (direction y)
        public static readonly int Columns = 300, Rows = 90;
        // Cell size x & y in steps (x should not be very big, bad results otherwise)
        public static readonly int SizeX = 30, SizeY = 150;
        // Step size (less => more detailed, bigger file)
        public static readonly float Step = 1f;
        // Position where the script starts generating the hmap
        public static readonly float StartX = -4100f, StartY = -4300f;
        // How many point may be have z=0 (usually they're bugged or in the sea (map edges))
        public int ZeroTolerance = 10;
        // Maximum retries for cells that weren't tolerable because of two many 0s
        public int RetryMax = 10;
        // Ignore the tolerance when there aren't more then the given points above sea level
        // (prevents massive slow down at the map boarders, setting it to 0 will slow down the script heavily)
        public int IgnoreTolerance = 5;
        // File name for the height map (with full path)
        public readonly string FileName = "D:\\Spiele\\Steam\\steamapps\\common\\Grand Theft Auto V\\hmap_high.dat";

        // ==================== OTHER FIELDS ==================== //

        // Position of current cell
        private float _currentX = StartX, _currentY = StartY;
        // Counter for cols&rows
        private int _columnCount, _rowCount;
        // Direction (-1=>backwards, 1=>forwards)
        private int _direction = 1;
        // 2D data array
        private readonly float[,] _data = new float[SizeY * Rows, SizeX * Columns];
        // Counter for retries
        private int _retryCount;

        // Mod activity state
        public bool Active;

        public HeightMapGenerator()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Using key O to (dis-)activate
            if (e.KeyCode != Keys.O) return;

            // Save when active
            if (Active)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(FileName, FileMode.Create)))
                {
                    foreach (var f in _data) writer.Write(f);
                }
            }

            Active = !Active;
        }



        private void OnTick(object sender, EventArgs e)
        {
            // Active & not done
            if (!Active || _rowCount >= Rows) return;

            // Teleport the player to the center of the cell
            var x = _currentX + SizeX * Step / 2f * _direction;
            var y = _currentY + SizeY * Step / 2f;
            var ground = World.GetGroundHeight(new Vector2(x, y));

            Game.Player.Character.Position = new Vector3(x, y, ground + 50);

            // Variables to count 0f and >0f
            var zero = 0;
            var greaterZero = 0;

            // Iterate through the cell
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    // Get ground height for the point
                    var f = World.GetGroundHeight(new Vector2(_currentX + Step * i, _currentY + Step * j));
                    // Save height to data
                    if (_direction < 0) _data[j + _rowCount * SizeY, i + Columns * SizeX - (_columnCount + 1) * SizeX] = f;
                    else _data[j + _rowCount * SizeY, i + _columnCount * SizeX] = f;

                    // Increment counters
                    if (f > 0) greaterZero++;
                    else if (f == 0) zero++;
                }
            }

            // Check, if the cell must be recalculated
            if (greaterZero <= IgnoreTolerance || zero < ZeroTolerance || _retryCount >= RetryMax)
            {
                // Reset retry counter, mark column as done
                _retryCount = 0;
                _columnCount++;

                // Check if the row is done
                if (_columnCount == Columns)
                {
                    // Reset col counter, mark row as done
                    _columnCount = 0;
                    _rowCount++;

                    // Update current pos(y) and inverse direction
                    _currentY += SizeY * Step;
                    _direction *= -1;
                }
                // Update current pos(x)
                else _currentX += SizeX * Step * _direction;
            }
            else _retryCount++;
        }
    }
}