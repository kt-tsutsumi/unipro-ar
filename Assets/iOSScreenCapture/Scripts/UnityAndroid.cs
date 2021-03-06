﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class UnityAndroid : MonoBehaviour
{
    //[SerializeField] private GameObject canvas;
    private string imageName = "";


	void Start () {
		
	}


	void Update()
	{
		if (Input.GetMouseButtonDown (0)) {
			//写真をとる瞬間だけUI非表示にする
			//シャッター音
			var mediaActionSound = new AndroidJavaObject("android.media.MediaActionSound");
			mediaActionSound.Call("play", mediaActionSound.GetStatic<int>("SHUTTER_CLICK"));

			StartCoroutine ("Captcha");
		}
	}

    IEnumerator Captcha() {

        //画像名
        imageName = System.DateTime.Now.ToString ("gyyyyMMddtthhmmssfff") + ".png";

        //ios,Android時パスを追加
#if !UNITY_EDITOR && UNITY_ANDROID && !UNITY_IOS
        Scan(imageName);
#endif
        yield return new WaitForEndOfFrame();

        //スクリーンショットをとる
        ScreenCapture.CaptureScreenshot(imageName);
        //canvas.SetActive(true);

#if !UNITY_EDITOR && UNITY_ANDROID && !UNITY_IOS
        var tex = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels (new Rect (0, 0, Screen.width, Screen.height), 0, 0);
        using (FileStream BinaryFile = new FileStream (imageName, FileMode.Create, FileAccess.Write)) 
        {
            using (BinaryWriter Writer = new BinaryWriter (BinaryFile)) 
            {
                Writer.Write (tex.EncodeToPNG ());
            }
        }

#endif
        yield return new WaitForEndOfFrame ();
        //保存まで待機
        float latency = 0, latencyLimit=2;
        while (latency < latencyLimit) {
            //ファイルが存在していればループ終了
            if (System.IO.File.Exists (imageName)) {
                break;
            }
            latency += Time.deltaTime;
            yield return null;
        }

        //メディアスキャンをする
        ScanMedia (imageName);

    }

    void ScanMedia(string fileName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        //メディアスキャン
#if !UNITY_EDITOR && UNITY_ANDROID && !UNITY_IOS
        using (AndroidJavaClass jcUnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject joActivity = jcUnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity"))
        using (AndroidJavaObject joContext = joActivity.Call<AndroidJavaObject> ("getApplicationContext"))
        using (AndroidJavaObject jcMediaScannerConnection = new AndroidJavaClass ("android.media.MediaScannerConnection"))
        using (AndroidJavaClass jcEnvironment = new AndroidJavaClass ("android.os.Environment"))
        using (AndroidJavaObject joExDir = jcEnvironment.CallStatic<AndroidJavaObject> ("getExternalStorageDirectory")) 
        {
            jcMediaScannerConnection.CallStatic ("scanFile", joContext, new string[] { fileName }, new string[] { "image/png" }, null);
        }
        Handheld.StopActivityIndicator();
#endif
    }

    //Android端末内のディレクトリを読み込んで、オリジナル保存フォルダを生成する
    void Scan(string fileName)
    {
#if !UNITY_EDITOR && UNITY_ANDROID && !UNITY_IOS
        using (AndroidJavaClass jcUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject joActivity = jcUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject joContext = joActivity.Call<AndroidJavaObject>("getApplicationContext"))
        using (AndroidJavaObject jcMediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection"))
        using (AndroidJavaClass jcEnvironment = new AndroidJavaClass("android.os.Environment"))
        using (AndroidJavaObject joExDir = jcEnvironment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
        {
        string imageNameDirectory = joExDir.Call<string>("toString") + "/DCIM/Unipro/";
            if (!Directory.Exists(imageNameDirectory))
            {
                System.IO.Directory.CreateDirectory(imageNameDirectory);
                return;
            }
            imageName = joExDir.Call<string>("toString") + "/DCIM/Unipro/" + fileName;
        }
#endif   
    }
}