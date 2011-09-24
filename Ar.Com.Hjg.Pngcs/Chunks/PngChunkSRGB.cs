// --------------------------------------------------------------------------------------------------
// This file was automatically generated by J2CS Translator (http://j2cstranslator.sourceforge.net/). 
// Version 1.3.6.20110331_01     
// 6/1/11 9:13 a.m.    
// ${CustomMessageForDisclaimer}                                                                             
// --------------------------------------------------------------------------------------------------
 namespace Ar.Com.Hjg.Pngcs.Chunks {
	
	using Ar.Com.Hjg.Pngcs;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Runtime.CompilerServices;

     /*
      * // http://www.w3.org/TR/PNG/#11sRGB
      */
     public class PngChunkSRGB : PngChunk {
		// http://www.w3.org/TR/PNG/#11PLTE

   	public static readonly int RENDER_INTENT_Perceptual = 0;
	public static readonly int RENDER_INTENT_Relative_colorimetric = 1;
	public static readonly int RENDER_INTENT_Saturation = 2;
	public static readonly int RENDER_INTENT_Absolute_colorimetric	 = 3;

		public int intent;
	
		public PngChunkSRGB(ImageInfo info) : base(Ar.Com.Hjg.Pngcs.Chunks.ChunkHelper.sRGB_TEXT, info) {
		}


        public override void ParseFromChunk(ChunkRaw c)
        {
            if (c.len != 1)
                throw new PngjException("bad chunk length " + c);
            intent = PngHelper.ReadInt1fromByte(c.data, 0);
        }

		public override ChunkRaw CreateChunk() {
            ChunkRaw c = null;
            c = CreateEmptyChunk(1, true);
            c.data[0] = (byte)intent;
            return c;
		}
	
        
         public override void CloneDataFromRead(PngChunk other) {
             PngChunkSRGB otherx = (PngChunkSRGB)other;
             intent = otherx.intent;
		}
	}
}