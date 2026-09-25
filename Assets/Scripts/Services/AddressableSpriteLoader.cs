using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Scripts.Services
{
    /// <summary>
    /// Keeps each owner's handles. Addressables counts references per asset and unloads a bundle once none of
    /// its assets is held, so screens sharing a theme bundle release it independently.
    /// </summary>
    public sealed class AddressableSpriteLoader : ISpriteLoader
    {
        private readonly Dictionary<object, List<AsyncOperationHandle<Sprite>>> _handles =
            new Dictionary<object, List<AsyncOperationHandle<Sprite>>>();

        public async UniTask<Sprite> Load(AssetReferenceSprite sprite, object owner)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(sprite.RuntimeKey);
            if (!_handles.TryGetValue(owner, out var handles))
                _handles[owner] = handles = new List<AsyncOperationHandle<Sprite>>();
            handles.Add(handle);
            return await handle.Task;
        }

        public void Release(object owner)
        {
            if (!_handles.Remove(owner, out var handles))
                return;
            foreach (var handle in handles)
                Addressables.Release(handle);
        }
    }
}
