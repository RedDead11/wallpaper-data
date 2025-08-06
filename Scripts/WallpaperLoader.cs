using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class Wallpaper
{
    public string id;
    public string title;
    public string type; // "static" or "live"
    public string url;
}

public class WallpaperLoader : MonoBehaviour
{
    public string jsonUrl = "https://tubular-lolly-ca2086.netlify.app/wallpapers.json";

    public Transform wallpaperContainer;
    public GameObject wallpaperCardPrefab;

    private void Start()
    {
        StartCoroutine(DownloadWallpapers());
    }

    IEnumerator DownloadWallpapers()
    {
        UnityWebRequest www = UnityWebRequest.Get(jsonUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load JSON: " + www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            var wallpapers = JsonConvert.DeserializeObject<List<Wallpaper>>(json);

            foreach (var wallpaper in wallpapers)
            {
                StartCoroutine(LoadWallpaperCard(wallpaper));
            }
        }
    }

    IEnumerator LoadWallpaperCard(Wallpaper data)
    {
        GameObject card = Instantiate(wallpaperCardPrefab, wallpaperContainer);
        card.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = data.title;

        // Get image and video containers
        var imageGO = card.transform.Find("RawImage").gameObject;
        var videoGO = card.transform.Find("Video").gameObject;

        if (data.type.ToLower() == "static")
        {
            imageGO.SetActive(true);
            videoGO.SetActive(false);

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(data.url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                imageGO.GetComponent<RawImage>().texture = tex;

                // Save button
                card.GetComponent<Button>().onClick.AddListener(() =>
                {
                    NativeGallery.SaveImageToGallery(tex, "Wallpapers", data.title + ".jpg");
                });
            }
            else
            {
                Debug.LogError("Failed to load static image: " + request.error);
            }
        }
        else if (data.type.ToLower() == "live")
        {
            imageGO.SetActive(false);
            videoGO.SetActive(true);

            var videoPlayer = videoGO.GetComponent<VideoPlayer>();
            var videoRawImage = videoGO.GetComponent<RawImage>();

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = data.url;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.targetTexture = new RenderTexture(512, 512, 0);

            videoRawImage.texture = videoPlayer.targetTexture;

            videoPlayer.isLooping = true;
            videoPlayer.Play();

            // Optional: Saving live videos isn't supported directly, so show message
            card.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Saving live wallpapers not supported on iOS directly.");
            });
        }
    }
}
