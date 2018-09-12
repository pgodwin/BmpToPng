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
        
        public int HeaderSize;
        public HeaderVersion Version;
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

        /// <summary>
        /// 1 channel for 8bit images, 3 for 24bit and 4 for 32bit.
        /// </summary>
        public int Channels => BitCount <= 8 ? 1 : BitCount == 24 ? 3 : 4; 

        public int BitDepth => BitCount / Channels;

      
        public BitmapHeader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                FileSize = reader.ReadInt32();
                Reservered = reader.ReadInt32();
                DataOffset = reader.ReadInt32();

                // Header begins
                HeaderSize = reader.ReadInt32();
                Version = (HeaderVersion)HeaderSize;

                if (Version != HeaderVersion.BITMAP_INFO_HEADER_SIZE)
                    throw new Exception("Unsupported header version. Only 40byte headers supported for now.");

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
    
    /// <summary>
    /// Bitmap version based on the header size (first field).
    /// </summary>
    public enum HeaderVersion : int
    {

        /// <summary>
        /// OS/2 V2
        /// </summary>
        OS2_V2_HEADER_SIZE = 64,

        /// <summary>
        /// OS/2 V2, first 16 bytes
        /// </summary>
        OS2_V2_HEADER_16_SIZE = 16,

        /// <summary>
        /// OS/2 V1
        /// </summary>
        BITMAP_CORE_HEADER_SIZE = 12,

        /// <summary>
        /// Windows 3.0 and later, most common format
        /// </summary>
        BITMAP_INFO_HEADER_SIZE = 40,
        /// <summary>
        /// Undocoumented, written by Photoshop
        /// </summary>
        BITMAP_V2_INFO_HEADER_SIZE = 52,
        /// <summary>
        /// Undocumented, written by Photoshop
        /// </summary>
        BITMAP_V3_INFO_HEADER_SIZE = 56,

        /// <summary>
        /// V4, Windows 95/NT 4 and later
        /// </summary>
        BITMAP_V4_INFO_HEADER_SIZE = 108,

        /// <summary>
        /// V5, Windows 98/2000 and later
        /// </summary>
        BITMAP_V5_INFO_HEADER_SIZE = 124

    }
}
