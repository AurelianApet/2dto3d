using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

public class MeshCreatorManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //CreateMesh();
        /*
        for (int i = 0; i < 4; i++)
        {


            GameObject cloneobj = Instantiate(temp) as GameObject;
            cloneobj.transform.parent = parent;
        }*/
    }
	
	// Update is called once per frame
	void Update () {
		
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

    public void CreateMesh()
    {
        //first comment
        /*Debug.Log(cutDog.width + "   " + cutDog.height);
        //convert Texture Size
        if(cutDog.width > cutDog.height)
        {
            int calc_height = (int)(((float)cutDog.height / (float)cutDog.width) * 400);
            int calc_width = 400;
            Debug.Log(calc_width + "    " + calc_height);
            cutDog.Resize(calc_width, calc_height, TextureFormat.RGB24, true);
        }
        else
        {
            int calc_width = (int)(((float)cutDog.width / (float)cutDog.height) * 400);
            int calc_height = 400;
            Debug.Log(calc_width + "    " + calc_height);
            cutDog.Resize(calc_width, calc_height, TextureFormat.RGB24, true);
        }
        cutDog.Apply();
        
        byte[] dogData = cutDog.EncodeToPNG();
        System.IO.File.WriteAllBytes("cd.png", dogData);*/

        //srcMat
        /*Texture2D srcTexture = cutDog;
        Mat srcMat = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC1);
        Mat srcMat1 = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC1);
        Utils.texture2DToMat(srcTexture, srcMat);
        Debug.Log("srcMat.ToString() " + srcMat.ToString());
        Imgproc.threshold(srcMat, srcMat, 127, 255, Imgproc.THRESH_BINARY);

        //dstMat
        Texture2D dstTexture = cutDog;
        Mat dstMat = new Mat(dstTexture.height, dstTexture.width, CvType.CV_8UC3);
        Mat dstMat1 = new Mat(dstTexture.height, dstTexture.width, CvType.CV_8UC4, new Scalar(255, 255, 255, 0));
        Utils.texture2DToMat(dstTexture, dstMat);
        Debug.Log("dstMat.ToString() " + dstMat.ToString());


        List<MatOfPoint> srcContours = new List<MatOfPoint>();
        Mat srcHierarchy = new Mat();

        /// Find srcContours
        //Imgproc.findContours (srcMat, srcContours, srcHierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_NONE);
        Imgproc.findContours(srcMat, srcContours, srcHierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_TC89_KCOS);
        Debug.Log("srcContours.Count " + srcContours.Count);

        for (int i = 0; i < srcContours.Count; i++)
        {

            MatOfPoint mp = srcContours[i];

            if (i != srcContours.Count + 1)
            {
                //Debug.Log(mp.width() + "    " + mp.height() + "   " + i);
                Imgproc.drawContours(dstMat, srcContours, i, new Scalar(255, 0, 0), 3, 8, srcHierarchy, 0, new Point());
                Imgproc.drawContours(dstMat1, srcContours, i, new Scalar(0, 0, 0, 255), 3, 8, srcHierarchy, 0, new Point());
            }
        }
        
        Texture2D texture = new Texture2D(dstMat.cols(), dstMat.rows(), TextureFormat.BGRA32, false);

        //Utils.matToTexture2D (dstMat, texture);
        Utils.matToTexture2D(dstMat1, texture);
        
        /*for (int i = 0; i < texture.width; i++)
            for (int j = 0; j < 3; j++)
                texture.SetPixel(i, j, new Color(1, 1, 1, 0));

        for (int i = 0; i < texture.width; i++)
            for (int j = 0; j < 3; j++)
                texture.SetPixel(i, texture.height - j - 1, new Color(1, 1, 1, 0));

        for (int i = 0; i < texture.height; i++)
            for (int j = 0; j < 3; j++)
                texture.SetPixel(j, i, new Color(1, 1, 1, 0));

        for (int i = 0; i < texture.height; i++)
            for (int j = 0; j < 3; j++)
                texture.SetPixel(texture.height - j - 1, i, new Color(1, 1, 1, 0));*/

        /*texture.Apply();
        byte[] data = texture.EncodeToPNG();
        Debug.Log("data " + data.Length);
        Debug.Log(dstTexture.width + " : " + dstTexture.height);


        System.IO.File.WriteAllBytes("1.png", data);*/

        //second comment
        /*Debug.Log(cutDog.width + " : " + cutDog.height);
        Texture2D scaleTex = ScaleTexture(cutDog, cutDog.width / 2, cutDog.height / 2);
        System.IO.File.WriteAllBytes("cd.png", scaleTex.EncodeToPNG());
        */

        MeshCreatorData meshData = this.GetComponent<MeshCreatorData>();
        meshData.outlineTexture = outlineTexture;
        MeshCreator.UpdateMesh(meshData.gameObject,baseMat,transMat,1, transMat, cutDog);
    }

    public Material baseMat;
    public Material transMat;
    public Material chroMat;

    public GameObject temp;
    public Transform parent;

    public Texture2D cutDog;
    public Texture2D outlineTexture;
}
