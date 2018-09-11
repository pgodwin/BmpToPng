using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmpConverter
{
    public class BitmapHeader
    {
        // Constants: 
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/dd183376%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396 


        /// <summary>
        /// Header signature "BM".
        /// </summary>
        public const string FILE_HEADER = "BM";

        public int FileSize;
        public int Reservered;
        public int DataOffset;
        
        public int Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitCount;

        public CompressionType Compression;

        public int ImageSize;

        public int XPixelsPerM;

        public int YPixelsPerM;

        public int ColorsUsed;

        public int ColorsImportant;

        public int NumberOfColours => (int)Math.Pow(2, BitCount);

        public BitmapHeader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                FileSize = reader.ReadInt32();
                Reservered = reader.ReadInt32();
                DataOffset = reader.ReadInt32();

                // Header begins
                Size = reader.ReadInt32();
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();

                Planes = reader.ReadInt16();
                Debug.Assert(Planes == 1);

                BitCount = reader.ReadInt16();
                Compression = (CompressionType)reader.ReadInt32();
                ImageSize = reader.ReadInt32();

                XPixelsPerM = reader.ReadInt32();
                YPixelsPerM = reader.ReadInt32();
                ColorsUsed = reader.ReadInt32();
                ColorsImportant = reader.ReadInt32();
            }
        }
    }
}
