namespace Mandelbrot
{
  using Color = System.ValueTuple<byte, byte, byte>;

  public struct ComputeParameters
  {
    public double MinX { get; }
    public double MaxX { get; }
    public double MinY { get; }
    public double MaxY { get; }
    public int Height { get; }
    public int Width { get; }

    public ComputeParameters(int width, int height, double minX, double maxX, double minY, double maxY)
    {
      Width = width;
      Height = height;
      MinX = minX;
      MaxX = maxX;
      MinY = minY;
      MaxY = maxY;
    }
  }

  public class MandelbrotBuilder
  {
    int _maxIterations = 256;
    double _limit = 4.0;


    public MandelbrotBuilder(int maxIterations, double limit)
    {
      _maxIterations = maxIterations;
      _limit = limit;
    }

    private int Compute(double cx, double cy)
    {
      (double x, double y) = (0.0, 0.0);
      int count = 0;
      while (x * x + y * y <= _limit && count < _maxIterations)
      {
        double xt = x * x - y * y + cx;
        double yt = 2.0 * x * y + cy;
        x = xt;
        y = yt;
        count += 1;
      }

      return count;
    }

    public Color[,] BuildImage(ComputeParameters parameters)
    {
      (int width, int height) = (parameters.Width, parameters.Height);
      (double minX, double maxX) = (parameters.MinX, parameters.MaxX);
      (double minY, double maxY) = (parameters.MinY, parameters.MaxY);

      var image = new Color[width, height];

      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          double cx = (double)x / width * (maxX - minX) + minX;
          double cy = (double)y / height * (maxY - minY) + minY;

          int count = Compute(cx, cy);

          Color color = (0, 0, 0);
          if (count > 0)
          {
            byte r = (byte)(count % 256);
            byte g = (byte)((count / 256) % 256);
            byte b = (byte)((count / 65535) % 256);
            color = (r, g, b);
          }

          image[x, y] = color;
        }
      }

      return image;
    }
  }
}