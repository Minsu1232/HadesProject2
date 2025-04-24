using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class IconCacheManager : MonoBehaviour
{
    public static IconCacheManager Instance { get; private set; }
    private Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetIcon(string iconAddress, bool loadIfMissing = true)
    {
        if (iconCache.TryGetValue(iconAddress, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        if (loadIfMissing)
        {
            LoadIconAsync(iconAddress);
        }

        return null;
    }

    public void LoadIconAsync(string iconAddress, System.Action<Sprite> callback = null)
    {
        if (iconCache.TryGetValue(iconAddress, out Sprite cachedSprite))
        {
            callback?.Invoke(cachedSprite);
            return;
        }

        Addressables.LoadAssetAsync<Sprite>(iconAddress).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                iconCache[iconAddress] = handle.Result;
                Debug.Log($"아이콘 로드 성공: {iconAddress}");
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogWarning($"아이콘을 로드할 수 없습니다: {iconAddress}");
                callback?.Invoke(null);
            }
        };
    }
}