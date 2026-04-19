using System.Diagnostics;
using System.Drawing;
using engine.Common.Entities;
using engine.Common.Entities3D;
using engine.Winforms;

namespace engine.Common.Tests.Performance
{
    [TestClass]
    public class WinformsPaintPerformance
    {
        public TestContext? TestContext { get; set; }

        [TestMethod]
        [TestCategory("Performance")]
        public void WorldPaintWithHundredsOf3DObjectsStaysResponsive()
        {
            var metrics = MeasureAveragePaintPerformance(SceneMode.ThreeD, objectCount: 250, frames: 6);
            TestContext?.WriteLine($"3D average frame time for 250 objects: {metrics.AverageFrameMs:0.00} ms, {metrics.FramesPerSecond:0.0} FPS, {metrics.BytesPerFrame} B/frame");

            Assert.IsTrue(metrics.AverageFrameMs < 100,
                $"World.Paint averaged {metrics.AverageFrameMs:0.00} ms for 250 objects, which is slower than expected.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void WinformsBackendReportsApproximateObjectLimit()
        {
            var twoDLimit = ReportSceneLimit(SceneMode.TwoD, new[] { 100, 500, 1000, 2500, 5000 }, frames: 4);
            var threeDLimit = ReportSceneLimit(SceneMode.ThreeD, new[] { 50, 100, 250, 500, 1000, 2000 }, frames: 4);
            var projectileLimit = ReportSceneLimit(SceneMode.ProjectileStorm, new[] { 100, 500, 1000, 2500 }, frames: 4);

            Assert.IsTrue(twoDLimit >= 1000, $"2D WinForms rendering degraded too early. 30 FPS limit: {twoDLimit} objects.");
            Assert.IsTrue(threeDLimit >= 250, $"3D WinForms rendering degraded too early. 30 FPS limit: {threeDLimit} objects.");
            Assert.IsTrue(projectileLimit >= 500, $"Projectile-heavy rendering degraded too early. 30 FPS limit: {projectileLimit} objects.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void MixedSceneAndProjectileStormStayResponsive()
        {
            var metrics = MeasureAveragePaintPerformance(SceneMode.ProjectileStorm, objectCount: 1000, frames: 5);
            TestContext?.WriteLine($"Projectile storm frame time: {metrics.AverageFrameMs:0.00} ms, {metrics.FramesPerSecond:0.0} FPS, {metrics.BytesPerFrame} B/frame");

            Assert.IsTrue(metrics.AverageFrameMs < 100,
                $"Projectile-heavy scene averaged {metrics.AverageFrameMs:0.00} ms/frame, which is slower than expected.");
        }

        private int ReportSceneLimit(SceneMode mode, int[] counts, int frames)
        {
            var thirtyFpsBudgetMs = 33.0;
            var responsiveLimit = 0;

            TestContext?.WriteLine($"{mode} scene scaling:");
            foreach (var count in counts)
            {
                var metrics = MeasureAveragePaintPerformance(mode, count, frames);
                TestContext?.WriteLine($"  {count} objects => {metrics.AverageFrameMs:0.00} ms/frame, {metrics.FramesPerSecond:0.0} FPS, {metrics.BytesPerFrame} B/frame");

                if (metrics.AverageFrameMs <= thirtyFpsBudgetMs)
                {
                    responsiveLimit = count;
                }
            }

            TestContext?.WriteLine($"Approximate 30 FPS {mode} limit: {responsiveLimit}");
            return responsiveLimit;
        }

        private static PaintMetrics MeasureAveragePaintPerformance(SceneMode mode, int objectCount, int frames)
        {
            using var bitmap = new Bitmap(1280, 720);
            using var graphics = Graphics.FromImage(bitmap);
            var surface = new WritableGraphics(BufferedGraphicsManager.Current, graphics, bitmap.Height, bitmap.Width);

            var world = CreateWorld(mode, objectCount);
            world.InitializeGraphics(surface, new NoOpSounds());

            // Warm up caches and JIT before measurement.
            world.Paint();
            world.Paint();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var allocatedBefore = GC.GetTotalAllocatedBytes(true);
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < frames; i++)
            {
                world.Paint();
            }
            timer.Stop();
            var allocatedAfter = GC.GetTotalAllocatedBytes(true);

            var averageFrameMs = timer.Elapsed.TotalMilliseconds / frames;
            var fps = averageFrameMs > 0 ? 1000d / averageFrameMs : 0d;
            var bytesPerFrame = (allocatedAfter - allocatedBefore) / frames;
            return new PaintMetrics(averageFrameMs, fps, bytesPerFrame);
        }

        private static World CreateWorld(SceneMode mode, int objectCount)
        {
            if (mode == SceneMode.TwoD)
            {
                return Create2DWorld(objectCount);
            }

            return Create3DWorld(mode, objectCount);
        }

        private static World Create2DWorld(int objectCount)
        {
            var human = new Player()
            {
                X = 0f,
                Y = 0f,
                Width = 40f,
                Height = 40f,
            };

            var players = new List<Player>(objectCount + 1) { human };
            var grid = (int)Math.Ceiling(Math.Sqrt(objectCount));
            var half = grid / 2;

            for (var row = 0; row < grid && players.Count <= objectCount; row++)
            {
                for (var col = 0; col < grid && players.Count <= objectCount; col++)
                {
                    players.Add(new Player()
                    {
                        X = (col - half) * 55f,
                        Y = (row - half) * 55f,
                        Width = 36f,
                        Height = 36f,
                    });
                }
            }

            return new World(
                new WorldConfiguration()
                {
                    Width = 1280,
                    Height = 720,
                    Is3D = false,
                    HorizonX = 5000,
                    HorizonY = 5000,
                    HorizonZ = 1000,
                    EnableZoom = false,
                },
                players.ToArray(),
                Array.Empty<Element>(),
                new Background(8000, 8000) { GroundColor = new RGBA() { R = 240, G = 240, B = 240, A = 255 } });
        }

        private static World Create3DWorld(SceneMode mode, int objectCount)
        {
            var player = new Player3D()
            {
                X = 0f,
                Y = 0f,
                Z = 400f,
                Width = 50f,
                Height = 50f,
                Depth = 50f,
                ShowDefaultDrawing = false,
                Body = new Humanoid3D(),
            };

            var objects = new List<Element>(objectCount);
            var grid = (int)Math.Ceiling(Math.Sqrt(objectCount));
            var half = grid / 2;

            for (var row = 0; row < grid && objects.Count < objectCount; row++)
            {
                for (var col = 0; col < grid && objects.Count < objectCount; col++)
                {
                    if (mode == SceneMode.ProjectileStorm && ((row + col) % 2 == 0))
                    {
                        objects.Add(new ShotTrajectory3D((col - half) * 40f, 0f, 150f + (row * 40f))
                        {
                            X1 = (col - half) * 40f,
                            Y1 = 0f,
                            Z1 = 150f + (row * 40f),
                            X2 = (col - half) * 40f,
                            Y2 = 0f,
                            Z2 = 220f + (row * 40f),
                            Damage = 8f,
                        });
                        continue;
                    }

                    objects.Add(new Cube()
                    {
                        X = (col - half) * 70f,
                        Y = 0f,
                        Z = 200f + (row * 70f),
                        Width = 32f,
                        Height = 32f,
                        Depth = 32f,
                        Wireframe = false,
                        DisableShading = true,
                        UniformColor = new RGBA()
                        {
                            R = (byte)(80 + ((row * 17) % 120)),
                            G = (byte)(90 + ((col * 13) % 100)),
                            B = (byte)(120 + ((row + col) % 80)),
                            A = 255
                        }
                    });
                }
            }

            return new World(
                new WorldConfiguration()
                {
                    Width = 1280,
                    Height = 720,
                    Is3D = true,
                    HorizonX = 4000,
                    HorizonY = 2500,
                    HorizonZ = 4000,
                    CameraZ = 900,
                    EnableZoom = false,
                },
                new Player[] { player },
                objects.ToArray(),
                new Background(8000, 8000) { GroundColor = new RGBA() { R = 240, G = 240, B = 240, A = 255 } });
        }

        private enum SceneMode
        {
            TwoD,
            ThreeD,
            ProjectileStorm
        }

        private readonly record struct PaintMetrics(double AverageFrameMs, double FramesPerSecond, long BytesPerFrame);

        private sealed class NoOpSounds : ISounds
        {
            public void Play(string path) { }
            public void Play(string name, Stream stream) { }
            public void PlayMusic(string path, bool repeat) { }
            public void Repeat() { }
        }
    }
}
