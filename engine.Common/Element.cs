using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common
{
    public class Element
    {
        // id
        public int Id { get; set; }

        // center
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // bounds (hit box)
        public float Height { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }

        // attributes
        public float Health { get; set; } = 0;
        public float Shield { get; set; } = 0;
        public bool CanMove { get; protected set; } = false;
        public bool TakesDamage { get; protected set; } = false;
        public bool ShowDamage { get; protected set; } = false;
        public bool IsSolid { get; protected set; } = false;
        public bool CanAcquire { get; protected set; } = false;
        public bool IsTransparent { get; protected set; } = false;
        public string Name { get; set; } = "";

        public bool ShowDefaultDrawing { get; set; }

        public bool IsDead { get; protected set; } = false;

        public virtual ImageSource Image { get; set; }

        public Element()
        {
            Id = GetNextId();
            X = Y = 0;
            Z = Constants.Ground;
            ShowDefaultDrawing = true;
        }

        public virtual void Draw(IGraphics g)
        {
            if (Constants.Debug_ShowHitBoxes) g.Rectangle(RGBA.Black, X-(Width/2), Y-(Height/2), Width, Height, false);
            if (CanAcquire)
            {
                if (!string.Equals(Name, PreviousName))
                {
                    DisplayName = $"[{Constants.Pickup2}] {Name}";
                    PreviousName = Name;
                }
                g.Text(RGBA.Black, X + (Width/2), Y, DisplayName);
            }
            if (TakesDamage && ShowDamage)
            {
                if (Health != PreviousHealth || Shield != PreviousShield)
                {
                    PreviousShield = Shield;
                    PreviousHealth = Health;
                    DisplayHealth = $"{Health:0}/{Shield:0}";
                }
                g.Text(RGBA.Black, X - Width, Y - Height, DisplayHealth);
            }
        }

        // todo audit these if they are updating state appropriately

        public virtual void Move(float xDelta, float yDelta, float zDelta)
        {
            X += xDelta;
            Y += yDelta;
            Z += zDelta;
        }

        public void ReduceHealth(float damage)
        {
            if (Shield > 0)
            {
                if (Shield > damage)
                {
                    Shield -= damage;
                    return;
                }
                damage -= Shield;
                Shield = 0;
            }
            if (Health > damage)
            {
                Health -= damage;
                return;
            }
            Health = 0;
            IsDead = true;
            return;
        }

        #region private
        private string DisplayName;
        private string PreviousName;

        private string DisplayHealth;
        private float PreviousHealth;
        private float PreviousShield;

        private static int NextId = 0;
        private static int GetNextId()
        {
            return System.Threading.Interlocked.Increment(ref NextId);
        }
        #endregion

        #region internal
        internal void MakeStationary()
        {
            CanMove = false;
            TakesDamage = true;
            ShowDamage = true;
            CanAcquire = false;
            IsSolid = true;
        }
        #endregion
    }
}
