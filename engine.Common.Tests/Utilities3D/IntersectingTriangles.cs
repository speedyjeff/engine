using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace engine.Common.Tests
{
	[TestClass]
	public class IntersectingTriangles
    {

		[TestMethod]
		public void NotOverlapping()
		{
			// https://gamedev.stackexchange.com/questions/88060/triangle-triangle-intersection-code
			var a = new Point() { X = -0.323523998f, Y = 1.68264794f, Z = -1.20740700f };
			var b = new Point() { X = -0.478354007f, Y = 1.68264794f, Z = -1.15484905f };
			var c = new Point() { X = -0.465537012f, Y = 1.80764794f, Z = -1.12390494f };

			var x = new Point() { X = 1.00000000f, Y = -0.682647943f, Z = -0.999998987f };
			var y = new Point() { X = 1.00000000f, Y = -2.68264794f, Z = -1.00000000f };
			var z = new Point() { X = -1.00000000f, Y = -0.682647943f, Z = -1.00000000f };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // false

			Assert.IsFalse(collision, "NotOverlapping failed");
		}

		[TestMethod]
		public void Overlapping1()
		{
			var a = new Point() { X = 3, Y = 1, Z = 1 };
			var b = new Point() { X = 1, Y = 4, Z = 2 };
			var c = new Point() { X = 1, Y = 3, Z = 4 };

			var x = new Point() { X = 1, Y = 1, Z = 4 };
			var y = new Point() { X = 1, Y = 4, Z = 1 };
			var z = new Point() { X = 4, Y = 1, Z = 1 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // true

			Assert.IsTrue(collision, "Overlapping1 failed");
		}

		[TestMethod]
		public void Overlapping2()
		{
			var a = new Point() { X = 0, Y = 5, Z = 0 };
			var b = new Point() { X = 0, Y = 0, Z = 0 };
			var c = new Point() { X = 8, Y = 0, Z = 0 };

			var x = new Point() { X = 6, Y = 8, Z = 3 };
			var y = new Point() { X = 6, Y = 8, Z = -2 };
			var z = new Point() { X = 6, Y = -4, Z = -2 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // true

			Assert.IsTrue(collision, "Overlapping2 failed");
		}

		[TestMethod]
		public void CoplanarNoOverlap()
		{
			var a = new Point() { X = 10, Y = -5, Z = 8 };
			var b = new Point() { X = 10, Y = 1, Z = 1 };
			var c = new Point() { X = 10, Y = 5, Z = 3 };

			var x = new Point() { X = 10, Y = -5, Z = -8 };
			var y = new Point() { X = 10, Y = 1, Z = -1 };
			var z = new Point() { X = 10, Y = 5, Z = -3 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // false

			Assert.IsFalse(collision, "CoplanarNoOverlap failed");
		}

		[TestMethod]
		public void CoplanarOverlap()
		{
			var a = new Point() { X = -5, Y = 8, Z = 8 };
			var b = new Point() { X = 1, Y = 8, Z = 1 };
			var c = new Point() { X = 5, Y = 8, Z = 3 };

			var x = new Point() { X = -5, Y = 8, Z = -8 };
			var y = new Point() { X = 1, Y = 8, Z = 2 };
			var z = new Point() { X = 5, Y = 8, Z = -3 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // true

			Assert.IsTrue(collision, "CoplanarNoOverlap failed");
		}

		[TestMethod]
		public void Parallel()
		{
			var a = new Point() { X = -5, Y = 1, Z = 8 };
			var b = new Point() { X = 1, Y = 8, Z = 8 };
			var c = new Point() { X = 5, Y = 1, Z = 8 };

			var x = new Point() { X = -5, Y = 1, Z = 3 };
			var y = new Point() { X = 1, Y = 8, Z = 3 };
			var z = new Point() { X = 5, Y = 1, Z = 3 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // false

			Assert.IsFalse(collision, "CoplanarNoOverlap failed");
		}

		[TestMethod]
		public void Touching()
		{
			var a = new Point() { X = -5, Y = 1, Z = 8 };
			var b = new Point() { X = 1, Y = 8, Z = 5 };
			var c = new Point() { X = 5, Y = 1, Z = 8 };

			var x = new Point() { X = -5, Y = 1, Z = 3 };
			var y = new Point() { X = 1, Y = 8, Z = 5 };
			var z = new Point() { X = 5, Y = 1, Z = 3 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // true

			Assert.IsTrue(collision, "CoplanarNoOverlap failed");
		}

		[TestMethod]
		public void NotTouching()
		{
			// realworld example
			var a = new Point() { X = -25, Y = -50, Z = 234 };
			var b = new Point() { X = -25, Y = -50, Z = 284 };
			var c = new Point() { X = 25, Y = -50, Z = 284 };

			var x = new Point() { X = -117, Y = -50, Z = 0 };
			var y = new Point() { X = 26, Y = -50, Z = -45 };
			var z = new Point() { X = -48, Y = -50, Z = -30 };

			var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(a, b, c, x, y, z); // false

			Assert.IsFalse(collision, "NotTouching failed");
		}

		[TestMethod]
		public void RotateTest()
		{
			var p1 = new Point() { X = 5, Y = 5, Z = 0 };
			var p2 = new Point() { X = 5, Y = 0, Z = 0 };
			var p3 = new Point() { X = 5, Y = 5, Z = 5 };

			// plane to intersect
			var b1 = new Point() { X = -10, Y = 2, Z = -10 };
			var b2 = new Point() { X = 10, Y = 2, Z = -10 };
			var b3 = new Point() { X = 0, Y = 2, Z = 10 };

			var yangles = new float[] { 0, 45, 90, 135, 180, 225, 270, 315 };
			var xangles = new float[] { 0, 315 /*-45*/, 45 };

			var collisions = new Dictionary<float, Dictionary<float, bool>>()
			{
				{0, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, true }, { 45, false } } },
				{45, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, true }, { 45, false } } },
				{90, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, false }, { 45, false } } },
				{135, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, false }, { 45, false } } },
				{180, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, true }, { 45, true } } },
				{225, new Dictionary<float, bool>() { { 0, false }, { 315 /*-45*/, false }, { 45, false } } },
				{270, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, false }, { 45, false } } },
				{315, new Dictionary<float, bool>() { { 0, true }, { 315 /*-45*/, false }, { 45, false } } }
			};

			for (int yangle = 0; yangle < 360; yangle += 45)
			{
				foreach (var xangle in new float[] { 0, 315 /*-45*/, 45 })
				{
					var a = new Point() { X = p1.X, Y = p1.Y, Z = p1.Z };
					var b = new Point() { X = p2.X, Y = p2.Y, Z = p2.Z };
					var c = new Point() { X = p3.X, Y = p3.Y, Z = p3.Z };

					Common.Entities3D.Utilities3D.Yaw(yangle, ref a.X, ref a.Y, ref a.Z);
					Common.Entities3D.Utilities3D.Yaw(yangle, ref b.X, ref b.Y, ref b.Z);
					Common.Entities3D.Utilities3D.Yaw(yangle, ref c.X, ref c.Y, ref c.Z);

					Common.Entities3D.Utilities3D.Pitch(xangle, ref a.X, ref a.Y, ref a.Z);
					Common.Entities3D.Utilities3D.Pitch(xangle, ref b.X, ref b.Y, ref b.Z);
					Common.Entities3D.Utilities3D.Pitch(xangle, ref c.X, ref c.Y, ref c.Z);

					var collision = Common.Entities3D.Utilities3D.IntersectingTriangles(b1, b2, b3, a, b, c);

					Assert.AreEqual(collision, collisions[yangle][xangle], $"{xangle},{yangle} failed. !{collision}");
				}
			}
		}

		[TestMethod]
		public void Perf()
		{
			var timer = new Stopwatch();
			var iters = 100000;

			// overlapping
			var oa = new Point() { X = 3, Y = 1, Z = 1 };
			var ob = new Point() { X = 1, Y = 4, Z = 2 };
			var oc = new Point() { X = 1, Y = 3, Z = 4 };

			var ox = new Point() { X = 1, Y = 1, Z = 4 };
			var oy = new Point() { X = 1, Y = 4, Z = 1 };
			var oz = new Point() { X = 4, Y = 1, Z = 1 };

			// not overlapping
			var noa = new Point() { X = -0.323523998f, Y = 1.68264794f, Z = -1.20740700f };
			var nob = new Point() { X = -0.478354007f, Y = 1.68264794f, Z = -1.15484905f };
			var noc = new Point() { X = -0.465537012f, Y = 1.80764794f, Z = -1.12390494f };

			var nox = new Point() { X = 1.00000000f, Y = -0.682647943f, Z = -0.999998987f };
			var noy = new Point() { X = 1.00000000f, Y = -2.68264794f, Z = -1.00000000f };
			var noz = new Point() { X = -1.00000000f, Y = -0.682647943f, Z = -1.00000000f };

			// coplanner
			var cpa = new Point() { X = -5, Y = 8, Z = 8 };
			var cpb = new Point() { X = 1, Y = 8, Z = 1 };
			var cpc = new Point() { X = 5, Y = 8, Z = 3 };

			var cpx = new Point() { X = -5, Y = 8, Z = -8 };
			var cpy = new Point() { X = 1, Y = 8, Z = 2 };
			var cpz = new Point() { X = 5, Y = 8, Z = -3 };

			timer.Start();
			for (int i = 0; i < iters; i++)
			{
				Common.Entities3D.Utilities3D.IntersectingTriangles(oa, ob, oc, ox, oy, oz);
				Common.Entities3D.Utilities3D.IntersectingTriangles(noa, nob, noc, nox, noy, noz);
				Common.Entities3D.Utilities3D.IntersectingTriangles(cpa, cpb, cpc, cpx, cpy, cpz);
			}
			timer.Stop();

			var ellapsed = timer.ElapsedMilliseconds;

            Assert.IsTrue(ellapsed < 150, $"Execution was too long {ellapsed}");
		}
	}
}
