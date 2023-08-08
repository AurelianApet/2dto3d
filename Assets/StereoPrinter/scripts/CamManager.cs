using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DlibFaceLandmarkDetectorExample;
using OpenCVForUnity;
using System.Runtime.InteropServices;
using System;
using System.Text;

public class CamManager : MonoBehaviour {

    public AudioSource audio;
    public Renderer camRender;
    public Image contourImg;
    public GameObject captureBtn;
    public GameObject backBtn;
    public GameObject TDBtn;
    GameObject newTDModel;
    // Use this for initialization
    void Start() {
		Global.processing = false;
		Global.starting = false;
    }

    // Update is called once per frame
    void Update() {

    }

    public void onClickCapture()
    {
        audio.Play();
        //Texture2D capTex = (Texture2D)camRender.material.mainTexture;
        //captureRenderer.sprite = Sprite.Create(capTex, new Rect(0.0f, 0.0f, capTex.width, capTex.height), new Vector2(0.5f, 0.5f), 100.0f);
        camRender.GetComponent<WebCamTextureExample>().stopRender();

        captureBtn.SetActive(false);
        backBtn.SetActive(true);
        TDBtn.SetActive(true);
        //captureRenderer.gameObject.SetActive(true);
    }

    public void onClickBack()
    {
        camRender.GetComponent<WebCamTextureExample>().Run();
        captureBtn.SetActive(true);
        backBtn.SetActive(false);
        TDBtn.SetActive(false);
        contourImg.gameObject.SetActive(false);

        if(newTDModel != null)
            Destroy(newTDModel);
		Global.starting = false;
		Global.processing = false;
    }

    public void onClickTDButton()
    {
        StartCoroutine(TDThread());
        TDBtn.SetActive(false);
    }

    public IEnumerator TDThread()
    {
		Global.starting = true;
		yield return new WaitForSeconds(0.01f);
        StartCoroutine(generateContour());
        /*
         StartCoroutine(loading1());
         StartCoroutine(loading2());
        */
    }
    
    public IEnumerator loading1()
    {
        for(int i=0;i<10;i++)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("loading1");
        }
    }

    public IEnumerator loading2()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1.5f);
            Debug.Log("loading2");
        }
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }

	public Texture2D sample;
	[DllImport("BGREMOVE")]
	private static extern IntPtr fnbgRemove(StringBuilder imgBase64, int iterations, int sample);

    public IEnumerator generateContour()
    {
        Debug.Log("model config start");
		//Texture2D srcTexture = new Texture2D(sample.width, sample.height, TextureFormat.RGB24, false);
		//srcTexture.SetPixels (sample.GetPixels ());
		Texture2D srcTexture = (Texture2D)camRender.material.mainTexture;
		if (srcTexture.width > 4000 || srcTexture.height > 4000) {
			srcTexture = ScaleTexture(srcTexture, srcTexture.width / 20, srcTexture.height / 20);
		}else if (srcTexture.width > 2000 || srcTexture.height > 2000) {
			srcTexture = ScaleTexture(srcTexture, srcTexture.width / 10, srcTexture.height / 10);
		}else if (srcTexture.width > 1000 || srcTexture.height > 1000) {
			srcTexture = ScaleTexture(srcTexture, srcTexture.width / 5, srcTexture.height / 5);
		}else if (srcTexture.width > 500 || srcTexture.height > 500) {
			srcTexture = ScaleTexture(srcTexture, srcTexture.width / 2, srcTexture.height / 2);
		}
		srcTexture.Apply ();

		/////////////////cpp dll//////////////////////////
		StringBuilder img64 = new StringBuilder(Convert.ToBase64String(srcTexture.EncodeToPNG()));
		IntPtr result = fnbgRemove(img64, 1, 0);
		//Marshal.FreeHGlobal(ptrCamData);
		byte[] pro_image = Convert.FromBase64String( Marshal.PtrToStringAnsi (result).ToString());
		srcTexture.LoadImage (pro_image);
		srcTexture.Apply ();
		////////////Unity opencv/////////////////
		Mat rgb = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC1);
		Utils.texture2DToMat(srcTexture, rgb);
		Mat gradThresh = new Mat();  //matrix for threshold 
		Mat hierarchy = new Mat();    //matrix for contour hierachy
		Mat mDilatedMask = new Mat();
		List<MatOfPoint> contours = new List<MatOfPoint>();
		//Imgproc.threshold(grayImage,gradThresh, 127,255,0);  global threshold
		Imgproc.adaptiveThreshold(rgb, gradThresh, 255, Imgproc.ADAPTIVE_THRESH_MEAN_C, Imgproc.THRESH_BINARY_INV, 3, 12);  //block size 3
		Imgproc.findContours(gradThresh, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));
		for (int i = 0; i < contours.Count; i++)
			Imgproc.drawContours(gradThresh, contours, i, new Scalar(98, 98, 98, 255), 50, 8, hierarchy, 0, new Point());
		for (int i = 0; i < contours.Count; i++)
			Imgproc.drawContours(gradThresh, contours, i, new Scalar(255, 255, 255, 255), 5, 8, hierarchy, 0, new Point());
		Texture2D texture = new Texture2D(gradThresh.cols(), gradThresh.rows(), TextureFormat.BGRA32, false);
		Utils.matToTexture2D(gradThresh, texture);
		texture.Apply();
		//grabcut function here
		Texture2D imageTexture = srcTexture;
		Mat image = new Mat(imageTexture.height, imageTexture.width, CvType.CV_8UC3);
		Utils.texture2DToMat(imageTexture, image);
		Debug.Log("image.ToString() " + image.ToString());
		Texture2D maskTexture = texture;
		Mat mask = new Mat(imageTexture.height, imageTexture.width, CvType.CV_8UC1);
		//Utils.texture2DToMat(maskTexture, mask);
		Debug.Log("mask.ToString() " + mask.ToString());
		OpenCVForUnity.Rect rectangle = new OpenCVForUnity.Rect(10, 10, image.cols() - 10, image.rows() - 10);
		Mat bgdModel = new Mat(); // extracted features for background
		Mat fgdModel = new Mat(); // extracted features for foreground
		convertToGrabCutValues(mask); // from grayscale values to grabcut values 
		int iterCount = 1;
		Imgproc.grabCut (image, mask, rectangle, bgdModel, fgdModel, iterCount, Imgproc.GC_INIT_WITH_RECT);
		//Imgproc.grabCut(image, mask, rectangle, bgdModel, fgdModel, iterCount, Imgproc.GC_INIT_WITH_MASK);
		convertToGrayScaleValues(mask); // back to grayscale values
		Imgproc.threshold(mask, mask, 128, 255, Imgproc.THRESH_TOZERO);
		Mat foreground = new Mat(image.size(), CvType.CV_8UC3, new Scalar(0, 0, 0));
		image.copyTo(foreground, mask);
		Texture2D texture1 = new Texture2D(image.cols(), image.rows(), TextureFormat.RGBA32, false);
		Utils.matToTexture2D(foreground, texture1);
		for (int i = 0; i < texture1.width; i++)
			for (int j = 0; j < texture1.height; j++)
				if (texture1.GetPixel(i, j).r == 0 && texture1.GetPixel(i, j).g == 0 && texture1.GetPixel(i, j).b == 0)
					texture1.SetPixel(i, j, new Color(0, 0, 0, 0));
		texture1.Apply();
		contourImg.sprite = Sprite.Create(texture1, new UnityEngine.Rect(0.0f, 0.0f, texture1.width, texture1.height), new Vector2(0.5f, 0.5f), 100.0f);
		//contourImg.gameObject.SetActive(true);
		Debug.Log("end here!");
        //System.IO.File.WriteAllBytes(Application.dataPath + "/2.png", texture.EncodeToPNG());
        //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/2.png", texture.EncodeToPNG());
		newTDModel = Instantiate(tdModel);
        newTDModel.name = "TDModel";
		newTDModel.GetComponent<MeshCreatorManager>().cutDog = texture1;
		newTDModel.GetComponent<MeshCreatorManager>().outlineTexture = texture1;
        newTDModel.GetComponent<MeshCreatorManager>().CreateMesh();
		Global.processing = true;
        yield return null;
    }

    private void convertToGrayScaleValues(Mat mask)    {
        int width = mask.rows();
        int height = mask.cols();
        byte[] buffer = new byte[width * height];
        mask.get(0, 0, buffer);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int value = buffer[y * width + x];

                if (value == Imgproc.GC_BGD)
                {
                    buffer[y * width + x] = 0; // for sure background
                }
                else if (value == Imgproc.GC_PR_BGD)
                {
                    buffer[y * width + x] = 85; // probably background
                }
                else if (value == Imgproc.GC_PR_FGD)
                {
                    buffer[y * width + x] = (byte)170; // probably foreground
                }
                else
                {
                    buffer[y * width + x] = (byte)255; // for sure foreground
                }
            }
        }
        mask.put(0, 0, buffer);
    }

    private void convertToGrabCutValues(Mat mask)
    {
        int width = mask.rows();
        int height = mask.cols();
        byte[] buffer = new byte[width * height];
        mask.get(0, 0, buffer);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int value = buffer[y * width + x];
                if (value >= 0 && value < 64)
                {
                    buffer[y * width + x] = Imgproc.GC_BGD; // for sure background
                }
                else if (value >= 64 && value < 128)
                {
                    buffer[y * width + x] = Imgproc.GC_PR_BGD; // probably background
                }
                else if (value >= 128 && value < 192)
                {
                    buffer[y * width + x] = Imgproc.GC_PR_FGD; // probably foreground
                }
                else
                {
                    buffer[y * width + x] = Imgproc.GC_FGD; // for sure foreground
                }
            }
        }
        mask.put(0, 0, buffer);
    }

    public GameObject tdModel;
}
