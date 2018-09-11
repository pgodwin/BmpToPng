namespace BmpConverter
{
    public enum CompressionType : int
    {
        /// <summary>
        /// No compression
        /// </summary>
        NO_COMPRESSION = 0,
        /// <summary>
        /// 8bit RLE Compression
        /// </summary>
        RLE8 = 1,
        /// <summary>
        /// 4bit RLE Compression
        /// </summary>
        RLE4 = 2,
        /// <summary>
        /// 16/32bit "bit field" compression
        /// </summary>
        BITFIELDS = 3,

        /// <summary>
        /// JPEG Compression
        /// </summary>
        JPEG = 4,

        /// <summary>
        /// PNG Compression
        /// </summary>
        PNG = 5
    }
}
