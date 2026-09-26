using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;

namespace Fives.Runtime.Tests
{
    /// <summary>A live board entity with tile views and the input → projection → movement → win → completion pipeline.</summary>
    internal sealed class BoardFixture : IDisposable
    {
        public readonly TestObjects Objects = new TestObjects();
        public readonly EcsWorld World = new EcsWorld();
        public readonly FakeFrameTime Time = new FakeFrameTime();
        public readonly GameSession Session;
        public readonly PlayerProgressService Progress;
        /// <summary>The puzzle being played.</summary>
        public readonly PuzzleData Puzzle;
        public EcsSystems Systems;
        public EcsEntity Board;
        public BoardState Layout;
        public EcsEntity[] Tiles;

        public BoardFixture(int columns = 3, int rows = 3)
        {
            var config = Objects.Config();
            Session = new GameSession(new GameBalance(config));
            Session.SetGameMode(Objects.Mode(columns, rows));
            Puzzle = Objects.Puzzles("dogs", "Rex")[0];
            Session.SetSelectedImage(Puzzle, null);
            Session.BeginRun();
            Progress = new PlayerProgressService(new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config)));
        }

        public List<Swap> Moves => Board.Get<BoardHistoryComponent>().Moves;
        public int MovingTiles => World.Count<MoveComponent>();

        public BoardFixture WithSeededBoard(int seed) => WithBoard(SeededShuffle.Create(Session.SelectedGameMode.Columns, Session.SelectedGameMode.Rows, seed), seed);

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

        /// <summary>The gameplay group as GameStartup orders it.</summary>
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

        public int[] Snapshot() => Enumerable.Range(0, Layout.CellCount).Select(cell => Layout[cell]).ToArray();

        public int Selected => Board.Has<TileSelectionComponent>() ? Board.Get<TileSelectionComponent>().TileId : -1;

        public void Dispose()
        {
            Systems?.Destroy();
            World.Destroy();
            Objects.Dispose();
        }
    }
}
