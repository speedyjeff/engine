using engine.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public class Map3D : Map
    {
        public Map3D(int width, int height, int depth, Player[] players, Element[] objects, Background background)
        {
            Initialize(width: width, height: height, depth: depth, players: players, objects: objects, background: background);
        }

        public override bool IsTouching(Element elem1, Element elem2, float x1delta = 0, float y1delta = 0, float z1delta = 0)
        {
            // bounding box test first
            if (base.IsTouching(elem1, elem2, x1delta, y1delta, z1delta))
            {
                // Projectiles already use a swept-path collision check before they reach
                // this point. When the target is a player, a simple bounding-box hit is
                // enough and avoids the very expensive humanoid triangle-vs-triangle path.
                if ((elem1 is ShotTrajectory || elem2 is ShotTrajectory) && (elem1 is Player || elem2 is Player))
                {
                    return true;
                }

                // if not using detailed collision, assume touching if bounding boxes are touching (or close enough with delta)
                if (!elem1.UseDetailedCollision || !elem2.UseDetailedCollision)
                {
                    return true;
                }

                // get the 3D object's polygons
                var e1_3D = GetCollisionBody(elem1);
                var e2_3D = GetCollisionBody(elem2);

                // test if both have polygons
                if (e1_3D != null && e2_3D != null)
                {
                    // TODO! - ugh, n^2 algorithm (perhaps sorting?)

                    var p1 = new Point();
                    var q1 = new Point();
                    var r1 = new Point();
                    var p2 = new Point();
                    var q2 = new Point();
                    var r2 = new Point();

                    // ensure to scale and position via X,Y,Z and Width,Height,Depth

                    // check if the polygons in elem1 are touching any of the polygons in elem2
                    for (int i = 0; i < e1_3D.Polygons.Length; i++)
                    {
                        if (e1_3D.Polygons[i].Length >= 3)
                        {
                            for (int j = 0; j < e2_3D.Polygons.Length; j++)
                            {
                                if (e2_3D.Polygons[j].Length >= 3)
                                {
                                    p1.X = (e1_3D.Polygons[i][0].X * e1_3D.Width) + e1_3D.X + x1delta; p1.Y = (e1_3D.Polygons[i][0].Y * e1_3D.Height) + e1_3D.Y + y1delta; p1.Z = (e1_3D.Polygons[i][0].Z * e1_3D.Depth) + e1_3D.Z + z1delta;
                                    q1.X = (e1_3D.Polygons[i][1].X * e1_3D.Width) + e1_3D.X + x1delta; q1.Y = (e1_3D.Polygons[i][1].Y * e1_3D.Height) + e1_3D.Y + y1delta; q1.Z = (e1_3D.Polygons[i][1].Z * e1_3D.Depth) + e1_3D.Z + z1delta;
                                    r1.X = (e1_3D.Polygons[i][2].X * e1_3D.Width) + e1_3D.X + x1delta; r1.Y = (e1_3D.Polygons[i][2].Y * e1_3D.Height) + e1_3D.Y + y1delta; r1.Z = (e1_3D.Polygons[i][2].Z * e1_3D.Depth) + e1_3D.Z + z1delta;

                                    p2.X = (e2_3D.Polygons[j][0].X * e2_3D.Width) + e2_3D.X; p2.Y = (e2_3D.Polygons[j][0].Y * e2_3D.Height) + e2_3D.Y; p2.Z = (e2_3D.Polygons[j][0].Z * e2_3D.Depth) + e2_3D.Z;
                                    q2.X = (e2_3D.Polygons[j][1].X * e2_3D.Width) + e2_3D.X; q2.Y = (e2_3D.Polygons[j][1].Y * e2_3D.Height) + e2_3D.Y; q2.Z = (e2_3D.Polygons[j][1].Z * e2_3D.Depth) + e2_3D.Z;
                                    r2.X = (e2_3D.Polygons[j][2].X * e2_3D.Width) + e2_3D.X; r2.Y = (e2_3D.Polygons[j][2].Y * e2_3D.Height) + e2_3D.Y; r2.Z = (e2_3D.Polygons[j][2].Z * e2_3D.Depth) + e2_3D.Z;

                                    // if true, exit early
                                    if (Utilities3D.IntersectingTriangles(p1, q1, r1, p2, q2, r2))
                                        return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                // go with the bounding box decision
                return true;
            }

            // not touching
            return false;
        }

        #region private
        private static Element3D GetCollisionBody(Element elem)
        {
            if (elem is Element3D element3D) return element3D;
            if (elem is Player3D player3D) return player3D.Body;
            if (elem is ShotTrajectory3D shot3D) return shot3D.Body;
            return null;
        }

        protected override bool SupportsTerrainStepUp => true;

        protected override bool TrackAttackTrajectory(Player player, Tool weapon, out List<Element> hit, out List<ShotTrajectory> trajectories)
        {
            // init
            hit = new List<Element>();
            trajectories = new List<ShotTrajectory>();

            var projectileColor = new RGBA() { R = 255, A = 255 };
            var projectileSize = 10f;
            if (weapon is RangeWeapon3D weapon3D)
            {
                projectileColor = weapon3D.ProjectileColor;
                projectileSize = weapon3D.ProjectileSize;
            }

            // provide a trajectory that takes into account the players yaw and pitch
            var x1 = Math.Max(player.Width * 0.16f, projectileSize * 0.5f);
            var y1 = -1 * Math.Max(player.Height * 0.30f, projectileSize + 8f);
            var z1 = -1 * Math.Max(player.Depth * 1.1f, (player.Depth / 2f) + projectileSize + 12f);

            var x2 = x1;
            var y2 = y1;
            var z2 = z1 - Math.Max(player.Depth * 4f, 200f);

            Utilities3D.Yaw(360f - player.Angle, ref x1, ref y1, ref z1);
            Utilities3D.Yaw(360f - player.Angle, ref x2, ref y2, ref z2);
            //Utilities3D.Pitch(player.PitchAngle, ref x2, ref y2, ref z2);

            // add projectile
            var trajectory = new ShotTrajectory3D(x: player.X + x1, y: player.Y + y1, z: player.Z + z1)
            {
                SourcePlayerId = player.Id,
                X1 = player.X + x1,
                Y1 = player.Y + y1,
                Z1 = player.Z + z1,
                X2 = player.X + x2,
                Y2 = player.Y + y2,
                Z2 = player.Z + z2,
                Damage = weapon.Damage,
                Width = projectileSize,
                Height = projectileSize,
                Depth = projectileSize,
            };

            if (trajectory.Body != null)
            {
                trajectory.Body.Width = projectileSize;
                trajectory.Body.Height = projectileSize;
                trajectory.Body.Depth = projectileSize;
                trajectory.Body.UniformColor = projectileColor;
            }

            trajectories.Add(trajectory);

            return true;
        }
        #endregion
    }
}
