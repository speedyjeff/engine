using System;

namespace engine.Common
{
    public delegate void BoardClickRegionDelegate(BoardClickRegion region, MouseButton button, float x, float y);

    public class BoardClickRegion
    {
        public BoardClickRegion()
        {
        }

        public BoardClickRegion(string id, BoardCell shape)
        {
            Id = id;
            Shape = shape;
        }

        public string Id { get; set; }
        public object Tag { get; set; }
        public BoardCell Shape { get; set; }
        public BoardClickRegionDelegate OnClick { get; set; }

        internal void Click(MouseButton button, float x, float y)
        {
            if (OnClick != null) OnClick(this, button, x, y);
        }
    }
}
