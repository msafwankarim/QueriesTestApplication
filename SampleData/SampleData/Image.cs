using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Alachisoft.NCache.Sample.Data
{
    [Serializable]
    public class Image
    {
        public string FileName { get; set; }  
        public byte[] Data { get; set; }      
        public int Width { get; set; }        
        public int Height { get; set; }      
        public DateTime CreatedAt { get; set; }
        public ImageFormat[] ImageFormats { get; set; }

        private static int itemcount = 0;

   
        public Image()
        {
            FileName = "skeleton" + itemcount++;
            Data = new byte [10];
            Width = 10;
            Height = 10;
          
            CreatedAt = DateTime.Now;
            ImageFormats = new ImageFormat[2] { new ImageFormat(".Jpeg") { },new ImageFormat(".Png") { } }  ;

        }

        public Image(string fileName, byte[] data, int width, int height, string format, DateTime createdAt, ImageFormat[] formats)
        {
            FileName = fileName;
            Data = data;
            Width = width;
            Height = height;
            CreatedAt = createdAt;
            ImageFormats = formats; 
        }



    }

    [Serializable]
    public class ImageFormat
    {
        public  string Format { get; set; }

        public FormatSubArray[] formatSubArray { get; set; }

        public ImageFormat() { }
        public ImageFormat(string format ) { Format = format; }
    }

    [Serializable]
    public class FormatSubArray
    {
        public string Name { get; set; }

        public FormatSubArray() { }
        public FormatSubArray(string name) { Name = name; }
    }

}
