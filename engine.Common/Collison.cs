using System;
using System.Runtime.CompilerServices;

namespace engine.Common
{
    // x,y coordinates have y inversed
    //                (-y, 0)
    //                   |
    // (-x,0) ----------------------- (+x, 0)
    //                   |
    //                (+y, 0)

    public static class Collision
    {
        public static bool IntersectingRectangles(
            float x1, float y1, float x2, float y2,
            float x3, float y3, float x4, float y4)
        {
            // validate input
            if (x1 > x2
                || y1 > y2
                || x3 > x4
                || y3 > y4) throw new Exception("Invalid input to compare rectangles");

            if (x3 > x1 && x3 < x2)
            {
                if (y3 >= y1 && y3 < y2) return true;
                if (y3 <= y1 && y4 >= y2) return true;
                if (y4 >= y1 && y4 < y2) return true;
            }
            else if (x4 > x1 && x4 < x2)
            {
                if (y4 >= y1 && y4 < y2) return true;
                if (y3 <= y1 && y4 >= y2) return true;
                if (y3 >= y1 && y3 < y2) return true;
            }
            else if ((y3 > y1 && y3 < y2) || (y4 > y1 && y4 < y2))
            {
                if (x3 <= x1 && x4 >= x2) return true;
            }
            else if (y3 <= y1 && x3 <= x1 && y4 >= y2 && x4 >= x2) return true;

            return false;
        }

        public static float DistanceToObject(
            float x1, float y1, float width1, float height1,
            float x2, float y2, float width2, float height2)
        {
            // this is an approximation, consider the shortest distance between any two points in these objects
            var e1 = new Tuple<float, float>[]
            {
                new Tuple<float,float>(x1, y1),
                new Tuple<float,float>(x1 - (width1 / 2), y1 - (height1 / 2)),
                new Tuple<float,float>(x1 + (width1 / 2), y1 + (height1 / 2))
            };

            var e2 = new Tuple<float, float>[]
            {
                new Tuple<float,float>(x2, y2),
                new Tuple<float,float>(x2 - (width2 / 2), y2 - (height2 / 2)),
                new Tuple<float,float>(x2 + (width2 / 2), y2 + (height2 / 2))
            };

            var minDistance = float.MaxValue;
            for (int i = 0; i < e1.Length; i++)
                for (int j = i + 1; j < e2.Length; j++)
                    minDistance = Math.Min(Collision.DistanceBetweenPoints(e1[i].Item1, e1[i].Item2, e2[j].Item1, e2[j].Item2), minDistance);
            return minDistance;
        }

        public static float CalculateAngleFromPoint(float x1, float y1, float x2, float y2)
        {
            // normalize x,y to 0,0
            float y = -1 * (y2 - y1); // inverting Y to make Atan correct
            float x = (x2 - x1);
            float angle = (float)(Math.Atan2(x, y) * (180 / Math.PI));

            if (angle < 0) angle += 360;
            if (angle < 0) throw new Exception("Invalid negative angle : " + angle);
            if (angle > 360) throw new Exception("Invalid angle larger than 360 : " + angle);

            return angle;
        }
        
        public static bool CalculateLineByAngle(float x, float y, float angle, float distance, out float x1, out float y1, out float x2, out float y2)
        {
            while (angle < 0) angle += 360;
            while (angle > 360) angle -= 360;

            x1 = x;
            y1 = y;
            float a = (float)Math.Cos(angle * Math.PI / 180) * distance;
            float o = (float)Math.Sin(angle * Math.PI / 180) * distance;
            x2 = x1 + o;
            y2 = y1 - a;

            return true;
        }

        public static float DistanceBetweenPoints(float x1, float y1, float x2, float y2)
        {
            // a^2 + b^2 = c^2
            //  a = |x1 - x2|
            //  b = |y1 - y2|
            //  c = result
            return (float)Math.Sqrt(
                Math.Pow(Math.Abs(x1-x2), 2) + Math.Pow(Math.Abs(y1-y2), 2)
                );
        }

        public static float LineIntersectingRectangle(
            float x1, float y1, float x2, float y2, // line
            float x, float y, float width, float height // rectangle
            )
        {
            float distance = Single.MaxValue;

            // return the minimum point of intersection
            var found = false;
            var lines = new Tuple<float, float, float, float>[]
            {
                new Tuple<float,float,float,float>(x - (width/2f), y - (height/2f), x + (width/2), y - (height/2f)) ,
                new Tuple<float,float,float,float>(x - (width/2f), y - (height/2f), x - (width/2), y + (height/2f)) ,
                new Tuple<float,float,float,float>(x + (width/2f), y + (height/2f), x + (width/2), y - (height/2f)) ,
                new Tuple<float,float,float,float>(x + (width/2f), y + (height/2f), x - (width/2), y + (height/2f)) ,
            };
            foreach(var line in lines)
            {
                if (PointOfLineSegmentIntersection(x1, y1, x2, y2, line.Item1, line.Item2, line.Item3, line.Item4, out float tix, out float tiy))
                {
                    // check if this point is closer
                    var dist = DistanceBetweenPoints(x1, y1, tix, tiy);
                    if (dist < distance)
                    {
                        distance = dist;
                        found = true;
                    }
                }
            }

            return found ? distance : 0f;
        }

        public static bool IntersectingLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            // https://stackoverflow.com/questions/3838329/how-can-i-check-if-two-segments-intersect
            if (CalcCcw(x1, y1, x3, y3, x4, y4) != CalcCcw(x2, y2, x3, y3, x4, y4)
                && CalcCcw(x1, y1, x2, y2, x3, y3) != CalcCcw(x1, y1, x2, y2, x4, y4))
                return true;
            return false;
        }

        public static bool IntersectingTriangles(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2)
        {
            // https://gamedev.stackexchange.com/questions/88060/triangle-triangle-intersection-code
            if (OrientTriangle(p1, q1, r1) < 0.0f)
            {
                if (OrientTriangle(p2, q2, r2) < 0.0f) return IsTriangleIntersection(p1, r1, q1, p2, r2, q2);

                return IsTriangleIntersection(p1, r1, q1, p2, q2, r2);
            }

            if (OrientTriangle(p2, q2, r2) < 0.0f) return IsTriangleIntersection(p1, q1, r1, p2, r2, q2);

            return IsTriangleIntersection(p1, q1, r1, p2, q2, r2);
        }

#region private

#region lines
        // https://stackoverflow.com/questions/3838329/how-can-i-check-if-two-segments-intersect
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CalcCcw(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return (y3 - y1) * (x2 - x1) > (y2 - y1) * (x3 - x1);
        }

        private static bool PointOfLineSegmentIntersection(
            float p1x, float p1y, float p2x, float p2y,
            float q1x, float q1y, float q2x, float q2y,
            out float ix, out float iy)
        {
            // initially set these to 0
            ix = iy = 0f;

            // https://www.codeproject.com/tips/862988/find-the-intersection-point-of-two-line-segments
            // https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect/565282#565282
            var epsilon = 0.00000001;

            //var r = p2 - p;
            var rx = p2x - p1x;
            var ry = p2y - p1y;

            //var s = q2 - q;
            var sx = q2x - q1x;
            var sy = q2y - q1y;

            //var t = q - p;
            var tx = q1x - p1x;
            var ty = q1y - p1y;

            //var rxs = r.Cross(s);
            var rxs = (rx * sy) - (ry * sx);

            //var qpxr = (q - p).Cross(r);
            var qpxr = (tx * ry) - (ty * rx);

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (Math.Abs(rxs) < epsilon && Math.Abs(qpxr) < epsilon)
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                //if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s)) return true;
                // TODO enable the coplanar example
                //var u = p - q
                //var ux = p1x - q1x;
                //var uy = p1y - q1y;
                //var r2 = r * r
                //var r2 = (rx * rx) + (ry * ry);
                //var s2 = s * s
                //var s2 = (sx * sx) + (sy * sy);
                //if ((0 <= ((tx * rx) + (ty * ry)) && ((tx * rx) + (tx * ry)) <= r2) || (0 <= ((ux * sx) + (uy * sy)) && ((ux * sx) + (uy * sy)) <= s2)) return true;

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (Math.Abs(rxs) < epsilon && Math.Abs(qpxr) > epsilon) return false;

            //var qpxr = (q - p).Cross(r);
            var qpxs = (tx * sy) - (ty * sx);

            //var t = (q - p).Cross(s) / rxs;
            var t = qpxs / rxs;

            //var u = (q - p).Cross(r) / rxs;
            var u = qpxr / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (Math.Abs(rxs) > epsilon && (0f <= t && t <= 1f) && (0f <= u && u <= 1f))
            {
                // We can calculate the intersection point using either t or u.
                //intersection = p + t * r;
                ix = p1x + (t * rx);
                iy = p1y + (t * ry);

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }
        #endregion

        #region triangles
        // https://gamedev.stackexchange.com/questions/88060/triangle-triangle-intersection-code
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OrientTriangle(Point a, Point b, Point c)
        {
            return ((a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTriangleIntersection(Point p1, Point q1, Point r1,
                        Point p2, Point q2, Point r2)
        {
            if (OrientTriangle(p2, q2, p1) >= 0.0f)
            {
                if (OrientTriangle(q2, r2, p1) >= 0.0f)
                {
                    if (OrientTriangle(r2, p2, p1) >= 0.0f) return true;

                    return IsTriangleEdgeIntersection(p1, q1, r1, p2, q2, r2);
                }

                if (OrientTriangle(r2, p2, p1) >= 0.0f) return IsTriangleEdgeIntersection(p1, q1, r1, r2, p2, q2);

                return IsTriangleVertexIntersection(p1, q1, r1, p2, q2, r2);
            }
            else if (OrientTriangle(q2, r2, p1) >= 0.0f)
            {
                if (OrientTriangle(r2, p2, p1) >= 0.0f) return IsTriangleEdgeIntersection(p1, q1, r1, q2, r2, p2);

                return IsTriangleVertexIntersection(p1, q1, r1, q2, r2, p2);
            }

            return IsTriangleVertexIntersection(p1, q1, r1, r2, p2, q2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTriangleVertexIntersection(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2)
        {
            if (OrientTriangle(r2, p2, q1) >= 0.0f)
            {
                if (OrientTriangle(r2, q2, q1) <= 0.0f)
                {
                    if (OrientTriangle(p1, p2, q1) > 0.0f)
                    {
                        if (OrientTriangle(p1, q2, q1) <= 0.0f) return true;
                    }
                    else if (OrientTriangle(p1, p2, r1) >= 0.0f)
                    {
                        if (OrientTriangle(q1, r1, p2) >= 0.0f) return true;
                    }
                }
                else
                {
                    if (OrientTriangle(p1, q2, q1) <= 0.0f)
                    {
                        if (OrientTriangle(r2, q2, r1) <= 0.0f)
                        {
                            if (OrientTriangle(q1, r1, q2) >= 0.0f) return true;
                        }
                    }
                }
            }
            else if (OrientTriangle(r2, p2, r1) >= 0.0f)
            {
                if (OrientTriangle(q1, r1, r2) >= 0.0f)
                {
                    if (OrientTriangle(p1, p2, r1) >= 0.0f) return true;
                }
                else if (OrientTriangle(q1, r1, q2) >= 0.0f)
                {
                    if (OrientTriangle(r2, r1, q2) >= 0.0f) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTriangleEdgeIntersection(Point p1, Point q1, Point r1, Point p2, Point q2, Point r2)
        {
            if (OrientTriangle(r2, p2, q1) >= 0.0f)
            {
                if (OrientTriangle(p1, p2, q1) >= 0.0f)
                {
                    if (OrientTriangle(p1, q1, r2) >= 0.0f) return true;
                }
                else if (OrientTriangle(q1, r1, p2) >= 0.0f)
                {
                    if (OrientTriangle(r1, p1, p2) >= 0.0f) return true;
                }
            }
            else if (OrientTriangle(r2, p2, r1) >= 0.0f)
            {
                if (OrientTriangle(p1, p2, r1) >= 0.0f)
                {
                    if (OrientTriangle(p1, r1, r2) >= 0.0f) return true;
                    else if (OrientTriangle(q1, r1, r2) >= 0.0f) return true;
                }
            }

            return false;
        }
#endregion

#endregion
    }
}
