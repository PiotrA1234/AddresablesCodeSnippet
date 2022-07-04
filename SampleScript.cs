using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
//my basic code snippet from my project loading some audio clips from addressables by its type
//also using some lambda expressions
public class SampleScript : MonoBehaviour
{
    public List<GameObject> musicButtons = new List<GameObject>();
    public List<AudioClip> musicClips = new List<AudioClip>();
    public IEnumerator OnEnableMusicModeUI()
    {
        Debug.Log("(OnEnableMusicModeUI)");
        CleanAssetRefsMusic();
        foreach (var item in MusicManager.Instance.soundtracksMenu)
        {
            yield return Load(item);
        }
        foreach (var item in MusicManager.Instance.soundtracksPanic)
        {
            yield return Load(item);
        }
        foreach (var item in MusicManager.Instance.soundtracksCredits)
        {
            yield return Load(item);
        }
    }
    public void CleanAssetRefsMusic()
    {
        Debug.Log("(CleanAssetRefsMusic)");
        foreach (var item in  MusicManager.Instance.soundtracksMenu)
            item.ReleaseAsset();
        foreach (var item in  MusicManager.Instance.soundtracksCredits)
            item.ReleaseAsset();
        foreach (var item in  MusicManager.Instance.soundtracksPanic)
            item.ReleaseAsset();
        musicButtons.Clear();
        musicClips.Clear();
        Resources.UnloadUnusedAssets();
    }
    IEnumerator Load(AssetReference item)
    {
        Debug.Log("(Load) :  " + item);
         var currentOperationHandle = item.LoadAssetAsync<AudioClip>();
            if (currentOperationHandle.IsValid())
            {
                yield return currentOperationHandle;
                if (currentOperationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>(musicButtonPrefab);
                    yield return loadOp;
                    if (loadOp.Status == AsyncOperationStatus.Succeeded)
                    {
                        var op = Addressables.InstantiateAsync(musicButtonPrefab);
                        if (op.IsDone)// <--- this will always be true.  A preloaded asset will instantiate synchronously. 
                        {
                            var musicButton = op.Result.gameObject;
                            musicButton.AddComponent<BtnBase>();
                            var music = currentOperationHandle.Result;
                            musicButtons.Add(musicButton);
                            musicClips.Add(music);
                            string musicName = music.name;
                            var btnBase = musicButton.GetComponent<BtnBase>();
                            ItemFactory.Instance.ShowCard(btnBase, false, musicName,
                                (StatBlockUI) =>
                                {
                                    foreach (Transform child in StatBlockUI.transform.parent)
                                    {
                                        Colorizer childColorizer = child.GetComponentInChildren<Colorizer>();
                                        childColorizer.colorType = Colorizer.ColorTypes.Background;
                                        childColorizer.Recolor();
                                    }
                                    AudioClip clip = musicClips[musicClips.IndexOf(music)];
                                    MusicManager.Instance.PlayCustomSong(clip);
                                }
                            , musicName.Contains("INFERNO")?true : false);
                        }
                    }
                }
            }
    }
}
