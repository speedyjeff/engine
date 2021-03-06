﻿using System;
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
            if (sizes.Count == 0)
            {
                // setup only 1 region
                RegionSize = Math.Max(width, Math.Max(height, depth));
            }
            else
            {
                // get the 80th percentile
                sizes.Sort();
                RegionSize = (int)sizes[(int)(sizes.Count * 0.8)];
            }

            // init
            RegionLock = new ReaderWriterLockSlim();
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

            // convert these into offsets to adjust the regions to a 0,0,0 based matrix
            if (minx < 0) XOffset = (int)Math.Floor(minx);
            else XOffset = (int)Math.Ceiling(minx);
            XOffset *= -1;
            if (miny < 0) YOffset = (int)Math.Floor(miny);
            else YOffset = (int)Math.Ceiling(miny);
            YOffset *= -1;
            if (minz < 0) ZOffset = (int)Math.Floor(minz);
            else ZOffset = (int)Math.Ceiling(minz);
            ZOffset *= -1;

            // add all the elements
            foreach (var elem in elements) Add(elem.Id, elem);
        }

        public void Add(int key, Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to add");

            try
            {
                RegionLock.EnterWriteLock();

                // get region to insert into
                GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);

                // check if this is an item that would span multiple regions
                if (IsOversized(elem) || IsOutofRange(row, column, layer))
                {
                    Oversized.Add(key, elem);
                    return;
                }

                // add to the region specified (or span multiple regions)
                Regions[layer][row][column].Add(key, elem);
            }
            finally
            {
                RegionLock.ExitWriteLock();
            }
        }

        public bool Remove(int key, Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to remove");

            try
            {
                RegionLock.EnterWriteLock();

                // get region to remove from
                GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);

                // check if this is an item that would span multiple regions
                if (IsOversized(elem) || IsOutofRange(row, column, layer))
                {
                    return Oversized.Remove(key);
                }

                // add to the region specified (or span multiple regions)
                return Regions[layer][row][column].Remove(key);
            }
            finally
            {
                RegionLock.ExitWriteLock();
            }
        }

        public void Move(int key, Region src, Region dst)
        {
            try
            {
                RegionLock.EnterWriteLock();

                // get element (either in oversized or a Region)
                Element elem;
                if (Oversized.TryGetValue(key, out elem))
                {
                    // if elment is Oversized or the dst is out of range, keep it here
                    if (IsOversized(elem)) return;

                    // assert that this element is currently out of range
                    if (!IsOutofRange(src)) throw new Exception("Invalid state in internal datastructures");

                    // if it remains out of range, keep it here
                    if (IsOutofRange(dst)) return;

                    // remove it from the oversized, as it is moving to a region
                    if (!Oversized.Remove(key)) throw new Exception("Failed to remove this element");
                }
                else
                {
                    // find it and remove it
                    if (!Regions[src.Layer][src.Row][src.Col].TryGetValue(key, out elem)) throw new Exception("Failed to location the element to move");
                    if (!Regions[src.Layer][src.Row][src.Col].Remove(key)) throw new Exception("Failed to remove this element");
                }

                // add element (either oversized or a Region)
                if (IsOutofRange(dst)) Oversized.Add(key, elem);
                else Regions[dst.Layer][dst.Row][dst.Col].Add(key, elem);
            }
            finally
            {
                RegionLock.ExitWriteLock();
            }
        }

        public IEnumerable<Element> Values(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            try
            {
                RegionLock.EnterReadLock();

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
                minr -= 1; minc -= 1; minl -= 1;
                maxr += 1; maxc += 1; maxl += 1;

                // hold the lock for the duration of this call
                for (int l = (minl >= 0) ? minl : 0; l <= maxl && l < Regions.Length; l++)
                {
                    for (int r = (minr >= 0 ? minr : 0); r <= maxr && r < Regions[l].Length; r++)
                    {
                        for (int c = (minc >= 0 ? minc : 0); c <= maxc && c < Regions[l][r].Length; c++)
                        {
                            if (Regions[l][r][c].Count == 0) continue;
                            foreach (var elem in Regions[l][r][c].Values) yield return elem;
                        }
                    }
                }

                // always return the oversized items
                foreach (var elem in Oversized.Values) yield return elem;
            }
            finally
            {
                RegionLock.ExitReadLock();
            }
        }

        public Region GetRegion(Element elem)
        {
            if (elem == null) throw new Exception("Invalid element to get region");

            try
            {
                RegionLock.EnterReadLock();

                if (IsOversized(elem))
                {
                    return new Region(-1, -1, -1);
                }

                GetRegion(elem.X, elem.Y, elem.Z, out int row, out int column, out int layer);
                return new Region(row, column, layer);
            }
            finally
            {
                RegionLock.ExitReadLock();
            }
        }

        #region private
        private ReaderWriterLockSlim RegionLock;
        private Dictionary<int, Element>[][][] Regions;
        private Dictionary<int, Element> Oversized;
        private int RegionSize;
        private int Width;
        private int Height;
        private int Depth;
        private int Columns;
        private int Rows;
        private int Layers;
        private int XOffset;
        private int YOffset;
        private int ZOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOversized(Element elem)
        {
            return elem.Width > RegionSize || elem.Height > RegionSize || elem.Depth > RegionSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetRegion(float x, float y, float z, out int row, out int column, out int layer)
        {
            // round to avoid near boundary misses
            x = (float)Math.Round(x);
            y = (float)Math.Round(y);
            z = (float)Math.Round(z);

            // get region
            row = (int)Math.Floor((y + YOffset) / RegionSize);
            column = (int)Math.Floor((x + XOffset) / RegionSize);
            layer = (int)Math.Floor((z + ZOffset) / RegionSize);
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
