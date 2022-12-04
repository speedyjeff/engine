using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using engine.Common.Entities;

namespace engine.Common
{
    struct Region
    {
        public Region(int row, int col, int layer)
        {
            Layer = layer;
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            if (obj is Region)
            {
                var otherRegion = (Region)obj;
                return Layer == otherRegion.Layer
                    && Row == otherRegion.Row
                    && Col == otherRegion.Col;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region private
        internal int Row;
        internal int Col;
        internal int Layer;
        #endregion
    }

    // this collection will provide fast access to a subset of the elements
    // the subset is defined by a region

    class RegionCollection
    {
        public RegionCollection(IEnumerable<Element> elements, int width, int height, int depth)
        {
            // gather all the element sizes and the lower bounds for x, y, and z
            var sizes = new List<float>();
            var minx = Single.MaxValue;
            var miny = Single.MaxValue;
            var minz = Single.MaxValue;
            foreach (var o in elements)
            {
                var max = Math.Max(o.Width, Math.Max(o.Height, o.Depth));
                sizes.Add(max);
                if (o.X < minx) minx = o.X;
                if (o.Y < miny) miny = o.Y;
                if (o.Z < minz) minz = o.Z;
            }

            // get the regionSize
            var maxRegionSize = Math.Max(width, Math.Max(height, depth));
            if (sizes.Count == 0)
            {
                // we hit a corner case
                // set mins to 0
                minx = miny = minz = 0;
                RegionSize = maxRegionSize;
            }
            else
            {
                // get the 80th percentile
                sizes.Sort();
                RegionSize = (int)sizes[(int)(sizes.Count * 0.8)];
            }

            // check if there is only one region and do an artifical split (worst case is that everything ends up in oversized - eg. 1 region)
            if (RegionSize == maxRegionSize)
            {
                // split the RegionSize into smaller pieces - a default player is 50x50
                // there are two possible strategies
                //  1. absolute split (400x400x400 regions)
                //  2. equal split (10%)
                // the combined strategy is to split into at least by 10%, but not smaller than 2x a default player
                var splitRegionSize = RegionSize / 10;
                if (splitRegionSize < (50*2) || maxRegionSize > 400) RegionSize = 400;
                else if (splitRegionSize > 50) RegionSize = splitRegionSize;
                // else make one region
            }

            // init
            Columns = (width / RegionSize) + 1;
            Rows = (height / RegionSize) + 1;
            Layers = (depth / RegionSize) + 1;
            Width = width;
            Height = height;
            Depth = depth;
            Oversized = new Dictionary<int, Element>();
            Regions = new Dictionary<int, Element>[Layers][][];
            for (int l = 0; l < Layers; l++)
            {
                Regions[l] = new Dictionary<int, Element>[Rows][];
                for (int r = 0; r < Rows; r++)
                {
                    Regions[l][r] = new Dictionary<int, Element>[Columns];
                    for (int c = 0; c < Columns; c++)
                    {
                        Regions[l][r][c] = new Dictionary<int, Element>();
                    }
                }
            }

            // init locks
            RegionColumnLocks = new ReaderWriterLockSlim[Rows][];
            for (int r = 0; r < Rows; r++)
            {
                RegionColumnLocks[r] = new ReaderWriterLockSlim[Columns];
                for (int c = 0; c < Columns; c++)
                {
                    RegionColumnLocks[r][c] = new ReaderWriterLockSlim();
                }
            }
            OversizedLock = new ReaderWriterLockSlim();

            // convert these into offsets to adjust the regions to a 0,0,0 based matrix
            if (minx < 0) XOffset = (float)Math.Floor(minx);
            else XOffset = (float)Math.Ceiling(minx);
            XOffset *= -1f;
            if (miny < 0) YOffset = (float)Math.Floor(miny);
            else YOffset = (float)Math.Ceiling(miny);
            YOffset *= -1f;
            if (minz < 0) ZOffset = -1 * (float)Math.Floor(minz);
            else ZOffset = 0;

            // add all the elements
            foreach (var elem in elements) Add(elem.Id, elem);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public void Add(int key, Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to add");

            // get region to insert into
            GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);

            // check if this is an item that would span multiple regions
            if (IsOversized(elem) || IsOutofRange(row, column, layer))
            {
                try
                {
                    OversizedLock.EnterWriteLock();

                    Oversized.Add(key, elem);
                    return;
                }
                finally
                {
                    OversizedLock.ExitWriteLock();
                }
            }

            try
            {
                RegionColumnLocks[row][column].EnterWriteLock();

                // add to the region specified (or span multiple regions)
                Regions[layer][row][column].Add(key, elem);
            }
            finally
            {
                RegionColumnLocks[row][column].ExitWriteLock();
            }
        }

        public bool Remove(int key, Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to remove");

            // get region to remove from
            GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);

            // check if this is an item that would span multiple regions
            if (IsOversized(elem) || IsOutofRange(row, column, layer))
            {
                try
                {
                    OversizedLock.EnterWriteLock();

                    return Oversized.Remove(key);
                }
                finally
                {
                    OversizedLock.ExitWriteLock();
                }
            }

            try
            {
                RegionColumnLocks[row][column].EnterWriteLock();

                // add to the region specified (or span multiple regions)
                return Regions[layer][row][column].Remove(key);
            }
            finally
            {
                RegionColumnLocks[row][column].ExitWriteLock();
            }
        }

        public void Move(int key, Region src, Region dst)
        {
            Element elem;
            var isInOversized = false;

            // get element if in Oversized
            try
            {
                OversizedLock.EnterReadLock();

                isInOversized = Oversized.TryGetValue(key, out elem);
            }
            finally
            {
                OversizedLock.ExitReadLock();
            }

            // check where it should go
            if (isInOversized)
            {
                // if elment is Oversized or the dst is out of range, keep it here
                if (IsOversized(elem)) return;

                // assert that this element is currently out of range
                if (!IsOutofRange(src)) throw new Exception("Invalid state in internal datastructures");

                // if it remains out of range, keep it here
                if (IsOutofRange(dst)) return;

                // remove it from the oversized, as it is moving to a region
                try
                {
                    OversizedLock.EnterWriteLock();

                    if (!Oversized.Remove(key)) throw new Exception("Failed to remove this element");
                }
                finally
                {
                    OversizedLock.ExitWriteLock();
                }
            }
            else
            {
                // check if the src and dst are the same, and dst is not out of range, keep it here
                if (src.Equals(dst) && !IsOutofRange(dst)) return;

                // find it and remove it
                try
                {
                    RegionColumnLocks[src.Row][src.Col].EnterWriteLock();

                    if (!Regions[src.Layer][src.Row][src.Col].TryGetValue(key, out elem)) throw new Exception("Failed to find the element to move");
                    if (!Regions[src.Layer][src.Row][src.Col].Remove(key)) throw new Exception("Failed to remove this element");
                }
                finally
                {
                    RegionColumnLocks[src.Row][src.Col].ExitWriteLock();
                }
            }

            // add element (either oversized or a Region)
            if (IsOutofRange(dst))
            {
                try
                {
                    OversizedLock.EnterWriteLock();

                    Oversized.Add(key, elem);
                }
                finally
                {
                    OversizedLock.ExitWriteLock();
                }
            }
            else
            {
                try
                {
                    RegionColumnLocks[dst.Row][dst.Col].EnterWriteLock();

                    Regions[dst.Layer][dst.Row][dst.Col].Add(key, elem);
                }
                finally
                {
                    RegionColumnLocks[dst.Row][dst.Col].ExitWriteLock();
                }
            }
        }

        public IEnumerable<Element> Values(float x1, float y1, float z1, float x2, float y2, float z2, bool expandRegion = true)
        {
            // get the starting row,col and ending row,col
            GetRegion(x1, y1, z1, out int r1, out int c1, out int l1);
            GetRegion(x2, y2, z2, out int r2, out int c2, out int l2);

            // get the min and max values
            var minl = Math.Min(l1, l2);
            var maxl = Math.Max(l1, l2);
            var minr = Math.Min(r1, r2);
            var maxr = Math.Max(r1, r2);
            var minc = Math.Min(c1, c2);
            var maxc = Math.Max(c1, c2);

            // expand the region
            if (expandRegion)
            {
                minr -= 1; minc -= 1; minl -= 1;
                maxr += 1; maxc += 1; maxl += 1;
            }

            // hold the lock for the duration of this call
            for (int l = (minl >= 0) ? minl : 0; l <= maxl && l < Regions.Length; l++)
            {
                for (int r = (minr >= 0 ? minr : 0); r <= maxr && r < Regions[l].Length; r++)
                {
                    for (int c = (minc >= 0 ? minc : 0); c <= maxc && c < Regions[l][r].Length; c++)
                    {
                        try
                        {
                            RegionColumnLocks[r][c].EnterReadLock();

                            if (Regions[l][r][c].Count == 0) continue;
                            foreach (var elem in Regions[l][r][c].Values) yield return elem;
                        }
                        finally
                        {
                            RegionColumnLocks[r][c].ExitReadLock();
                        }
                    }
                }
            }

            // always return the oversized items
            try
            {
                OversizedLock.EnterReadLock();

                foreach (var elem in Oversized.Values) yield return elem;
            }
            finally
            {
                OversizedLock.ExitReadLock();
            }
        }

        public Region GetRegion(Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to get region");

            if (IsOversized(elem))
            {
                return new Region(-1, -1, -1);
            }

            GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);
            return new Region(row, column, layer);
        }

        #region private
        private ReaderWriterLockSlim[][] RegionColumnLocks;
        private Dictionary<int, Element>[][][] Regions;
        private Dictionary<int, Element> Oversized;
        private ReaderWriterLockSlim OversizedLock;
        private int RegionSize;
        private int Columns;
        private int Rows;
        private int Layers;
        private float XOffset;
        private float YOffset;
        private float ZOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOversized(Element elem)
        {
            return elem.Width > RegionSize || elem.Height > RegionSize || elem.Depth > RegionSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetRegion(float x, float y, float z, out int row, out int column, out int layer)
        {
            // round to avoid near boundary misses
            x = (float)Math.Round(x, digits: 0);
            y = (float)Math.Round(y, digits: 0);
            z = (float)Math.Round(z, digits: 0);

            // get region
            row = (int)Math.Floor((y + YOffset) / (float)RegionSize);
            column = (int)Math.Floor((x + XOffset) / (float)RegionSize);
            layer = (int)Math.Floor((z + ZOffset) / (float)RegionSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsOutofRange(float row, float col, float layer)
        {
            return row < 0 || row >= Rows 
                || col < 0 || col >= Columns
                || layer < 0 || layer >= Layers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsOutofRange(Region region)
        {
            return region.Row < 0 || region.Row >= Rows 
                || region.Col < 0 || region.Col >= Columns
                || region.Layer < 0 || region.Layer >= Layers;
        }
        #endregion
    }
}
