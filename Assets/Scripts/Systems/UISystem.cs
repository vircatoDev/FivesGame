using System.Collections.Generic;
using System.IO;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.Systems
{
    /// <summary>Opens and closes screen prefabs on two layers: main screens and popups.</summary>
    public class UISystem : IEcsRunSystem
    {
        private readonly EcsFilter<OpenScreenEvent> _openScreenFilter;
        private readonly EcsFilter<CloseScreenEvent> _closeScreenFilter;
        private readonly EcsFilter<CloseAllScreensEvent> _closeAllScreensFilter;

        private readonly Stack<Transform> _mainScreenStack = new();
        private readonly Stack<Transform> _popupStack = new();

        private readonly Transform _uiRoot;
        private readonly Transform _popupLayer;

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

                if (FindScreenInStacks(openEvent.PrefabName) != null)
                    continue;

                var screen = InstantiateScreen(openEvent.PrefabName, openEvent.IsPopup);
                if (screen == null)
                    continue;

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
                    CloseSpecificScreen(closeEvent.PrefabName);
                else
                    CloseTopScreen();

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

        private Transform InstantiateScreen(string prefabName, bool isPopup)
        {
            var prefab = Resources.Load<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError($"Screen prefab {prefabName} not found!");
                return null;
            }

            var parent = isPopup ? _popupLayer : _uiRoot;
            return Object.Instantiate(prefab, parent).transform;
        }

        private void OpenScreen(Transform screen, bool isPopup, BasePresenter initData)
        {
            (isPopup ? _popupStack : _mainScreenStack).Push(screen);

            if (initData != null)
            {
                var view = screen.GetComponent<BaseView>();
                if (view != null)
                    view.Initialize(initData);
            }

            screen.gameObject.SetActive(true);
        }

        private void CloseTopScreen()
        {
            var stack = _popupStack.Count > 0 ? _popupStack : _mainScreenStack;
            if (stack.Count > 0)
                Object.Destroy(stack.Pop().gameObject);
        }

        private void CloseSpecificScreen(string screenName)
        {
            var screen = FindScreenInStacks(screenName);
            if (screen == null)
            {
                Debug.LogWarning($"Screen {screenName} missing.");
                return;
            }

            RemoveScreenFromStack(_popupStack.Contains(screen) ? _popupStack : _mainScreenStack, screen);
            Object.Destroy(screen.gameObject);
        }

        private void CloseAllScreens()
        {
            while (_popupStack.Count > 0)
                Object.Destroy(_popupStack.Pop().gameObject);

            while (_mainScreenStack.Count > 0)
                Object.Destroy(_mainScreenStack.Pop().gameObject);
        }

        private Transform FindScreenInStacks(string screenName)
        {
            var cleanScreenName = Path.GetFileNameWithoutExtension(screenName);
            foreach (var stack in new[] { _popupStack, _mainScreenStack })
            {
                foreach (var screen in stack)
                {
                    if (screen.gameObject.name.Contains(cleanScreenName))
                        return screen;
                }
            }

            return null;
        }

        private static void RemoveScreenFromStack(Stack<Transform> stack, Transform screenToRemove)
        {
            var kept = new Stack<Transform>();
            while (stack.Count > 0)
            {
                var screen = stack.Pop();
                if (screen != screenToRemove)
                    kept.Push(screen);
            }

            while (kept.Count > 0)
                stack.Push(kept.Pop());
        }
    }
}
