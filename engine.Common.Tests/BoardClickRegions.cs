using engine.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace engine.Common.Tests
{
    [TestClass]
    public class BoardClickRegions
    {
        [TestMethod]
        public void ClickInvokesMatchingRegionHandlers()
        {
            var region = new BoardClickRegion("track-1", CreateCell(
                10, 10,
                40, 20,
                35, 50,
                5, 45));

            var regionClickCount = 0;
            var boardRegionClickCount = 0;
            BoardClickRegion clickedRegion = null;
            MouseButton clickedButton = MouseButton.None;
            float localX = 0;
            float localY = 0;

            region.OnClick = (r, button, x, y) =>
            {
                regionClickCount++;
                clickedRegion = r;
                clickedButton = button;
                localX = x;
                localY = y;
            };

            var board = CreateBoard(region);
            board.OnRegionClicked += (r, button, x, y) =>
            {
                boardRegionClickCount++;
                Assert.AreSame(region, r);
                Assert.AreEqual(MouseButton.Left, button);
                Assert.AreEqual(15f, x);
                Assert.AreEqual(15f, y);
            };

            board.Mousedown(MouseButton.Left, 20, 25);

            Assert.AreEqual(1, regionClickCount);
            Assert.AreEqual(1, boardRegionClickCount);
            Assert.AreSame(region, clickedRegion);
            Assert.AreEqual(MouseButton.Left, clickedButton);
            Assert.AreEqual(15f, localX);
            Assert.AreEqual(15f, localY);
        }

        [TestMethod]
        public void ClickOutsideRegionDoesNotInvokeRegionHandlers()
        {
            var region = new BoardClickRegion("track-1", CreateCell(
                10, 10,
                40, 20,
                35, 50,
                5, 45));

            var regionClickCount = 0;
            var cellClickCount = 0;
            region.OnClick = (r, button, x, y) => regionClickCount++;

            var board = CreateBoard(region);
            board.OnRegionClicked += (r, button, x, y) => regionClickCount++;
            board.OnCellClicked += (row, col, x, y) => cellClickCount++;

            board.Mousedown(MouseButton.Left, 90, 90);

            Assert.AreEqual(0, regionClickCount);
            Assert.AreEqual(1, cellClickCount);
        }

        private static Board CreateBoard(params BoardClickRegion[] clickRegions)
        {
            return new Board(new BoardConfiguration()
            {
                Width = 100,
                Height = 100,
                Rows = 1,
                Columns = 1,
                EdgeAngle = 0,
                Background = RGBA.Black,
                ClickRegions = clickRegions
            });
        }

        private static BoardCell CreateCell(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            return new BoardCell(new Point[]
            {
                new Point(x1, y1, 0),
                new Point(x2, y2, 0),
                new Point(x3, y3, 0),
                new Point(x4, y4, 0)
            });
        }
    }
}
