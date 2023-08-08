using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTexture : MonoBehaviour {

    public Camera captureCamera;
    // Use this for initialization
    void Start () {


        int resWidthN = 512;
        int resHeightN = 512;
        RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
        captureCamera.targetTexture = rt;

        TextureFormat tFormat;
        tFormat = TextureFormat.ARGB32;


        Texture2D photo = new Texture2D(resWidthN, resHeightN, tFormat, false);
        captureCamera.Render();
        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
        photo.Apply();
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        byte[] bytes = photo.EncodeToPNG();

        System.IO.File.WriteAllBytes("2.png", bytes);
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
