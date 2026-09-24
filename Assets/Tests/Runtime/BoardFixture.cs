using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Systems;

namespace Fives.Runtime.Tests
{
    /// <summary>A live board entity with tile views and the input → projection → movement → win pipeline.</summary>
    internal sealed class BoardFixture : IDisposable
    {
        public readonly TestObjects Objects = new TestObjects();
        public readonly EcsWorld World = new EcsWorld();
        public readonly FakeFrameTime Time = new FakeFrameTime();
        public readonly GameSession Session;
        public readonly EcsEntity State;
        public EcsSystems Systems;
        public EcsEntity Board;
        public BoardState Layout;
        public EcsEntity[] Tiles;

        public BoardFixture(int size = 3)
        {
            Session = new GameSession(Objects.Config());
            Session.SetGameMode(Objects.Mode(size));
            Session.BeginRun();
            State = World.NewEntity();
            State.Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        }

        public List<Swap> Moves => Board.Get<BoardHistoryComponent>().Moves;
        public List<Swap> Undone => Board.Get<BoardHistoryComponent>().Undone;
        public int MovingTiles => World.Count<MoveComponent>();

        public BoardFixture WithSeededBoard(int seed) => WithBoard(SeededShuffle.Create(Session.SelectedGameMode.BoardSize, seed), seed);

        public BoardFixture WithBoard(BoardState layout, int seed = 1)
        {
            Layout = layout;
            Board = World.NewEntity();
            Board.Replace(new BoardComponent { State = layout });
            Board.Replace(new BoardHistoryComponent { Seed = seed, Moves = new List<Swap>(), Undone = new List<Swap>() });
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

        /// <summary>The gameplay pipeline as GameStartup orders it, with WinCheckSystem after the group.</summary>
        public BoardFixture WithGameplaySystems()
        {
            Systems = new EcsSystems(World)
                .Add(new BoardInputSystem()).Add(new BoardProjectionSystem()).Add(new TileMoveSystem()).Add(new WinCheckSystem())
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
