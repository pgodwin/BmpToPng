using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmpConverter
{
    public class ColorEntry
    {
        public byte Red { get; set; }

        public byte Green { get; set; }

        public byte Blue { get; set; }


        /// <summary>
        /// Reservered, though it's Alpha on 32-bit bitmaps
        /// </summary>
        public byte Reservered { get; set; }

        public ColorEntry(int r, int g, int b, int a) : this((byte)r, (byte)g, (byte)b, (byte)a) { }
        

        public ColorEntry(byte r, byte g, byte b, byte a)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
            this.Reservered = a;
        }
    }
}
