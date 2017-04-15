# heightmap
GTA V height map generator (v1.0)

## What is this?
This is a script that enables you to extract the GTA V height map.
Also the project includes a python script that can unite two maps to a corrected one.

## Dependencies

- GTA V
- scripthookv
- scripthookv .NET

For the python script:

- Python 3
- Numpy
- Matplotlib

## How to extract the map?

- Place HeightMapGenerator.cs in your GTA V script folder
- Go into GTA V (windowed mode is preferable)
- Press Key O to start the script
- Get a cup of coffee and enjoy getting warped around in the world because this will take ~30min
- Press Key O again when the script's done to save

## Adjust the script

There are several parameters that can be adjusted in the c# file.
Just go read the description above them.

## What is the python script for exactly

You won't get perfect results from the script, so if you need an almost completly correct height map,
you'll have to combine a few maps to achieve that.
GTA V returns 0 for non-loaded ground heights, so you can just fill in another map there.

Also Matplotlib is used to plot the map (looks awesome)

## How do I use the python script

Read the data if you're just want to plot the map:
```python
x = read("hmap.dat", (90*150, 30*300))
```

To correct the map use correct instead of read like this:
```python
x = correct("hmap.dat", "hmap2.dat", (90*150, 30*300), visualize=0, visualize_report=0, mem_usage=1)
```

If you don't got much RAM you can go for this (slower):
```python
x = correct("hmap.dat", "hmap2.dat", (90*150, 30*300), mem_usage=0)
```

To visualize the areas that can be corrected go for:
```python
x = correct("hmap.dat", "hmap2.dat", (90*150, 30*300), visualize=1)
```

The script will create a temp file for saving the data, you can rename it with
```python
tmp_file="your_file_name"
```

And so on, if you need more args, take a look at the method head in the python script


If you don't want data below or above a certain value use numpys clip:
```python
x = x.clip(-100, 1000)
```

To plot the map you can use
```python
plot(x)
```
or
```python
plot(x, True)
```
to knock out heights=0

To save the corrected data you can use:
```python
x.tofile("hmap.out.dat")
```

## GTA Network embedding

Use the script inside the GTA Network folder like this:

- Load the map: ```csharp
var hmap = new HeightMap("path/to/the/hmap.dat", -4100f, -4300f, -4100f+300*30, -4300f+150*90);
```
- Get the ground height at a pos: ```csharp
var z = hmap.Get(player.position.X, player.position.Y);
```

The loading script is taken from out GTA:N server (opposing forces)[http://www.gta-op.com].
The server uses it's own math library including a 2D Vector class, which we will release someday.
When we do so this script will also be included in a modified version (because 2d vectors are much better to handle then 2 float vals)

## Images

Default plot
![default](http://www.gta-op.com/hmap/default.jpg)

Plot without mapborder
![default](http://www.gta-op.com/hmap/island.jpg)

Island above the sea
![default](http://www.gta-op.com/hmap/above_sea.jpg)

Some fancy stuff with visualize (1 file: a good map, 2 file: another map limited to z<=300)
- yellow => both 0
- green => one is 0
- light blue => both equal
- dark blue => both differ
![default](http://www.gta-op.com/hmap/valid.png)
