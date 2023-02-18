using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Mandelbrot;

int width = 10000;
int height = 10000;
MandelbrotBuilder mb = new(maxIterations: 1000, limit: 4.0);
ComputeParameters computeParameters = new(width: width, height: height, minX: -2.0, maxX: 1.0, minY: -1.5, maxY: 1.5);
var imageBytes = mb.BuildImage(computeParameters);

using (Image<Rgba32> image = new(width, height))
{
  for (int x = 0; x < width; x++)
  {
    for (int y = 0; y < height; y++)
    {
      (byte r, byte g, byte b) = imageBytes[x, y];
      image[x, y] = new Rgba32(r, g, b);
    }
  }

  image.SaveAsPng("test.png");
}
