using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Services
{
    /// <summary>Loads Addressable sprites for an owner; everything an owner loaded is released together.</summary>
    public interface ISpriteLoader
    {
        UniTask<Sprite> Load(AssetReferenceSprite sprite, object owner);
        void Release(object owner);
    }
}
