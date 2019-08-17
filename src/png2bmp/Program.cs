using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace png2bmp
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("png2bmp target_png_file [threshold(0-255)]");
                return;
            }
            try
            {
                int threshold = 128;
                if(args.Length == 2)
                {
                    threshold = System.Convert.ToInt32(threshold);
                }
                Convert(args[0], threshold);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void SaveBmp565(string filename, Image<Rgba32> img)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using(BinaryWriter bw = new BinaryWriter(fs))
                {
                    int offset = 70;
                    int width = img.Width;
                    int height = img.Height;
                    int dataSize = img.Width * img.Height * 2;
                    int fileSize = offset + dataSize;
                    int resolution = 0x2e23;

                    // RGB565 Bitmap Header
                    bw.Write(new byte[] {0x42,0x4d});
                    bw.Write(fileSize);
                    bw.Write((Int32)0);
                    bw.Write(offset);
                    bw.Write((Int32)0x38);
                    bw.Write(width);
                    bw.Write(height);
                    bw.Write((Int16)1);
                    bw.Write((Int16)16);    // bit per pixel
                    bw.Write((Int32)3);     // why?
                    bw.Write(dataSize);
                    bw.Write(resolution);
                    bw.Write(resolution);
                    bw.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF8, 0x00, 0x00,
                        0xE0, 0x07, 0x00,0x00,0x1F,0x00,0x00,0x00,0x00,0x00,0x00,0x00 });
                    for (int y = height-1; y >= 0; y--)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int val = (img[x, y].B >> 3) | ((img[x, y].G >> 2) << 5) | ((img[x, y].R >> 3) << 11);
                            UInt16 data = (UInt16)(val & 0xFFFF);
                            bw.Write(data);
                        }
                    }
                }
            }
        }
        static void Convert(string filename, int threshold)
        {
            string dir = Path.GetDirectoryName(filename);
            string name = Path.GetFileNameWithoutExtension(filename);
            string imagePath = Path.Combine(dir, name + ".bmp");
            string maskPath = Path.Combine(dir, name + ".m.bmp");

            using (Image<Rgba32> img = Image.Load(filename))
            {
                int width = img.Width;
                int height = img.Height;

                using (var maskImg = img.Clone())
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int mask = img[x, y].A;
                            if (mask >= threshold)
                            {
                                maskImg[x, y] = NamedColors<Rgba32>.White;
                                img[x, y] = new Rgba32(img[x, y].R, img[x, y].G, img[x, y].B, 255);
                            }
                            else
                            {
                                maskImg[x, y] = NamedColors<Rgba32>.Black;
                                img[x, y] = new Rgba32(0,0,0, 255);
                            }
                        }
                    }
                    SaveBmp565(imagePath,img);
                    SaveBmp565(maskPath,maskImg);
                }
            }
        }
    }
}
