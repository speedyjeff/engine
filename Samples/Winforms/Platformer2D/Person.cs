using engine.Common;
using engine.Common.Entities;
using engine.Winforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Samples.Winforms
{
    class Person : Player
    {
        public Person()
        {
            Index = 0;
            Delay = MaxDelay;
            ShowDefaultDrawing = false;
        }

        public override void Draw(IGraphics g)
        {
            if (Images == null)
            {
                // load
                var images = Resources.LoadImages(System.Reflection.Assembly.GetExecutingAssembly());
                Images = new IImage[images.Values.Count() ];
                var count = 0;
                foreach(var img in images.Values)
                {
                    img.MakeTransparent(RGBA.White);
                    Images[count++] = img;
                }
            }

            g.Image(Images[Index], X-(Width/2), Y-(Height/2), Width, Height);

            if (Delay-- < 0)
            {
                Index = (Index + 1) % Images.Length;
                Delay = MaxDelay;
            }

            base.Draw(g);
        }

        #region private
        private IImage[] Images;
        private int Index;
        private const int MaxDelay = 5;
        private int Delay;
        #endregion
    }
}
