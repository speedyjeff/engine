﻿using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common.Entities
{
    public enum ActionEnum { None, SwitchPrimary, Pickup, Drop, Reload, Attack, Move, Jump, COUNT };

    public class Player : Element
    {
        public Player() : base()
        {
            CanMove = true;
            TakesDamage = true;
            IsSolid = true;
            Health = 50;
            Shield = 0;
            Color = new RGBA() { R = 255, A = 255 };
            Kills = 0;
            HandCapacity = 1;
            Secondary = new Element[HandCapacity];
            MaxYForcePercentage = 1;

            // hit box
            Height = 50;
            Width = 50;

            // special melee
            Fists = new Tool() { Distance = (int)Width, Damage = 5 };
        }

        // in hands
        public int HandCapacity { get; set; }
        public Element Primary { get; private set; }
        public Element[] Secondary { get; set; }

        public Tool Fists { get; protected set; }

        public RGBA Color { get; protected set; }

        public int Kills { get; set; }

        public int Ranking { get; set; }

        public float XForcePercentage { get; set; }
        public float YForcePercentage { get; set; }
        public float ZForcePercentage { get; set; }

        public float MaxYForcePercentage { get; protected set; }

        public virtual string HurtSoundPath => "hurt";

        public override void Draw(IGraphics g)
        {
            if (ShowDefaultDrawing)
            {
                if (Z > Constants.Ground)
                {
                    g.DisableTranslation(true /* nonScaledTranslation */);
                    {
                        // we are in a parachute
                        g.Ellipse(Color, X - (Width / 2), Y - (Height / 2), Width, Height);
                        g.Rectangle(new RGBA() { R = 146, G = 27, B = 167, A = 255 }, X - Width, Y, Width * 2, Height / 2, true);
                        g.Line(RGBA.Black, X - Width, Y, X, Y - (Height / 4), 5f);
                        g.Line(RGBA.Black, X, Y - (Height / 4), X + Width, Y, 5f);
                    }
                    g.EnableTranslation();
                }
                else
                {
                    // on ground
                    // calculate location for fists/object in hand
                    float x1, y1, x2, y2;
                    Collision.CalculateLineByAngle(X, Y, Angle, Width / 2, out x1, out y1, out x2, out y2);

                    // draw body
                    g.Ellipse(Color, X - Width / 2, Y - Height / 2, Width, Height, true);

                    // draw a fist
                    g.Ellipse(Color, x2, y2, Width / 3, Width / 3);
                }
            }
            base.Draw(g);
        }

        public bool Take(Element item)
        {
            if (item is Ammo)
            {
                if (Primary != null && Primary is RangeWeapon)
                {
                    var gun = (Primary as RangeWeapon);
                    gun.AddAmmo((int)item.Health);
                    return true;
                }
            }
            else if (item is Shield)
            {
                if (Shield < Constants.MaxShield)
                {
                    Shield += item.Shield;
                    if (Shield > Constants.MaxShield) Shield = Constants.MaxShield;
                    return true;
                }
            }
            else if (item is Health)
            {
                if (Health < Constants.MaxHealth)
                {
                    Health += item.Health;
                    if (Health > Constants.MaxHealth) Health = Constants.MaxHealth;
                    return true;
                }
            }
            else if (item.CanAcquire)
            {
                if (Primary == null)
                {
                    Primary = item;
                    return true;
                }

                // seek an open slot
                int index = -1;
                for (int i = 0; i < Secondary.Length; i++)
                {
                    if (Secondary[i] == null) index = i;
                }

                if (index >= 0)
                {
                    Secondary[index] = Primary;
                    Primary = item;
                    return true;
                }

                return false;
            }
            else throw new Exception("Unknow item : " + item.GetType());

            return false;
        }

        public AttackStateEnum Attack()
        {
            // check if we have a primary weapon
            if (Primary == null) return AttackStateEnum.Melee;

            if (Primary is RangeWeapon)
            {
                var gun = (Primary as RangeWeapon);

                // check if there is a round in the clip
                int rounds;
                gun.RoundsInClip(out rounds);
                if (rounds <= 0)
                {
                    if (gun.HasAmmo()) return AttackStateEnum.NeedsReload;
                    else return AttackStateEnum.NoRounds;
                }
                // check if gun ready to fire
                if (!gun.CanShoot()) return AttackStateEnum.LoadingRound;

                bool fired = gun.Shoot();
                if (fired) return AttackStateEnum.Fired;
                else throw new Exception("Failed to fire");
            }

            return AttackStateEnum.Melee;
        }

        public AttackStateEnum Reload()
        {
            if (Primary == null) return AttackStateEnum.None;

            if (Primary is RangeWeapon)
            {
                var gun = (Primary as RangeWeapon);

                // check if we have a primary weapon
                if (!gun.HasAmmo()) return AttackStateEnum.NoRounds;
                // check if there are rounds
                if (gun.RoundsInClip(out int rounds)) return AttackStateEnum.FullyLoaded;

                bool reload = gun.Reload();
                if (reload) return AttackStateEnum.Reloaded;
                else throw new Exception("Failed to reload");
            }

            return AttackStateEnum.None;
        }

        public bool SwitchPrimary()
        {
            // push the primary to the end of the secondary hand, and pop off the next
            // from the hand

            // shift the secondary hand
            var tmp = Secondary[0];
            for(int i=1; i<Secondary.Length; i++)
            {
                // shift
                Secondary[i - 1] = Secondary[i];
            }
            Secondary[Secondary.Length - 1] = Primary;

            // move the first in the hand to Primary
            Primary = tmp;

            return (Primary != null);
        }

        public Element DropPrimary()
        {
            if (Primary == null) return null;
            var tmp = Primary;
            Primary = null;
            return tmp;
        }

        public virtual void Feedback(ActionEnum action, object item, bool result)
        {
        }
    }
}
