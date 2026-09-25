using System;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public sealed class BoardHintTests
    {
        //  4 x 3:  0  1  2  3
        //          4  5  6  7
        //          8  9 10 11
        private static BoardState Board(params (int cell, int tile)[] moved)
        {
            var tiles = Enumerable.Range(0, 12).ToArray();
            foreach (var (cell, tile) in moved)
                tiles[cell] = tile;
            return new BoardState(4, 3, tiles);
        }

        [Test]
        public void SolvedBoard_HasNoHint()
        {
            Assert.That(BoardHint.ClosestMisplaced(new BoardState(4, 3)), Is.Empty);
        }

        [Test]
        public void ClosestMisplaced_ReturnsEveryTileAtTheSmallestDistance()
        {
            // Tiles 0 and 1 are one cell from home; tiles 3 and 8 are five cells away.
            var board = Board((0, 1), (1, 0), (3, 8), (8, 3));

            Assert.That(BoardHint.ClosestMisplaced(board), Is.EquivalentTo(new[] { 0, 1 }));
        }

        [Test]
        public void Path_GoesAroundAPlacedTile_WhenAnotherShortestPathExists()
        {
            // Tile 5 sits in cell 0; cell 1 holds its own tile, cell 4 does not.
            var board = Board((0, 5), (4, 0), (5, 4));
            Assert.That(BoardHint.PathOf(board, 5), Is.EqualTo(new[] { 0, 4, 5 }));

            var mirrored = Board((0, 5), (1, 0), (5, 1));
            Assert.That(BoardHint.PathOf(mirrored, 5), Is.EqualTo(new[] { 0, 1, 5 }));
        }

        [Test]
        public void Path_PrefersHorizontalSteps_WhenRoutesAreEqual()
        {
            // Both cells between 0 and 5 hold misplaced tiles.
            var board = Board((0, 5), (1, 4), (4, 1), (5, 0));

            Assert.That(BoardHint.PathOf(board, 5), Is.EqualTo(new[] { 0, 1, 5 }));
        }

        [TestCase(4, 3)]
        [TestCase(3, 3)]
        [TestCase(6, 6)]
        public void FollowingAPath_PlacesTheTile_AndMovesOthersByOneCellAtMost(int columns, int rows)
        {
            for (var seed = 1; seed <= 300; seed++)
            {
                var start = SeededShuffle.Create(columns, rows, seed);
                foreach (var tile in Enumerable.Range(0, start.CellCount))
                {
                    var board = start.Copy();
                    var path = BoardHint.PathOf(board, tile);
                    var from = board.CellOf(tile);

                    Assert.That(path.First(), Is.EqualTo(from));
                    Assert.That(path.Last(), Is.EqualTo(tile));
                    Assert.That(path.Count - 1, Is.EqualTo(Math.Abs(from % columns - tile % columns) + Math.Abs(from / columns - tile / columns)),
                        "a shortest path");

                    for (var step = 1; step < path.Count; step++)
                        Assert.That(board.TrySwap(new Swap(path[step - 1], path[step])), Is.True, $"seed={seed}, tile={tile}, step={step}");

                    Assert.That(board.CellOf(tile), Is.EqualTo(tile));
                    for (var other = 0; other < board.CellCount; other++)
                    {
                        if (other == tile) continue;
                        var before = start.CellOf(other);
                        var after = board.CellOf(other);
                        Assert.That(Math.Abs(before % columns - after % columns) + Math.Abs(before / columns - after / columns),
                            Is.LessThanOrEqualTo(1), $"seed={seed}, tile={tile}, other={other}");
                    }
                }
            }
        }
    }
}
