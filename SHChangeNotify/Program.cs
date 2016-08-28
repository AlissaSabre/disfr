using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SHChangeNotify
{
    class Program
    {
        /// <summary>
        /// Tells Windows Shell that file type association is changed in the registry. 
        /// </summary>
        /// <remarks>
        /// This program is used by the installer.
        /// </remarks>
        static void Main()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(Int32 wEventId, UInt32 uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const Int32 SHCNE_ASSOCCHANGED = 0x08000000;

        private const UInt32 SHCNF_IDLIST = 0;
    }
}
