using System.Collections.Generic;
using System.IO;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.Systems
{
    public class UISystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EcsFilter<OpenScreenEvent> _openScreenFilter;
        private readonly EcsFilter<CloseScreenEvent> _closeScreenFilter;
        private readonly EcsFilter<CloseAllScreensEvent> _closeAllScreensFilter;

        private readonly Stack<Transform> _mainScreenStack = new();
        private readonly Stack<Transform> _popupStack = new();
        private readonly Dictionary<string, Transform> _screenCache = new();

        private readonly Transform _uiRoot;
        private readonly Transform _popupLayer;
        private bool _useCaching = false;

        public UISystem(Transform uiRoot, Transform popupLayer)
        {
            _uiRoot = uiRoot;
            _popupLayer = popupLayer;
        }

        public void Run()
        {
            HandleOpenScreens();
            HandleCloseScreens();
            HandleCloseAllScreens();
        }

        private void HandleOpenScreens()
        {
            foreach (var i in _openScreenFilter)
            {
                ref var openEvent = ref _openScreenFilter.Get1(i);

                if (IsScreenAlreadyOpen(openEvent.PrefabName))
                {
                    continue;
                }

                Transform screen = GetScreenFromCache(openEvent.PrefabName);
                if (screen == null)
                {
                    screen = InstantiateScreen(openEvent.PrefabName, openEvent.IsPopup);
                    if (_useCaching)
                    {
                        CacheScreen(openEvent.PrefabName, screen);
                    }
                }

                OpenScreen(screen, openEvent.IsPopup, openEvent.InitData);

                _openScreenFilter.GetEntity(i).Destroy();
            }
        }

        private void HandleCloseScreens()
        {
            foreach (var i in _closeScreenFilter)
            {
                ref var closeEvent = ref _closeScreenFilter.Get1(i);

                if (!string.IsNullOrEmpty(closeEvent.PrefabName))
                {
                    CloseSpecificScreen(closeEvent.PrefabName);
                }
                else
                {
                    CloseTopScreen();
                }

                _closeScreenFilter.GetEntity(i).Destroy();
            }
        }

        private void HandleCloseAllScreens()
        {
            foreach (var i in _closeAllScreensFilter)
            {
                CloseAllScreens();

                _closeAllScreensFilter.GetEntity(i).Destroy();
            }
        }

        private bool IsScreenAlreadyOpen(string screenName)
        {
            return FindScreenInStacks(screenName) != null;
        }

        private Transform GetScreenFromCache(string screenName)
        {
            if (_useCaching && _screenCache.TryGetValue(screenName, out var cachedScreen) &&
                cachedScreen.gameObject.activeSelf)
            {
                return cachedScreen;
            }

            return null;
        }

        private Transform InstantiateScreen(string prefabName, bool isPopup)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError($"Screen prefab {prefabName} not found!");
                return null;
            }

            Transform parent = isPopup ? _popupLayer : _uiRoot;
            return Object.Instantiate(prefab, parent).transform;
        }

        private void CacheScreen(string screenName, Transform screen)
        {
            if (!_useCaching || _screenCache.ContainsKey(screenName))
            {
                return;
            }

            _screenCache[screenName] = screen;
        }

        private void OpenScreen(Transform screen, bool isPopup, BasePresenter initData)
        {
            PushScreen(isPopup, screen);

            if (initData != null)
            {
                var view = screen.GetComponent<BaseView>();
                if (view != null)
                {
                    view.Initialize(initData);
                }
            }

            screen.gameObject.SetActive(true);
        }

        private void CloseTopScreen()
        {
            if (_popupStack.Count > 0)
            {
                CloseScreen(_popupStack.Pop());
            }
            else if (_mainScreenStack.Count > 0)
            {
                CloseScreen(_mainScreenStack.Pop());
            }
        }

        private void CloseSpecificScreen(string screenName)
        {
            Transform screenToClose = FindScreenInStacks(screenName);
            if (screenToClose == null)
            {
                Debug.LogWarning($"Screen {screenName} missing.");
                return;
            }

            if (_popupStack.Contains(screenToClose))
            {
                RemoveScreenFromStack(_popupStack, screenToClose);
            }
            else if (_mainScreenStack.Contains(screenToClose))
            {
                RemoveScreenFromStack(_mainScreenStack, screenToClose);
            }

            DestroyOrDeactivateScreen(screenToClose);
        }

        private void CloseAllScreens()
        {
            while (_popupStack.Count > 0)
            {
                DestroyOrDeactivateScreen(_popupStack.Pop());
            }

            while (_mainScreenStack.Count > 0)
            {
                DestroyOrDeactivateScreen(_mainScreenStack.Pop());
            }
        }

        private Transform FindScreenInStacks(string screenName)
        {
            string cleanScreenName = Path.GetFileNameWithoutExtension(screenName);
            foreach (var stack in new[] { _popupStack, _mainScreenStack })
            {
                foreach (var screen in stack)
                {
                    if (screen.gameObject.name.Contains(cleanScreenName))
                    {
                        return screen;
                    }
                }
            }

            return null;
        }

        private void PushScreen(bool isPopup, Transform screen)
        {
            (isPopup ? _popupStack : _mainScreenStack).Push(screen);
        }

        private void RemoveScreenFromStack(Stack<Transform> stack, Transform screenToRemove)
        {
            var tempStack = new Stack<Transform>();

            while (stack.Count > 0)
            {
                var screen = stack.Pop();
                if (screen != screenToRemove)
                {
                    tempStack.Push(screen);
                }
            }

            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }

        private void DestroyOrDeactivateScreen(Transform screen)
        {
            if (_useCaching)
            {
                screen.gameObject.SetActive(false);
            }
            else
            {
                Object.Destroy(screen.gameObject);
            }
        }

        private void CloseScreen(Transform screen)
        {
            if (_popupStack.Contains(screen))
            {
                _popupStack.Pop();
            }
            else if (_mainScreenStack.Contains(screen))
            {
                _mainScreenStack.Pop();
            }

            DestroyOrDeactivateScreen(screen);

            if (_popupStack.Count == 0 && _mainScreenStack.Count > 0)
            {
                _mainScreenStack.Peek().gameObject.SetActive(true);
            }
        }
    }
}