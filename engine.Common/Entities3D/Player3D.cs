using System;
using engine.Common.Entities;

namespace engine.Common.Entities3D
{
    public class Player3D : Player
    {
        public bool ShowTarget { get; set; }
        internal object SyncRoot => _syncRoot;

        public Element3D Body
        {
            get
            {
                return _body;
            }
            set
            {
                lock (_syncRoot)
                {
                    _body = value;
                    if (_body != null)
                    {
                        _body.Width = Width;
                        _body.Height = Height;
                        _body.Depth = Depth;
                        _body.X = X;
                        _body.Y = Y;
                        _body.Z = Z;
                    }
                }
            }
        }
        public bool LockBodyToCamera { get; set; } = true;

        public Player3D()
        {
            Depth = Math.Max(Width, Height);
        }

        public override void Draw(IGraphics g)
        {
            lock (_syncRoot)
            {
                if (ShowDefaultDrawing && _body == null) Body = CreateDefaultBody();

                if (ShowTarget)
                {
                    g.DisableTranslation();
                    {
                        g.Line(RGBA.Black, g.Width / 2, (g.Height / 2) - 10, g.Width / 2, (g.Height / 2) + 10, 1);
                        g.Line(RGBA.Black, (g.Width / 2) - 10, g.Height / 2, (g.Width / 2) + 10, g.Height / 2, 1);
                    }
                    g.EnableTranslation();
                }
                if (_body != null)
                {
                    // The local player body can be camera-locked, but other 3D players/NPCs
                    // should render in world space with normal perspective.
                    if (LockBodyToCamera) g.DisableTranslation(TranslationOptions.Translation | TranslationOptions.Scaling);
                    {
                        _body.Draw(g);
                    }
                    if (LockBodyToCamera) g.EnableTranslation();
                }
            }
        }

        public override void Move(float xDelta, float yDelta, float zDelta)
        {
            lock (_syncRoot)
            {
                base.Move(xDelta, yDelta, zDelta);

                if (_body != null)
                {
                    _body.Width = Width;
                    _body.Height = Height;
                    _body.Depth = Depth;
                    _body.Move(xDelta, yDelta, zDelta);
                }
            }
        }

        #region private
        private readonly object _syncRoot = new object();
        private Element3D _body;

        private static Humanoid3D CreateDefaultBody()
        {
            var body = new Humanoid3D()
            {
                ShirtColor = new RGBA() { R = 96, G = 108, B = 92, A = 255 },
                PantsColor = new RGBA() { R = 52, G = 60, B = 72, A = 255 },
                BootColor = new RGBA() { R = 86, G = 62, B = 42, A = 255 },
                SkinColor = new RGBA() { R = 244, G = 214, B = 58, A = 255 },
                BackpackColor = new RGBA() { R = 81, G = 95, B = 75, A = 255 },
            };

            return body;
        }
        #endregion
    }
}
