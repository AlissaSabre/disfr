using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Doc
{
    public static class ZipUtils
    {
        public static bool IsZip(this Stream file)
        {
            var position = file.Position;
            try
            {
                return file.ReadByte() == 0x50
                    && file.ReadByte() == 0x4B
                    && file.ReadByte() == 0x03
                    && file.ReadByte() == 0x04;
            }
            finally
            {
                file.Position = position;
            }
        }
    }
}
