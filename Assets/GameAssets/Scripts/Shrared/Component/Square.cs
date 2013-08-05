using System;
using System.Collections.Generic;

namespace DeltaCommon.Component
{
    public class Square
    {
#if (SILVERLIGHT || WINDOWS_PHONE)
        public const int kPixelSize=28;
#else
        public const int kPixelSize=32;
#endif

        public bool NorthExit { get; set; }
        public bool WestExit { get; set; }
        public bool SouthExit { get; set; }
        public bool EastExit { get; set; }

        // what is this for?
        public bool Occupied { get; set; }

        public Square()
        {
            Clear();
        }

        /// <summary>
        ///  resets the square 
        /// </summary>
        public void Clear()
        {
            NorthExit = false;
            WestExit = false;
            SouthExit = false;
            EastExit = false;
            Occupied = false;
        }
    }

}