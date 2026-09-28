using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Components;
using Fives.Domain;
using Fives.Models;
using Fives.Services;
using Fives.Systems;
using Leopotam.Ecs;

namespace Fives.Runtime.Tests
{
    /// <summary>
    /// A run on the game's 4 x 3 board: a board entity with its tiles, and the gameplay systems on demand.
    ///   0  1  2  3
    ///   4  5  6  7
    ///   8  9 10 11
    /// </summary>
    internal sealed class BoardFixture : IDisposable
    {
        public readonly GameStand Game;
        public readonly FakeFrameTime Time = new FakeFrameTime();
        /// <summary>The puzzle being played.</summary>
        public readonly PuzzleData Puzzle;
        public EcsSystems Systems;
        public EcsEntity Board;
        public BoardState Layout;
        /// <summary>Tile entities by tile id.</summary>
        public EcsEntity[] Tiles;

        public BoardFixture(int stars = TestConfig.Stars)
        {
            Game = new GameStand(stars);
            Session.SetGameMode(Objects.Board());
            Puzzle = Objects.Puzzles("dogs", "Rex")[0];
            Session.SetSelectedImage(Puzzle, null);
            Session.BeginRun();
        }

        public TestObjects Objects => Game.Objects;
        public EcsWorld World => Game.World;
        public GameSession Session => Game.Session;
        public PlayerProgressService Progress => Game.Progress;

        public List<Swap> Moves => Board.Get<BoardHistoryComponent>().Moves;
        public int MovingTiles => World.Count<MoveComponent>();

        public BoardFixture WithSeededBoard(int seed) => WithBoard(SeededShuffle.Create(4, 3, seed), seed);

        public BoardFixture WithBoard(BoardState layout, int seed = 1)
        {
            Layout = layout;
            Board = World.NewEntity();
            Board.Replace(new BoardComponent { State = layout });
            Board.Replace(new BoardHistoryComponent { Seed = seed, Moves = new List<Swap>() });
            Tiles = new EcsEntity[layout.CellCount];
            for (var cell = 0; cell < layout.CellCount; cell++)
            {
                var id = layout[cell];
                var rect = Objects.Rect($"Tile {id}");
                rect.anchoredPosition = Session.SelectedGameMode.CellToAnchored(cell);
                Tiles[id] = World.NewEntity();
                Tiles[id].Replace(new TileComponent { Id = id, Cell = cell, Rect = rect });
            }
            return this;
        }

        /// <summary>Input, projection, movement, win check and completion, in the order GameStartup runs them.</summary>
        public BoardFixture WithGameplaySystems()
        {
            Systems = new EcsSystems(World)
                .Add(new BoardInputSystem()).Add(new BoardProjectionSystem()).Add(new TileMoveSystem()).Add(new WinCheckSystem())
                .Add(new PuzzleCompletionSystem(Progress))
                .OneFrame<TileClickEvent>().OneFrame<TileSwipeEvent>().OneFrame<BoardControlEvent>().OneFrame<BoardChangedEvent>()
                .Inject(Session).Inject(Time);
            Systems.Init();
            return this;
        }

        public void Tap(int tileId)
        {
            World.NewEntity().Replace(new TileClickEvent { Id = tileId });
            Systems.Run();
        }

        /// <summary>A swipe on a tile: dx = 1 is right, dy = 1 is down.</summary>
        public void Swipe(int tileId, int dx, int dy)
        {
            World.NewEntity().Replace(new TileSwipeEvent { Id = tileId, Dx = dx, Dy = dy });
            Systems.Run();
        }

        public void Control(BoardControl control)
        {
            World.NewEntity().Replace(new BoardControlEvent { Control = control });
            Systems.Run();
        }

        /// <summary>Lets running tile animations finish (0.35 s at 0.1 s per frame).</summary>
        public void Settle() => Systems.Tick(5);

        /// <summary>Tile ids by cell.</summary>
        public int[] Snapshot() => Enumerable.Range(0, Layout.CellCount).Select(cell => Layout[cell]).ToArray();

        /// <summary>The selected tile id, or -1.</summary>
        public int Selected => Board.Has<TileSelectionComponent>() ? Board.Get<TileSelectionComponent>().TileId : -1;

        public void Dispose()
        {
            Systems?.Destroy();
            Game.Dispose();
        }
    }
}
