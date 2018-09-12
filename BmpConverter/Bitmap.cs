using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
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

            // Bitmap is written bottom to top (annoyingly)
            // For a file, this is okay but for a non-seekable stream we're going to have a problem
            if (this.Stream.CanSeek == false)
                throw new Exception("Stream must be seekable to do the conversion");



            // Only works with 24-bit images right now.
            using (var reader = new BinaryReader(this.Stream, Encoding.Default, true))
            {
                if (Header.BitCount <= 8)
                {
                    ConvertPalettedImage(reader, outStream);
                }
                else if (Header.BitCount == 16)
                {
                    Convert16BitImage(reader, outStream);
                }
                else
                {
                    ConvertTrueColor(reader, outStream);
                }
            }

        }

        private void ConvertPalettedImage(BinaryReader reader, Stream outStream)
        {
            ImageInfo info = new ImageInfo(Header.Width, Header.Height, Header.BitDepth, false, false, true);
            PngWriter png = new PngWriter(outStream, info);

            // Add a palette to the PNG
            PngChunkPLTE plte = png.GetMetadata().CreatePLTEChunk();
            plte.SetNentries(Header.NumberOfColours);
            // Populate the palette
            for (int i = 0; i < this.ColorTable.Length; i++)
            {
                var c = this.ColorTable[i];
                plte.SetEntry(i, c.Red, c.Green, c.Blue);
            }

            int pixelsPerByte = 8 / Header.BitCount;
            int bytesPerLine = (Header.Width * Header.BitCount + 31) / 32 * 4;
            //if (Header.BitDepth == 8)
            //    bytesPerLine = (Header.Width / 4 + 1) * 4;
            
            long dataStart = Header.DataOffset;
            int dataEnd = Header.FileSize - Header.DataOffset; //Header.ImageSize;
            
            for (int y = 0; y < Header.Height; y++)
            {
                // Seek to the start of the line
                Stream.Seek(dataStart + (dataEnd - (bytesPerLine * y) - bytesPerLine), SeekOrigin.Begin);

                byte[] bmpLine = new byte[bytesPerLine];
                ImageLine pngLine = new ImageLine(info, ImageLine.ESampleType.BYTE);
                Stream.Read(bmpLine, 0, bytesPerLine);

                // Bitmap lines are already packed for palleted images, so just copy it over
                Array.Copy(bmpLine, 0, pngLine.ScanlineB, 0, Math.Min(bytesPerLine, Header.Width));

                Stream.ReadByte(); //skip the padded byte
                
                png.WriteRow(pngLine, y);
               
            }

            png.End();
        }


        public void ConvertTrueColor(BinaryReader reader, Stream outStream)
        {
            ImageInfo info = new ImageInfo(Header.Width, Header.Height, Header.BitDepth, Header.BitCount == 32);
            PngWriter png = new PngWriter(outStream, info);

            // dataPerLine also known as ScanlineStride
            int dataPerLine = Header.Width * Header.Channels;
            // Calculation from https://en.wikipedia.org/wiki/BMP_file_format#Pixel_storage
            int bytesPerLine = ((Header.BitCount * Header.Width + 31) / 32) * 4;
            int padBytesPerLine = bytesPerLine - dataPerLine;


            long dataStart = Header.DataOffset;
            int dataEnd = Header.ImageSize;


            for (int y = 0; y < Header.Height; y++)
            {
                // Seek to the start of the line
                Stream.Seek(dataStart + (dataEnd - (bytesPerLine * y) - bytesPerLine), SeekOrigin.Begin);

                // build the row data - this going to be slow for big images sadly.
                // not too much we can do, we're going for safety, not performance.
                // we might be able to just pack the bytes in future.
                ImageLine pngLine = new ImageLine(info);

                for (int x = 0; x < Header.Width; x++)
                {
                    int b = reader.ReadByte();
                    int g = reader.ReadByte();
                    int r = reader.ReadByte();
                    int a = 255;
                    if (Header.BitCount == 32)
                        a = reader.ReadByte();

                    // having issues writing out the alpha component in the png
                    // ignoring it for now
                    ImageLineHelper.SetPixel(pngLine, x, r, g, b);
                }

                png.WriteRow(pngLine, y);
            }
            png.End();
        }

        public void Convert16BitImage(BinaryReader reader, Stream outStream)
        {
            // Force 8 bit images as PNG doesn't support 16bit iamges
            ImageInfo info = new ImageInfo(Header.Width, Header.Height, 8, false);
            PngWriter png = new PngWriter(outStream, info);


            int bytesPerLine = ((Header.BitCount * Header.Width + 31) / 32) * 4;

            long dataStart = Header.DataOffset;
            int dataEnd = Header.ImageSize;

            // Masks from https://docs.microsoft.com/en-us/windows/desktop/directshow/working-with-16-bit-rgb
            // Assuming RGB555, until I deal with packed bits. I don't expect these to be common for our purpsoses though.
            var red_mask = 0x7C00;
            var green_mask = 0x3E0;
            var blue_mask = 0x1F;

            for (int y = 0; y < Header.Height; y++)
            {
                var offSet = (Header.Height - y - 1) * Header.Width;
                // Seek to the start of the line
                Stream.Seek(dataStart + (dataEnd - (bytesPerLine * y) - bytesPerLine), SeekOrigin.Begin);

                // build the row data - this going to be slow for big images sadly.
                // not too much we can do, we're going for safety, not performance.
                ImageLine pngLine = new ImageLine(info);
              
                for (int x = 0; x < Header.Width; x++)
                {
                    var value = reader.ReadInt16();

                    int r = ((value & red_mask) >> 10) << 3;
                    int g = ((value & green_mask) >> 5) << 3;
                    int b = ((value & blue_mask)) << 3;
                    ImageLineHelper.SetPixel(pngLine, x, r, g, b);
                }

                png.WriteRow(pngLine, y);
                
            }
            png.End();
        }
    }
}
