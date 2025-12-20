using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Core.ResourceLoader
{
    public class ResourcesLoader : MonoBehaviour
    {
        /// <summary>
        /// group assets
        /// </summary>
        private readonly Dictionary<string, List<AsyncOperationHandle>> groupHandle = new();

        /// <summary>
        /// single asset
        /// </summary>
        private readonly Dictionary<string, AsyncOperationHandle> keyHandle = new();

        /// <summary>
        /// label - asset address list, used for release 
        /// </summary>
        private readonly Dictionary<string, HashSet<string>> groupAddresses = new();

        /// <summary>
        /// asset cache, step 1: find in cache; step 2: load asset
        /// </summary>
        private readonly Dictionary<string, Object> assetCache = new();

        public static ResourcesLoader instance;

        private void Awake() { instance = this; }

        private IEnumerator LoadGroupAsync(string label, IProgress<float> progress = null)
        {
            if (groupHandle.TryGetValue(label, out var list)) { yield break; }

            list = new List<AsyncOperationHandle>();
            groupHandle[label] = list;

            // 记录这个 group 下的 addresses，用于 UnloadGroup 时清 cache
            if (!groupAddresses.TryGetValue(label, out var addrSet))
            {
                addrSet = new HashSet<string>();
                groupAddresses[label] = addrSet;
            }

            var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(Object));
            list.Add(locationHandle);

            while (!locationHandle.IsDone)
            {
                progress?.Report(locationHandle.PercentComplete * 0.2f); // 给 locations 0~20%
                yield return null;
            }

            if (locationHandle.Status != AsyncOperationStatus.Succeeded)
            {
                groupHandle.Remove(label);
                groupAddresses.Remove(label);
                throw new Exception($"{label} loads in error, msg: {locationHandle.OperationException}");
            }

            var locations = locationHandle.Result;
            int total = locations.Count;
            if (total == 0)
            {
                progress?.Report(1f);
                yield break;
            }

            for (int i = 0; i < total; i++)
            {
                var loc = locations[i];
                string address = loc.PrimaryKey;

                // 记录 address 属于这个 group（即使已缓存也记一下，方便 UnloadGroup 清）
                addrSet.Add(address);

                // 已经缓存就跳过加载，但进度要推进
                if (assetCache.ContainsKey(address))
                {
                    progress?.Report(0.2f + 0.8f * ((i + 1f) / total));
                    continue;
                }

                var assetHandle = Addressables.LoadAssetAsync<Object>(loc);
                list.Add(assetHandle); // ✅ 关键：把每个 assetHandle 也记录进 group list

                while (!assetHandle.IsDone)
                {
                    float baseProgress = i / (float)total;
                    float itemProgress = assetHandle.PercentComplete / total;
                    progress?.Report(0.2f + 0.8f * (baseProgress + itemProgress));
                    yield return null;
                }

                if (assetHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception(
                        $"{label} loads in error, address: {address}, msg:{assetHandle.OperationException}");
                }

                assetCache[address] = assetHandle.Result;
                progress?.Report(0.2f + 0.8f * ((i + 1f) / total));
            }

            progress?.Report(1f);
        }

        private IEnumerator UnloadGroupAsync(string label, bool unloadUnusedAsset = false)
        {
            if (groupAddresses.TryGetValue(label, out var addrSet))
            {
                foreach (var addr in addrSet) { assetCache.Remove(addr); }

                groupAddresses.Remove(label);
            }

            if (groupHandle.TryGetValue(label, out var handles))
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    var h = handles[i];
                    if (h.IsValid())
                        Addressables.Release(h);
                }

                groupHandle.Remove(label);
            }

            if (unloadUnusedAsset)
            {
                yield return Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        private IEnumerator LoadKeyAsync<T>(string key, Action<T> cb) where T : Object
        {
            if (keyHandle.TryGetValue(key, out var existing) && existing.IsValid())
            {
                assetCache[key] = (Object)existing.Result;
                cb?.Invoke(GetAsset<T>(key));
                yield break;
            }

            var handle = Addressables.LoadAssetAsync<object>(key);
            keyHandle[key] = handle;

            while (handle.IsDone == false) { yield return null; }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                keyHandle.Remove(key);
                throw new Exception($"{key} loads in error, msg: {handle.OperationException}");
            }

            assetCache[key] = (Object)handle.Result;

            var t = GetAsset<T>(key);
            cb?.Invoke(t);
        }

        private T GetAsset<T>(string label) where T : Object
        {
            if (assetCache.TryGetValue(label, out var obj))
            {
                if (obj is T t) { return t; }

                if (obj is Texture2D t2d && typeof(T) == typeof(Sprite))
                {
                    Sprite sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                        new Vector2(0.5f, 0.5f));
                    return sprite as T;
                }

                Debug.LogError($"{label} type un-match, type is {obj.GetType()}, is not {typeof(T)}");
            }

            return null;
        }

        private IEnumerator UnloadKeyAsync(string key, bool unloadUnusedAsset = false)
        {
            if (keyHandle.TryGetValue(key, out var existing))
            {
                if (existing.IsValid()) { Addressables.Release(existing); }

                keyHandle.Remove(key);
                assetCache.Remove(key);
            }


            if (unloadUnusedAsset)
            {
                yield return Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        private IEnumerator InstantiateAsync(string key, Transform parent, Action<GameObject> cb)
        {
            var handle = Addressables.InstantiateAsync(key, parent);
            while (handle.IsDone == false) { yield return null; }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception($"{key} loads in error, msg: {handle.OperationException}");
            }

            cb?.Invoke(handle.Result);
        }

        private IEnumerator ReleaseInstance(GameObject go)
        {
            if (go == null) { yield break; }

            Addressables.ReleaseInstance(go);
        }

        private IEnumerator UnloadAllAsset(bool unloadUnusedAsset = false)
        {
            assetCache.Clear();
            groupAddresses.Clear();

            foreach (var kv in groupHandle)
            {
                var list = kv.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    var h = list[i];
                    if (h.IsValid())
                        Addressables.Release(h);
                }

                yield return null;
            }

            groupHandle.Clear();

            foreach (var kv in keyHandle)
            {
                var h = kv.Value;
                if (h.IsValid())
                    Addressables.Release(h);

                yield return null;
            }

            keyHandle.Clear();

            if (unloadUnusedAsset)
            {
                yield return Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        public void LoadGroupAsy(string label, IProgress<float> progress)
        {
            StartCoroutine(LoadGroupAsync(label, progress));
        }

        public void UnloadGroupAsy(string label, bool unloadUnusedAsset = false)
        {
            StartCoroutine(UnloadGroupAsync(label, unloadUnusedAsset));
        }

        public void GetAssetAsync<T>(string key, Action<T> cb) where T : Object
        {
            StartCoroutine(LoadKeyAsync(key, cb));
        }

        public void UnloadAssetAsync(string key, bool unloadUnusedAsset = false)
        {
            StartCoroutine(UnloadKeyAsync(key, unloadUnusedAsset));
        }

        public void InstantiateGo(string key, Transform parent, Action<GameObject> cb)
        {
            StartCoroutine(InstantiateAsync(key, parent, cb));
        }

        public void ReleaseGo(GameObject go) { StartCoroutine(ReleaseInstance(go)); }

        public void UnloadAllAssets() { StartCoroutine(UnloadAllAsset()); }
    }
}