﻿using System;
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
                    DisplayName = string.Format("[{0}] {1}", Constants.Pickup2, Name);
                    PreviousName = Name;
                }
                g.Text(RGBA.Black, X - Width / 2, Y - Height / 2 - 20, DisplayName);
            }
            if (TakesDamage && ShowDamage)
            {
                if (Health != PreviousHealth || Shield != PreviousShield)
                {
                    PreviousShield = Shield;
                    PreviousHealth = Health;
                    DisplayHealth = string.Format("{0:0}/{1:0}", Health, Shield);
                }
                g.Text(RGBA.Black, X - Width / 2, Y - Height / 2 - 20, DisplayHealth);
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
    }
}
