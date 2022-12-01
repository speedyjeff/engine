using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public static class Utilities3D
    {
        static Utilities3D()
        {
            // populate the cos/sin caches (reduces the cost of recalcuating the values over and over again)
            CosCache = new float[361];
            SinCache = new float[361];
            for (int angle=0; angle<361; angle++)
            {
                var radians = angle * (Math.PI / 180);
                CosCache[angle] = (float)Math.Cos(radians);
                SinCache[angle] = (float)Math.Sin(radians);
            }
        }

        // scale and apply perspective
        // assumes that the coordinates are normalized with z increasing into the screen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Perspective(float maxZ, ref float x, ref float y, ref float z)
        {
            // ratio
            var ratio = (-1 * z) / maxZ;

            // delta for aspect ratio
            var dx = Math.Abs(x) * ratio;
            var dy = Math.Abs(y) * ratio;

            if (x < 0) x += dx;
            else x -= dx;

            if (y < 0) y += dy;
            else y -= dy;

            return ratio;
        }

        // pitch (head tip) - x axis
        // assumes coordinates are origin normalized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pitch(float angle, ref float x, ref float y, ref float z)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix

            // rotate
            var cosa = CosCache[(int)angle];
            var sina = SinCache[(int)angle];

            var nx = (1 * x) + (0 * y) + (0 * z);
            var ny = (0 * x) + (cosa * y) - (sina * z);
            var nz = (0 * x) + (sina * y) + (cosa * z);

            x = nx;
            y = ny;
            z = nz;
        }

        // yaw (turn) - y-axis
        // assumes coordinates are origin normalized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Yaw(float angle, ref float x, ref float y, ref float z)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix

            // rotate
            var cosa = CosCache[(int)angle];
            var sina = SinCache[(int)angle];

            var nx = (cosa * x) + (0 * y) + (sina * z);
            var ny = (0 * x) + (1 * y) + (0 * z);
            var nz = (sina * x * -1) + (0 * y) + (cosa * z);

            x = nx;
            y = ny;
            z = nz;
        }

        // roll (twist) - z-axis
        // assumes coordinates are origin normalized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Roll(float angle, ref float x, ref float y, ref float z)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix

            var cosa = CosCache[(int)angle];
            var sina = SinCache[(int)angle];

            var nx = (cosa * x) - (sina * y) + (0 * z);
            var ny = (sina * x) + (cosa * y) + (0 * z);
            var nz = (0 * 1) + (0 * y) + (1 * z);

            x = nx;
            y = ny;
            z = nz;
        }

        public static bool IntersectingTriangles(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2)
        {
            // https://gamedev.stackexchange.com/questions/88060/triangle-triangle-intersection-code

            // Compute distance signs  of p1, q1 and r1 
            // to the plane of triangle(p2,q2,r2)
            var n2 = Normal(r2, p2, q2);
            var v1 = Point.Subtract(p1, r2);
            var dp1 = Point.Dot(v1, n2);
            
            v1 = Point.Subtract(q1, r2);
            var dq1 = Point.Dot(v1, n2);

            v1 = Point.Subtract(r1, r2);
            var dr1 = Point.Dot(v1, n2);

            if (((dp1 * dq1) > 0.0f) && ((dp1 * dr1) > 0.0f)) return false;

            // Compute distance signs  of p2, q2 and r2 
            // to the plane of triangle(p1,q1,r1)

            var n1 = Normal(p1, q1, r1);

            v1 = Point.Subtract(p2, r1);
            var dp2 = Point.Dot(v1, n1);

            v1 = Point.Subtract(q2, r1);
            var dq2 = Point.Dot(v1, n1);

            v1 = Point.Subtract(r2, r1);
            var dr2 = Point.Dot(v1, n1);

            if (((dp2 * dq2) > 0.0f) && ((dp2 * dr2) > 0.0f)) return false;

            // Permutation in a canonical form of T1's vertices
            if (dp1 > 0.0f)
            {
                if (dq1 > 0.0f) return IsTriangleIntersection(r1, p1, q1, p2, r2, q2, dp2, dr2, dq2, n1, n2);
                else if (dr1 > 0.0f) return IsTriangleIntersection(q1, r1, p1, p2, r2, q2, dp2, dr2, dq2, n1, n2);

                return IsTriangleIntersection(p1, q1, r1, p2, q2, r2, dp2, dq2, dr2, n1, n2);
            }
            else if (dp1 < 0.0f)
            {
                if (dq1 < 0.0f) return IsTriangleIntersection(r1, p1, q1, p2, q2, r2, dp2, dq2, dr2, n1, n2);
                else if (dr1 < 0.0f) return IsTriangleIntersection(q1, r1, p1, p2, q2, r2, dp2, dq2, dr2, n1, n2);

                return IsTriangleIntersection(p1, q1, r1, p2, r2, q2, dp2, dr2, dq2, n1, n2);
            }
            else if (dq1 < 0.0f)
            {
                if (dr1 >= 0.0f) return IsTriangleIntersection(q1, r1, p1, p2, r2, q2, dp2, dr2, dq2, n1, n2);

                return IsTriangleIntersection(p1, q1, r1, p2, q2, r2, dp2, dq2, dr2, n1, n2);
            }
            else if (dq1 > 0.0f)
            {
                if (dr1 > 0.0f) return IsTriangleIntersection(p1, q1, r1, p2, r2, q2, dp2, dr2, dq2, n1, n2);
                else return IsTriangleIntersection(q1, r1, p1, p2, q2, r2, dp2, dq2, dr2, n1, n2);
            }
            else if (dr1 > 0.0f) return IsTriangleIntersection(r1, p1, q1, p2, q2, r2, dp2, dq2, dr2, n1, n2);
            else if (dr1 < 0.0f) return IsTriangleIntersection(r1, p1, q1, p2, r2, q2, dp2, dr2, dq2, n1, n2);

            // triangles are co-planar
            return IsCoplanarTriangle(p1, q1, r1, p2, q2, r2, n1, n2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point Normal(Point a, Point b, Point c)
        {
            // https://sciencing.com/plane-3-points-8123924.html
            // AB = (b - a)
            var ab = new Point() { X = b.X - a.X, Y = b.Y - a.Y, Z = b.Z - a.Z };

            // AC = (c - a)
            var ac = new Point() { X = c.X - a.X, Y = c.Y - a.Y, Z = c.Z - a.Z };

            // N = AB x AC
            return Point.Product(ab, ac);
        }

        #region private

        private static float[] CosCache;
        private static float[] SinCache;

        #region triangles
        // https://gamedev.stackexchange.com/questions/88060/triangle-triangle-intersection-code
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTriangleIntersection(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2, float dp2, float dq2, float dr2, Point n1, Point n2)
        {
            if (dp2 > 0.0f)
            {
                if (dq2 > 0.0f) return HasTriangleIntersection(p1, r1, q1, r2, p2, q2);
                else if (dr2 > 0.0f) return HasTriangleIntersection(p1, r1, q1, q2, r2, p2);

                return HasTriangleIntersection(p1, q1, r1, p2, q2, r2);
            }
            else if (dp2 < 0.0f)
            {
                if (dq2 < 0.0f) return HasTriangleIntersection(p1, q1, r1, r2, p2, q2);
                else if (dr2 < 0.0f) return HasTriangleIntersection(p1, q1, r1, q2, r2, p2);

                return HasTriangleIntersection(p1, r1, q1, p2, q2, r2);
            }
            else if (dq2 < 0.0f)
            {
                if (dr2 >= 0.0f) return HasTriangleIntersection(p1, r1, q1, q2, r2, p2);

                return HasTriangleIntersection(p1, q1, r1, p2, q2, r2);
            }
            else if (dq2 > 0.0f)
            {
                if (dr2 > 0.0f) return HasTriangleIntersection(p1, r1, q1, p2, q2, r2);

                return HasTriangleIntersection(p1, q1, r1, q2, r2, p2);
            }
            else if (dr2 > 0.0f) return HasTriangleIntersection(p1, q1, r1, r2, p2, q2);
            else if (dr2 < 0.0f) return HasTriangleIntersection(p1, r1, q1, r2, p2, q2);

            // triangles are co-planar
            return IsCoplanarTriangle(p1, q1, r1, p2, q2, r2, n1, n2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasTriangleIntersection(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2)
        {
            var n = Normal(p1, q1, r2);
            var v = Point.Subtract(p2, p1);

            if (Point.Dot(v, n) > 0.0f)
            {
                n = Normal(p1, r1, r2);
                if (Point.Dot(v, n) <= 0.0f) return true;

                return false;
            }

            n = Normal(p1, q1, q2);
            if (Point.Dot(v, n) < 0.0f) return false;
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCoplanarTriangle(Point p1, Point q1, Point r1,
                       Point p2, Point q2, Point r2,
                       Point normal_1, Point normal_2)
        {
            var n_x = (normal_1.X < 0) ? (-1 * normal_1.X) : normal_1.X;
            var n_y = (normal_1.Y < 0) ? (-1 * normal_1.Y) : normal_1.Y;
            var n_z = (normal_1.Z < 0) ? (-1 * normal_1.Z) : normal_1.Z;

            // Projection of the triangles in 3D onto 2D such that the area of
            // the projection is maximized.
            var P1 = new Point();
            var Q1 = new Point();
            var R1 = new Point();
            var P2 = new Point();
            var Q2 = new Point();
            var R2 = new Point();

            if ((n_x > n_z) && (n_x >= n_y))
            {
                // Project onto plane YZ
                P1.X = q1.Z; P1.Y = q1.Y;
                Q1.X = p1.Z; Q1.Y = p1.Y;
                R1.X = r1.Z; R1.Y = r1.Y;

                P2.X = q2.Z; P2.Y = q2.Y;
                Q2.X = p2.Z; Q2.Y = p2.Y;
                R2.X = r2.Z; R2.Y = r2.Y;
            }
            else if ((n_y > n_z) && (n_y >= n_x))
            {
                // Project onto plane XZ
                P1.X = q1.X; P1.Y = q1.Z;
                Q1.X = p1.X; Q1.Y = p1.Z;
                R1.X = r1.X; R1.Y = r1.Z;

                P2.X = q2.X; P2.Y = q2.Z;
                Q2.X = p2.X; Q2.Y = p2.Z;
                R2.X = r2.X; R2.Y = r2.Z;
            }
            else
            {
                // Project onto plane XY
                P1.X = p1.X; P1.Y = p1.Y;
                Q1.X = q1.X; Q1.Y = q1.Y;
                R1.X = r1.X; R1.Y = r1.Y;

                P2.X = p2.X; P2.Y = p2.Y;
                Q2.X = q2.X; Q2.Y = q2.Y;
                R2.X = r2.X; R2.Y = r2.Y;
            }

            return Collision.IntersectingTriangles(P1, Q1, R1, P2, Q2, R2);
        }
        #endregion

        #endregion
    }
}
