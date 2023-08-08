using UnityEngine;
using System.Collections.Generic;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

namespace OpenCVForUnityExample
{
    /// <summary>
    /// MatchShapes Example
    /// http://docs.opencv.org/3.1.0/d5/d45/tutorial_py_contours_more_functions.html
    /// </summary>
    public class MatchShapesExample : MonoBehaviour
    {
        // Use this for initialization
        void Start ()
        {

            Texture2D srcTexture = Resources.Load("test") as Texture2D;
            
            Mat rgb = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC1);
            Utils.texture2DToMat(srcTexture, rgb);

            /*Mat grayImage = new Mat();  //grey color matrix
            Imgproc.cvtColor(rgb, grayImage, Imgproc.COLOR_RGB2GRAY);
            */
            Mat gradThresh = new Mat();  //matrix for threshold 
            Mat hierarchy = new Mat();    //matrix for contour hierachy
            Mat mDilatedMask = new Mat(); 
            List<MatOfPoint> contours = new List<MatOfPoint>();
            //Imgproc.threshold(grayImage,gradThresh, 127,255,0);  global threshold
            Imgproc.adaptiveThreshold(rgb, gradThresh, 255, Imgproc.ADAPTIVE_THRESH_MEAN_C, Imgproc.THRESH_BINARY_INV, 3, 12);  //block size 3
            //Imgproc.dilate(gradThresh, mDilatedMask, new Mat());

            Imgproc.findContours(gradThresh, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));

            for (int i = 0; i < contours.Count; i++)
                Imgproc.drawContours(gradThresh, contours, i, new Scalar(98, 98, 98, 255), 50, 8, hierarchy, 0, new Point());

            for (int i = 0; i < contours.Count; i++)
                Imgproc.drawContours(gradThresh, contours, i, new Scalar(255, 255, 255, 255), 5, 8, hierarchy, 0, new Point());

            Texture2D texture = new Texture2D(gradThresh.cols(), gradThresh.rows(), TextureFormat.BGRA32, false);
            Utils.matToTexture2D(gradThresh, texture);

            texture.Apply();
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            byte[] data = texture.EncodeToPNG();
            Debug.Log("data " + data.Length);
            System.IO.File.WriteAllBytes("1.png", data);

            Debug.Log("contour count : " + contours.Count);

            
            


























            /*
            //srcMat
            Texture2D srcTexture = Resources.Load ("lena") as Texture2D;
            Mat srcMat = new Mat (srcTexture.height, srcTexture.width, CvType.CV_8UC1);
            Mat srcMat1 = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC1);
            Utils.texture2DToMat (srcTexture, srcMat);
            Debug.Log ("srcMat.ToString() " + srcMat.ToString ());
            Imgproc.threshold (srcMat, srcMat, 127, 255, Imgproc.THRESH_BINARY);

            //dstMat
            Texture2D dstTexture = Resources.Load ("lena") as Texture2D;
            Mat dstMat = new Mat (dstTexture.height, dstTexture.width, CvType.CV_8UC3);
            Mat dstMat1 = new Mat(dstTexture.height, dstTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
            Utils.texture2DToMat (dstTexture, dstMat);
            Debug.Log ("dstMat.ToString() " + dstMat.ToString ());
            
            List<MatOfPoint> srcContours = new List<MatOfPoint> ();
            Mat srcHierarchy = new Mat ();

            //Imgproc.adaptiveThreshold(srcMat1, srcMat, 255, Imgproc.ADAPTIVE_THRESH_MEAN_C, Imgproc.THRESH_BINARY_INV, 3, 12);
            /// Find srcContours
            Imgproc.findContours (srcMat, srcContours, srcHierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);
            //Imgproc.findContours(srcMat, srcContours, srcHierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_TC89_KCOS);
            Debug.Log ("srcContours.Count " + srcContours.Count);

            //Imgproc.fillPoly(dstMat1, srcContours, new Scalar(255, 0, 0, 255), 0, 0, new Point());

            for (int i=0; i<srcContours.Count; i++) {

                MatOfPoint mp = srcContours[i];
                
                if (i != srcContours.Count + 1)
                {
                    Debug.Log(mp.width() + "    " + mp.height() + "   " + i);
                    Imgproc.drawContours(dstMat, srcContours, i, new Scalar(255, 0, 0), 3, 8, srcHierarchy, 0, new Point());
                    Imgproc.drawContours(dstMat1, srcContours, i, new Scalar(255, 255, 255, 255), 3, 8, srcHierarchy, 0, new Point());
                    
                }
            }
            */


            /*for (int i=0; i<srcContours.Count; i++) {
                double returnVal = Imgproc.matchShapes (srcContours [1], srcContours [i], Imgproc.CV_CONTOURS_MATCH_I1, 0);
                Debug.Log ("returnVal " + i + " " + returnVal);

                Point point = new Point ();
                float[] radius = new float[1];
                Imgproc.minEnclosingCircle (new MatOfPoint2f (srcContours [i].toArray ()), point, radius);
                Debug.Log ("point.ToString() " + point.ToString ());
                Debug.Log ("radius.ToString() " + radius [0]);
                
                Imgproc.circle (dstMat, point, 5, new Scalar (0, 0, 255), -1);
                Imgproc.putText (dstMat, " " + returnVal, point, Core.FONT_HERSHEY_SIMPLEX, 0.4, new Scalar (0, 255, 0), 1, Imgproc.LINE_AA, false);
            }
            */



            /*
            Texture2D texture = new Texture2D (dstMat.cols (), dstMat.rows (), TextureFormat.BGRA32, false);
            
            //Utils.matToTexture2D (dstMat, texture);
            Utils.matToTexture2D(dstMat1, texture);
            
            texture.Apply();
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
            byte[] data = texture.EncodeToPNG();
            Debug.Log("data "+data.Length);
            Debug.Log(dstTexture.width + " : " + dstTexture.height);
            

            System.IO.File.WriteAllBytes("1.png", data);
            */

        }
        
        // Update is called once per frame
        void Update ()
        {
            
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("OpenCVForUnityExample");
            #else
            Application.LoadLevel ("OpenCVForUnityExample");
            #endif
        }
    }
}