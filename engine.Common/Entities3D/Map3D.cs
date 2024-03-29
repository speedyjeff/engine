﻿using engine.Common.Entities;
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
                // get the 3D object's polygons
                var e1_3D = (elem1 is Element3D) ? elem1 as Element3D : 
                    ((elem1 is Player3D) ? (elem1 as Player3D).Body : 
                    ((elem1 is ShotTrajectory3D) ? (elem1 as ShotTrajectory3D).Body : null) );
                var e2_3D = (elem2 is Element3D) ? elem2 as Element3D :
                    ((elem2 is Player3D) ? (elem2 as Player3D).Body :
                    ((elem2 is ShotTrajectory3D) ? (elem2 as ShotTrajectory3D).Body : null));

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
                    for (int i=0; i<e1_3D.Polygons.Length; i++)
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
        protected override bool TrackAttackTrajectory(Player player, Tool weapon, out List<Element> hit, out List<ShotTrajectory> trajectories)
        {
            // init
            hit = new List<Element>();
            trajectories = new List<ShotTrajectory>();

            // provide a trajectory that takes into account the players yaw and pitch
            var x1 = 0f;
            var y1 = 0f;
            var z1 = -1 * player.Depth;
            Utilities3D.Yaw(360f - player.Angle, ref x1, ref y1, ref z1);
            // todo - apply pitch
            //Utilities3D.Pitch(player.PitchAngle, ref x1, ref y1, ref z1);

            var x2 = 0f;
            var y2 = 0f;
            var z2 = -1 * player.Depth * 4;
            Utilities3D.Yaw(360f - player.Angle, ref x2, ref y2, ref z2);
            // todo - apply pitch
            //Utilities3D.Pitch(player.PitchAngle, ref x2, ref y2, ref z2);

            // add projectile
            trajectories.Add(new ShotTrajectory3D(x: player.X + x1, y: player.Y + y1, z: player.Z + z1)
            { 
                X1 = player.X + x1,
                Y1 = player.Y + y1,
                Z1 = player.Z + z1,
                X2 = player.X + x2,
                Y2 = player.Y + y2,
                Z2 = player.Z + z2,
            });

            return true;
        }
        #endregion
    }
}
