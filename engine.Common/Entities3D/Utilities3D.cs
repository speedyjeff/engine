using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace engine.Common.Entities3D
{
    public static class Utilities3D
    {
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
            var radians = angle * (Math.PI / 180);

            var cosa = (float)Math.Cos(radians);
            var sina = (float)Math.Sin(radians);

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
            var radians = angle * (Math.PI / 180);

            var cosa = (float)Math.Cos(radians);
            var sina = (float)Math.Sin(radians);

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

            // rotate
            var radians = angle * (Math.PI / 180);

            var cosa = (float)Math.Cos(radians);
            var sina = (float)Math.Sin(radians);

            var nx = (cosa * x) - (sina * y) + (0 * z);
            var ny = (sina * x) + (cosa * y) + (0 * z);
            var nz = (0 * 1) + (0 * y) + (1 * z);

            x = nx;
            y = ny;
            z = nz;
        }
    }
}
