# Перед коммитом: seeded shuffle, Undo и replay на ECS

Изменения находятся в `feature/deterministic-board`, поверх `1b66f4e`.
Коммит и push пока не выполнены.

## Что работает в коде

- Начальная раскладка воспроизводится по seed, размеру, скрытому тайлу и числу шагов.
- Состояние партии и история принадлежат ECS-сущности поля.
- Отмена последнего хода возвращает предыдущее состояние и корректирует историю.
- Просмотр истории текущей незавершённой партии идёт на отдельном поле.
  «Стоп» возвращает текущее игровое поле даже посреди анимации.
- После просмотра можно продолжить играть. Энергия и награды просмотром не меняются.
- Существующие тайлы получают координаты из модели; анимация больше не меняет правила игры.
- В игровом экране добавлена панель «Отмена», «Повтор/Стоп», число ходов и seed.

## Изменения по файлам

| Файл | Назначение изменения |
| --- | --- |
| `Assets/Fives/Domain/SeededShuffle.cs` | Фиксированный алгоритм случайного обхода допустимых ходов; достижимая, не собранная раскладка. |
| `Assets/Fives/Domain/ReplayData.cs` | Версия, параметры раскладки и копия ходов; проверка записи перед воспроизведением. |
| `Assets/Scripts/Components/BoardComponent.cs` | Авторитетное состояние игрового поля. |
| `Assets/Scripts/Components/BoardHistoryComponent.cs` | Seed, число шагов перемешивания, начальная пустая клетка и принятые ходы. |
| `Assets/Scripts/Components/BoardReplayComponent.cs` | Временная модель просмотра, запись и текущий индекс хода. |
| `Assets/Scripts/Components/BoardControlEvent.cs` | Команды кнопок Undo, Replay и Stop. |
| `Assets/Scripts/Components/BoardRefreshEvent.cs` | Запрос мгновенно показать начальное/текущее поле при переключении просмотра. |
| `Assets/Scripts/Components/TileComponent.cs` | Убран неиспользуемый `isEmpty`; позиция остаётся только для отображения. |
| `Assets/Scripts/Systems/BoardSetupSystem.cs` | Создание одной ECS-сущности партии с воспроизводимым полем. |
| `Assets/Scripts/Systems/BoardInputSystem.cs` | Одна команда за тик, допустимый ход, Undo, запуск/остановка просмотра; защита на время анимации и выхода. |
| `Assets/Scripts/Systems/BoardReplaySystem.cs` | Пошаговый просмотр после завершения анимаций, автоматический возврат в игру. |
| `Assets/Scripts/Systems/BoardProjectionSystem.cs` | Перенос раскладки в координаты тайлов и запросы анимации. |
| `Assets/Scripts/Systems/TileMoveSystem.cs` | Удалена логическая перестановка; оставлена только анимация. |
| `Assets/Scripts/Systems/WinCheckSystem.cs` | Победа определяется по модели; просмотр не запускает завершение партии. |
| `Assets/Scripts/Systems/BoardDestroySystem.cs` | Удаление ECS-сущности вместе с историей и просмотром при выходе. |
| `Assets/Scripts/GameStartup.cs` | Порядок новых систем и очистка одноразовых событий. |
| `Assets/Scripts/UI/Presenters/GamePlayPresenter.cs` | Отправка ECS-команд кнопок и чтение состояния для панели. |
| `Assets/Scripts/UI/Views/GamePlayView.cs` | Подключение панели управления к существующему экрану. |
| `Assets/Scripts/UI/Views/BoardControlsView.cs` | Небольшая uGUI-панель под превью; используется существующий шрифт. |
| `Assets/Tests/EditMode/SeededShuffleTests.cs` | 18 новых NUnit-кейсов для перемешивания и формата replay. |
| `tools/logic-probes/BoardProbes.cs` | Проверки совместной работы реальных ECS-систем, Undo, replay, очистки и нового старта. |
| `tools/logic-probes/CorrectnessProbes.cs` | Старые сценарии переведены на новую точку входа; усилена проверка быстрых кликов. |
| `tools/logic-probes/LifecycleProbes.cs` | Проверка победы и блокировки ввода теперь использует модель поля. |
| `tools/logic-probes/EngineStubs.cs` | Минимальные замены недоступных вне Unity свойств отображения для логических проверок. |
| `tools/logic-probes/run.py` | Компиляция новых систем и тестового файла вместо удалённых реализаций. |
| `README.md`, `docs/ARCHITECTURE.md`, `docs/BOARD_STATE.md`, `docs/ROADMAP.md`, `tools/domain-tests/README.md` | Описание текущей ECS-архитектуры, контрактов, тестов и границ готовности. |

Удалены `PuzzleGenerator`, `ShuffleSystem`, `TileClickSystem` и `EmptyTileComponent`
вместе с их `.meta`: они заменены новыми системами, второй реализации правил нет.
Новые Unity-файлы имеют `.meta`. `GameSession`, проектные `.csproj`, asmdef и workflow
не изменены. Слой `Fives.Application` не вводится.

## Проверки и ограничения

- 74/74 NUnit-теста прошли в Release.
- 50/50 проверок сервисов/ECS прошли, включая прежние регрессии.
- Ядро, NUnit-assembly и полный runtime компилируются с библиотеками Unity 6000.0.71f1.
- Отдельный запуск Unity Test Runner блокирует открытый экземпляр Editor.
  Управление приложением недоступно без разрешения Computer Use.
- Визуальная проверка панели, PlayMode и Android ещё не выполнены.
  [Порядок ручной проверки](BOARD_STATE.md#manual-acceptance-in-unity).
- Replay пока относится к текущей незавершённой партии; сохранение записей, просмотр
  после экрана результата, импорт и обмен кодом не добавлены.
- GitHub CI для этого diff ещё не запускался: изменения не закоммичены и не отправлены.

Изменения TMP/URP и настроек графики, уже находившиеся в рабочем дереве, исключены
из подготовленного feature-patch. Перед коммитом они требуют отдельного решения.
