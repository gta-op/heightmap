using System.IO;
using System.IO.MemoryMappedFiles;

/// <summary>
/// GTA V heightmap loader
/// This script released under the conditions of the MIT license
/// </summary>
///
/// <author>
/// Author: Draex (https://forum.gtanet.work/index.php?members/draex.4963/)
/// </author>
public class HeightMap
{
    private readonly float _startX, _startY;
    private readonly float _endX, _endY;
    private readonly string _file;

    /// <summary>
    /// Load heightmap data from a file (resolution 1.0f x 1.0f)
    /// </summary>
    /// <param name="file">Heightmap containing floats (direction x first)</param>
    /// <param name="startX">Start X</param>
    /// <param name="startY">Start Y</param>
    /// <param name="endX">End X</param>
    /// <param name="endY">End Y</param>
    public HeightMap(string file, float startX, float startY, float endX, float endY)
    {
        _file = file;

        _startX = startX;
        _startY = startY;
        _endX = endX;
        _endY = endY;
    }



    /// <summary>
    /// Get the z value of the ground at the given position
    /// </summary>
    /// <param name="posX">Position x</param>
    /// <param name="posY">Position y</param>
    /// <returns>Ground level</returns>
    public float Get(float posX, float posY) // TODO interpolation
    {
        if (!Contains(posX, posY)) return 0f;

        var mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.Open);
        using (mmf)
        {
            var x = (int) posX - (int) _startX;
            var y = (long) (_endX-_startX) * ((long) posY - (long) _startY);
            using (var accessor = mmf.CreateViewAccessor((y + x) * 4, 4))
            {
                return accessor.ReadSingle(0);
            }
        }
    }

    /// <summary>
    /// Checks if the height data of the given position is contained in the map
    /// </summary>
    /// <param name="x">Position x</param>
    /// <param name="y">Position y</param>
    /// <returns>True if contained</returns>
    public bool Contains(float x, float y)
    {
        return _startX < x && x < _endX && _startY < y && y < _endY;
    }
}
