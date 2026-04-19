using System.Diagnostics;
using engine.Common.Entities;
using engine.Common.Entities3D;

namespace engine.Common.Tests.Performance
{
    [TestClass]
    public class EngineUpdatePerformance
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        [TestCategory("Performance")]
        public void Dense3DMovementReportsCollisionCost()
        {
            var counts = new[] { 250, 1000, 2500 };
            var worstAverageMs = 0d;

            foreach (var count in counts)
            {
                var metrics = MeasureMovePerformance(count, iterations: 1500);
                TestContext?.WriteLine($"{count} 3D obstacles => {metrics.AverageMs:0.000} ms/move, {metrics.OperationsPerSecond:0} moves/s, {metrics.BytesPerOperation} B/op");
                worstAverageMs = Math.Max(worstAverageMs, metrics.AverageMs);
            }

            Assert.IsTrue(worstAverageMs < 20,
                $"Dense 3D movement became too slow. Worst average move time: {worstAverageMs:0.000} ms.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void ProjectileStormUpdateReportsTickCost()
        {
            var counts = new[] { 250, 1000, 2500 };
            var worstAverageMs = 0d;

            foreach (var count in counts)
            {
                var metrics = MeasureProjectileUpdatePerformance(count, ticks: 30);
                TestContext?.WriteLine($"{count} projectiles => {metrics.AverageMs:0.000} ms/tick, {metrics.OperationsPerSecond:0} projectile updates/s, {metrics.BytesPerOperation} B/op");
                worstAverageMs = Math.Max(worstAverageMs, metrics.AverageMs);
            }

            Assert.IsTrue(worstAverageMs < 50,
                $"Projectile updates became too slow. Worst average tick time: {worstAverageMs:0.000} ms.");
        }

        private static PerfMetrics MeasureMovePerformance(int obstacleCount, int iterations)
        {
            var player = new Player3D()
            {
                X = 0f,
                Y = 0f,
                Z = 100f,
                Width = 48f,
                Height = 60f,
                Depth = 48f,
                ShowDefaultDrawing = false,
                Body = new Humanoid3D(),
            };

            var map = new Map3D(
                width: 12000,
                height: 12000,
                depth: 3000,
                players: new Player[] { player },
                objects: CreateObstacleField(obstacleCount),
                background: new Background(12000, 12000) { GroundColor = new RGBA() { A = 255 }, BasePace = 1f });
            map.IsPaused = false;

            var deltas = new (float X, float Y, float Z)[]
            {
                (0f, 0f, 0.60f),
                (0.15f, 0f, 0.45f),
                (-0.15f, 0f, 0.45f),
                (0f, 0f, -0.60f),
            };

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var allocatedBefore = GC.GetTotalAllocatedBytes(true);
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                var delta = deltas[i % deltas.Length];
                var xdelta = delta.X;
                var ydelta = delta.Y;
                var zdelta = delta.Z;
                map.Move(player, ref xdelta, ref ydelta, ref zdelta, out _, 1f);

                if (Math.Abs(player.Z) > 5000f || Math.Abs(player.X) > 800f)
                {
                    map.MoveAbsolute(player, 0f, 0f, 100f);
                }
            }
            timer.Stop();
            var allocatedAfter = GC.GetTotalAllocatedBytes(true);

            return PerfMetrics.FromElapsed(timer.Elapsed, iterations, allocatedAfter - allocatedBefore);
        }

        private static PerfMetrics MeasureProjectileUpdatePerformance(int projectileCount, int ticks)
        {
            var projectiles = new List<ShotTrajectory3D>(projectileCount);
            for (var i = 0; i < projectileCount; i++)
            {
                var x = (i % 50) * 10f;
                var z = (i / 50) * 15f;
                projectiles.Add(new ShotTrajectory3D(x, 0f, z)
                {
                    X1 = x,
                    Y1 = 0f,
                    Z1 = z,
                    X2 = x + 100f,
                    Y2 = 0f,
                    Z2 = z + 300f,
                    Damage = 6f,
                });
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var allocatedBefore = GC.GetTotalAllocatedBytes(true);
            var timer = Stopwatch.StartNew();
            for (var tick = 0; tick < ticks; tick++)
            {
                foreach (var projectile in projectiles)
                {
                    if (projectile.Action(out var xdelta, out var ydelta, out var zdelta))
                    {
                        projectile.Move(xdelta, ydelta, zdelta);
                    }
                }
            }
            timer.Stop();
            var allocatedAfter = GC.GetTotalAllocatedBytes(true);

            return PerfMetrics.FromElapsed(timer.Elapsed, ticks, allocatedAfter - allocatedBefore);
        }

        private static Element[] CreateObstacleField(int obstacleCount)
        {
            var objects = new List<Element>(obstacleCount);
            var grid = (int)Math.Ceiling(Math.Sqrt(obstacleCount));
            var half = grid / 2;

            for (var row = 0; row < grid && objects.Count < obstacleCount; row++)
            {
                for (var col = 0; col < grid && objects.Count < obstacleCount; col++)
                {
                    // Leave a navigable lane through the middle so movement exercises
                    // both normal travel and grazing collision checks.
                    if (Math.Abs(col - half) <= 1) continue;

                    objects.Add(new Cube()
                    {
                        X = (col - half) * 55f,
                        Y = 0f,
                        Z = 150f + (row * 55f),
                        Width = 36f,
                        Height = 36f,
                        Depth = 36f,
                        Wireframe = false,
                        DisableShading = true,
                        UniformColor = new RGBA() { R = 120, G = 120, B = 160, A = 255 }
                    });
                }
            }

            return objects.ToArray();
        }

        private readonly record struct PerfMetrics(double AverageMs, double OperationsPerSecond, long BytesPerOperation)
        {
            public static PerfMetrics FromElapsed(TimeSpan elapsed, int operations, long allocatedBytes)
            {
                var averageMs = elapsed.TotalMilliseconds / operations;
                var opsPerSecond = elapsed.TotalSeconds > 0 ? operations / elapsed.TotalSeconds : 0d;
                var bytesPerOp = operations > 0 ? allocatedBytes / operations : 0;
                return new PerfMetrics(averageMs, opsPerSecond, bytesPerOp);
            }
        }
    }
}
