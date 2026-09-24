using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Systems;
using UnityEngine;

internal static partial class CorrectnessProbes
{
    private static EcsEntity CreateBoard(EcsWorld world, int size, int seed) =>
        CreateBoard(world, SeededShuffle.Create(size, seed), seed);

    private static EcsEntity CreateBoard(EcsWorld world, BoardState board, int seed = 1)
    {
        var entity = world.NewEntity();
        entity.Replace(new BoardComponent { State = board });
        entity.Replace(new BoardHistoryComponent { Seed = seed, Moves = new List<Swap>(), Undone = new List<Swap>() });
        return entity;
    }

    private static EcsEntity[] CreateTiles(EcsWorld world, BoardState board)
    {
        var tiles = new EcsEntity[board.CellCount];
        for (var cell = 0; cell < board.CellCount; cell++)
        {
            var id = board[cell];
            tiles[id] = world.NewEntity();
            tiles[id].Replace(new TileComponent
            {
                Id = id, Cell = cell,
                Rect = new RectTransform { anchoredPosition = new Vector2(cell % board.Size, -(cell / board.Size)) }
            });
        }
        return tiles;
    }

    private static EcsSystems CreateBoardSystems(EcsWorld world, GameSession session) => new EcsSystems(world)
        .Add(new BoardInputSystem()).Add(new BoardProjectionSystem())
        .Add(new TileMoveSystem()).Add(new WinCheckSystem())
        .OneFrame<TileClickEvent>().OneFrame<TileSwipeEvent>().OneFrame<BoardControlEvent>().OneFrame<BoardChangedEvent>().Inject(session);

    private static int[] Snapshot(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    private static void Settle(EcsSystems systems) { for (var tick = 0; tick < 5; tick++) systems.Run(); }
    private static void Tap(EcsWorld world, EcsSystems systems, int tileId)
    {
        world.NewEntity().Replace(new TileClickEvent { Id = tileId });
        systems.Run();
    }
    private static void Swipe(EcsWorld world, EcsSystems systems, int tileId, int dx, int dy)
    {
        world.NewEntity().Replace(new TileSwipeEvent { Id = tileId, Dx = dx, Dy = dy });
        systems.Run();
    }
    private static int MovingTiles(EcsWorld world) => world.GetFilter(typeof(EcsFilter<TileComponent, MoveComponent>)).GetEntitiesCount();
    private static void Control(EcsWorld world, EcsSystems systems, BoardControl control)
    {
        world.NewEntity().Replace(new BoardControlEvent { Control = control });
        systems.Run();
    }

    private static void CheckRapidClicks()
    {
        Time.deltaTime = 0.1f;
        var world = new EcsWorld();
        world.NewEntity().Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 3, TileSize = 1 });
        session.BeginRun();
        var entity = CreateBoard(world, 3, 42);
        var board = entity.Get<BoardComponent>().State;
        CreateTiles(world, board);
        var first = board[0];
        var systems = CreateBoardSystems(world, session);
        systems.Init();
        world.NewEntity().Replace(new TileSwipeEvent { Id = board[0], Dx = 1 });
        world.NewEntity().Replace(new TileSwipeEvent { Id = board[4], Dx = 1 });
        systems.Run();
        Check("rapid swipes accept exactly one swap", entity.Get<BoardHistoryComponent>().Moves.Count == 1
            && board[1] == first, "one accepted command and one history entry");
        systems.Destroy(); world.Destroy();
    }

    private static void RunBoardProbes()
    {
        CheckTapSelection();
        CheckIdleProjection();
        CheckBoardSetup();
        CheckBoardCleanup();
        Time.deltaTime = 0.1f;
        var world = new EcsWorld();
        var state = world.NewEntity();
        state.Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 3, TileSize = 1 });
        session.BeginRun();
        var entity = CreateBoard(world, 3, 42);
        var board = entity.Get<BoardComponent>().State;
        var tiles = CreateTiles(world, board);
        var initial = Snapshot(board);
        var history = entity.Get<BoardHistoryComponent>().Moves;
        var systems = CreateBoardSystems(world, session);
        systems.Init();
        var undone = entity.Get<BoardHistoryComponent>().Undone;
        Control(world, systems, BoardControl.Undo);
        Control(world, systems, BoardControl.Redo);
        Check("empty history ignores Undo and Redo", history.Count == 0 && undone.Count == 0
            && initial.SequenceEqual(Snapshot(board)), "no mutation");

        var afterMoves = new List<int[]>();
        foreach (var (cell, dx, dy) in new[] { (0, 1, 0), (4, 1, 0), (6, 0, -1) })
        {
            Swipe(world, systems, board[cell], dx, dy); Settle(systems);
            afterMoves.Add(Snapshot(board));
        }
        Check("accepted ECS moves populate history", history.Count == 3, "three moves");
        Control(world, systems, BoardControl.Undo);
        Check("Undo animates and moves the swap to the redo stack", history.Count == 2 && undone.Count == 1
            && afterMoves[1].SequenceEqual(Snapshot(board)) && MovingTiles(world) == 2, "previous board restored, both tiles animating");
        Control(world, systems, BoardControl.Undo);
        Check("Undo during animation is ignored", history.Count == 2, "no overlapping inverse moves");
        Settle(systems);
        Control(world, systems, BoardControl.Undo); Settle(systems);
        Control(world, systems, BoardControl.Redo);
        Check("Redo re-applies the last undone swap", history.Count == 2 && undone.Count == 1
            && afterMoves[1].SequenceEqual(Snapshot(board)) && MovingTiles(world) == 2, "one step forward, animated");
        Settle(systems);
        Control(world, systems, BoardControl.Redo); Settle(systems);
        Check("Redo walks forward to the latest move", history.Count == 3 && undone.Count == 0
            && afterMoves[2].SequenceEqual(Snapshot(board)), "all undone moves restored");
        Control(world, systems, BoardControl.Redo); Settle(systems);
        Check("Redo with nothing undone does nothing", history.Count == 3 && afterMoves[2].SequenceEqual(Snapshot(board)), "no mutation");

        while (history.Count > 0) { Control(world, systems, BoardControl.Undo); Settle(systems); }
        Check("Undo all returns exactly to shuffled layout", initial.SequenceEqual(Snapshot(board)) && undone.Count == 3,
            "seeded layout restored");
        Swipe(world, systems, board[8], 0, -1); Settle(systems);
        Check("new move after Undo clears the redo stack", history.Count == 1 && undone.Count == 0
            && history[0].Equals(new Swap(5, 8)), "abandoned moves cannot be redone");
        Tap(world, systems, -1);
        Swipe(world, systems, -1, 1, 0);
        Check("invalid tile ID leaves board and history intact", history.Count == 1
            && !entity.Has<TileSelectionComponent>(), "no fabricated moves or selection");
        var beforeExit = Snapshot(board);
        world.NewEntity().Get<GameEndEvent>();
        Control(world, systems, BoardControl.Undo);
        Check("exit takes precedence over board commands", history.Count == 1 && beforeExit.SequenceEqual(Snapshot(board)),
            "no mutation during cleanup");
        systems.Destroy(); world.Destroy();
    }

    private static void CheckTapSelection()
    {
        Time.deltaTime = 0.1f;
        var world = new EcsWorld();
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 3, TileSize = 1 });
        session.BeginRun();
        var entity = CreateBoard(world, 3, 42);
        var board = entity.Get<BoardComponent>().State;
        CreateTiles(world, board);
        var history = entity.Get<BoardHistoryComponent>().Moves;
        var systems = CreateBoardSystems(world, session);
        systems.Init();
        int Selected() => entity.Has<TileSelectionComponent>() ? entity.Get<TileSelectionComponent>().TileId : -1;

        var corner = board[0];
        Tap(world, systems, corner);
        Check("tap selects a tile without moving it", Selected() == corner && history.Count == 0, $"selected={Selected()}");
        var far = board[8];
        Tap(world, systems, far);
        Check("tap on a non-neighbor moves the selection", Selected() == far && history.Count == 0, $"selected={Selected()}");
        Tap(world, systems, far);
        Check("second tap on the selected tile clears it", Selected() == -1, "deselected");
        var right = board[1];
        Tap(world, systems, corner);
        Tap(world, systems, right);
        Check("tap on a neighbor swaps and clears the selection", history.Count == 1 && board[0] == right && board[1] == corner
            && Selected() == -1 && MovingTiles(world) == 2, "both tiles animate");
        Settle(systems);
        Swipe(world, systems, board[2], 1, 0);
        Swipe(world, systems, board[6], 0, 1);
        Check("swipe off the board edge is rejected", history.Count == 1, "no swap outside the grid");
        Swipe(world, systems, board[4], 0, 1);
        Check("swipe down swaps with the tile below", history.Count == 2 && history[1].Equals(new Swap(4, 7)), history[1].ToString());
        systems.Destroy(); world.Destroy();
    }

        private static void CheckIdleProjection()
    {
        var world = new EcsWorld();
        var session = new GameSession(CreateConfig());
        session.BeginRun();
        var entity = CreateBoard(world, 3, 42);
        var board = entity.Get<BoardComponent>().State;
        var tiles = CreateTiles(world, board);
        var systems = new EcsSystems(world).Add(new BoardProjectionSystem())
            .OneFrame<BoardChangedEvent>().OneFrame<BoardInitializedEvent>().Inject(session);
        systems.Init();
        // A sentinel proves an idle tick does not walk and rewrite tile positions.
        tiles[0].Get<TileComponent>().Cell = -1;
        systems.Run();
        Check("idle projection leaves tiles untouched", tiles[0].Get<TileComponent>().Cell == -1
            && !tiles[0].Has<MoveComponent>(), "no board event, no projection work");
        world.NewEntity().Get<BoardInitializedEvent>();
        systems.Run();
        Check("initial projection places every tile instantly", Enumerable.Range(0, 9).All(cell =>
            tiles[board[cell]].Get<TileComponent>().Cell == cell
            && tiles[board[cell]].Get<MoveComponent>().InstaMove), "first layout appears in place");
        foreach (var tile in tiles) tile.Del<MoveComponent>();
        board.TrySwap(new Swap(0, 1));
        world.NewEntity().Get<BoardChangedEvent>();
        systems.Run();
        Check("a board change animates only the moved tiles", tiles[board[0]].Has<MoveComponent>() && tiles[board[1]].Has<MoveComponent>()
            && !tiles[board[0]].Get<MoveComponent>().InstaMove && Enumerable.Range(2, 7).All(cell => !tiles[board[cell]].Has<MoveComponent>()),
            "two animated tiles");
        systems.Destroy(); world.Destroy();
    }

    private static void CheckBoardCleanup()
    {
        var world = new EcsWorld();
        world.NewEntity().Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 3 });
        session.BeginRun();
        var systems = new EcsSystems(world).Add(new BoardSetupSystem()).Add(new BoardDestroySystem(new Transform()))
            .OneFrame<GameEndEvent>().Inject(session);
        systems.Init(); systems.Run();
        var boards = (EcsFilter<BoardComponent>)world.GetFilter(typeof(EcsFilter<BoardComponent>));
        boards.GetEntity(0).Get<TileSelectionComponent>();
        boards.GetEntity(0).Get<BoardHistoryComponent>().Undone.Add(new Swap(0, 1));
        world.NewEntity().Get<GameEndEvent>();
        systems.Run();
        Check("cleanup removes board, history and selection even without a view", boards.GetEntitiesCount() == 0
            && world.GetFilter(typeof(EcsFilter<BoardHistoryComponent>)).GetEntitiesCount() == 0
            && world.GetFilter(typeof(EcsFilter<TileSelectionComponent>)).GetEntitiesCount() == 0 && !session.IsRunning,
            "one entity owns all attempt data");
        systems.Run();
        Check("ended run cannot recreate a board", boards.GetEntitiesCount() == 0, "Playing state may still be awaiting navigation");
        session.BeginRun(); systems.Run();
        Check("next run creates fresh history", boards.GetEntitiesCount() == 1
            && boards.GetEntity(0).Get<BoardHistoryComponent>().Moves.Count == 0
            && boards.GetEntity(0).Get<BoardHistoryComponent>().Undone.Count == 0
            && !boards.GetEntity(0).Has<TileSelectionComponent>(), "no old moves, redo stack or selection");
        systems.Destroy(); world.Destroy();
    }

    private static void CheckBoardSetup()
    {
        var world = new EcsWorld();
        world.NewEntity().Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 6 });
        session.BeginRun();
        var systems = new EcsSystems(world).Add(new BoardSetupSystem()).Inject(session);
        systems.Init(); systems.Run(); systems.Run();
        var boards = (EcsFilter<BoardComponent, BoardHistoryComponent>)world.GetFilter(typeof(EcsFilter<BoardComponent, BoardHistoryComponent>));
        var board = boards.Get1(0).State;
        var history = boards.Get2(0);
        Check("setup creates one reproducible ECS board", boards.GetEntitiesCount() == 1 && board.CellCount == 36
            && !board.IsSolved && Snapshot(board).SequenceEqual(Snapshot(SeededShuffle.Create(6, history.Seed))),
            "seed retained on the entity");
        systems.Destroy(); world.Destroy();
    }
}
