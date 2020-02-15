using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities
{
    public class Background : Element
    {
        public Background(int width, int height) : base()
        {
            CanMove = false;
            TakesDamage = false;
            ShowDamage = false;
            IsSolid = false;
            CanAcquire = false;
            IsTransparent = false;
            BasePace = 2;

            // dimensions
            Width = width;
            Height = height;

            // center
            X = 0;
            Y = 0;
        }

        public RGBA GroundColor { get; set; }
        public float BasePace { get; set; }

        public override void Draw(IGraphics g)
        {
            g.Clear(GroundColor);
            base.Draw(g);
        }

        public virtual void Update()
        {
        }

        public virtual float Pace(float x, float y)
        {
            return BasePace;
        }

        public virtual float Damage(float x, float y)
        {
            return 0;
        }
    }
}
