using Hjg.Pngcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmpConverter
{
    public class Bitmap
    {
        public Bitmap(Stream stream)
        {
            byte[] signature = new byte[2];
            stream.Read(signature, 0, signature.Length);

            if (Encoding.UTF8.GetString(signature) != BitmapHeader.FILE_HEADER)
                throw new Exception("Not a bitmap!");

            this.Header = new BitmapHeader(stream);

            if (this.Header.BitCount <= 8)
            {
                // read the colour table
                ColorTable = new ColorEntry[Header.NumberOfColours];
                for (int i = 0; i < Header.NumberOfColours; i++)
                {
                    ColorTable[i] = new ColorEntry(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
                }
            }


            if (Header.Compression != CompressionType.NO_COMPRESSION)
                throw new Exception("Compressed bitmaps not supported (yet)");

            this.Stream = stream;
            
        }

        public BitmapHeader Header { get; }

        public ColorEntry[] ColorTable { get; private set; }
        public Stream Stream { get; }

        public void ConvertToPng(Stream outStream)
        {
            // Only works with 24-bit images right now.
            using (var reader = new BinaryReader(this.Stream, Encoding.Default, true))
            {
                ImageInfo info = new ImageInfo(Header.Width, Header.Height, 8, Header.BitCount == 32);
                PngWriter png = new PngWriter(outStream, info);
                
                //padding to nearest 32 bits
                int dataPerLine = Header.Width * 3; // 3 channels
                int bytesPerLine = dataPerLine;
                if (bytesPerLine % 4 != 0)
                {
                    bytesPerLine = (bytesPerLine / 4 + 1) * 4; // + reserved channel
                }
                int padBytesPerLine = bytesPerLine - dataPerLine;

                // Bitmap is written bottom to top (annoyingly)
                // For a file, this is okay but for a non-seekable stream we're going to have a problem
                if (this.Stream.CanSeek == false)
                    throw new Exception("Stream must be seekable to do the conversion");

                
                long dataStart = Header.DataOffset;
                int dataEnd = Header.ImageSize;


                //for (int y = Header.Height - 1; y >= 0; y--)
                for (int y = 0; y < Header.Height; y++)
                {
                    // Seek to the start of the line
                    Stream.Seek(dataStart + (dataEnd - (bytesPerLine * y) - bytesPerLine), SeekOrigin.Begin);
                    
                    // build the row data - this going to be slow for big images sadly.
                    // not too much we can do, we're going for safety, not performance.
                    ImageLine pngLine = new ImageLine(info);
                    
                    for (int x = 0; x < Header.Width; x++)
                    {
                        int b = reader.ReadByte();
                        int g = reader.ReadByte();
                        int r = reader.ReadByte();
                        ImageLineHelper.SetPixel(pngLine, x, r, g, b);
                    }
                    
                    png.WriteRow(pngLine, y);
                    Stream.ReadByte(); // ignore the last byte
                }
                png.End();
                
            }
        }
    }
}
