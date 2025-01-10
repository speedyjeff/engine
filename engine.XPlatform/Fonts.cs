using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace engine.XPlatform
{
    public static class Fonts
    {
        public static void Load(Stream stream)
        {
            All.Add(stream);
        }

        #region private
        internal static FontCollection All;

        static Fonts()
        {
            All = new FontCollection();
        }
        #endregion
    }
}
