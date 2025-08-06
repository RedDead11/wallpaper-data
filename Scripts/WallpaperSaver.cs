using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using NativeGalleryNamespace;

public class WallpaperSaver : MonoBehaviour
{
    public static WallpaperSaver Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SaveMedia(string url, string fileName, string type)
    {
        if (type.ToLower() == "static")
            StartCoroutine(DownloadAndSaveImage(url, fileName));
        else if (type.ToLower() == "live")
            StartCoroutine(DownloadAndSaveVideo(url, fileName));
    }

    private IEnumerator DownloadAndSaveImage(string imageUrl, string fileName)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            byte[] imageData = tex.EncodeToJPG();
            string path = Path.Combine(Application.temporaryCachePath, fileName + ".jpg");
            File.WriteAllBytes(path, imageData);

            NativeGallery.SaveImageToGallery(path, "Wallpapers", fileName + ".jpg");
            Debug.Log("Image saved to gallery: " + path);
        }
        else
        {
            Debug.LogError("Image download failed: " + request.error);
        }
    }

    private IEnumerator DownloadAndSaveVideo(string videoUrl, string fileName)
    {
        string path = Path.Combine(Application.temporaryCachePath, fileName + ".mov");
        UnityWebRequest request = UnityWebRequest.Get(videoUrl);
        request.downloadHandler = new DownloadHandlerFile(path);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            NativeGallery.SaveVideoToGallery(path, "Wallpapers", fileName + ".mov");
            Debug.Log("Video saved to gallery: " + path);
        }
        else
        {
            Debug.LogError("Video download failed: " + request.error);
        }
    }
}
