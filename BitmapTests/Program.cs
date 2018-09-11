using BmpConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmapTests
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var f in Directory.EnumerateFiles(".\\tests", "*.bmp"))
            {
                using (var file = File.Open(f, FileMode.Open))
                {
                    Bitmap bmp = new Bitmap(file);
                    Console.WriteLine("W: {0}, H: {1}", bmp.Header.Width, bmp.Header.Height);
                    using (var png = File.Create(f + ".png"))
                    {
                        bmp.ConvertToPng(png);
                    }
                }
            }
            
        }
    }
}
