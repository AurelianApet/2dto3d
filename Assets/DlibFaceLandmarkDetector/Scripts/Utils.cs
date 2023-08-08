using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

namespace DlibFaceLandmarkDetector
{
    public static class Utils
    {
        /**
        * Returns this "Dlib FaceLandmark Detector" version number.
        * 
        * @return this "Dlib FaceLandmark Detector" version number
        */
        public static string getVersion ()
        {
            return "1.1.8";
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder. e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android]The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copyied. 
        * <br>[WebGL]If the target file has not yet been copied to WebGL's virtual filesystem, you need to use getFilePathAsync() at first.
        * 
        * @param filepath a relative file path starting from "StreamingAssets" folder
        * @param refresh [Android]If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copyied.
        * @return returns the file path in case of success and returns empty in case of error.
        */
        public static string getFilePath (string filepath, bool refresh = false)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            filepath = filepath.TrimStart (chTrims);

            string srcPath = Path.Combine(Application.streamingAssetsPath, filepath);
            string destPath = Path.Combine(Application.persistentDataPath, "dlibfacelandmarkdetector");
            destPath = Path.Combine(destPath, filepath);

            if (!refresh && File.Exists(destPath))
                return destPath;

            using (WWW request = new WWW (srcPath)) {
                while (!request.isDone) {;}

                if (!string.IsNullOrEmpty(request.error)) {
                    Debug.LogWarning (request.error);
                    return String.Empty;
                }

                //create Directory
                String dirPath = Path.GetDirectoryName (destPath);
                if (!Directory.Exists (dirPath))
                    Directory.CreateDirectory (dirPath);

                File.WriteAllBytes (destPath, request.bytes);
            }

            return destPath;
            #elif UNITY_WEBGL && !UNITY_EDITOR
            filepath = filepath.TrimStart (chTrims);

            string destPath = Path.Combine(Path.AltDirectorySeparatorChar.ToString(), "dlibfacelandmarkdetector");
            destPath = Path.Combine(destPath, filepath);

            if (File.Exists(destPath)){
                return destPath;
            }else{
                return String.Empty;
            }
            #else
            filepath = filepath.TrimStart (chTrims);

            string destPath = Path.Combine (Application.streamingAssetsPath, filepath);

            if (File.Exists (destPath)) {
                return destPath;
            } else {
                return String.Empty;
            }
            #endif
        }

        /**
        * Gets the readable path of a file in the "StreamingAssets" folder by using coroutines.
        * <p>
        * <br>Set a relative file path from the starting point of the "StreamingAssets" folder.  e.g. "foobar.txt" or "hogehoge/foobar.txt".
        * <br>[Android]The target file that exists in the "StreamingAssets" folder is copied into the folder of the Application.persistentDataPath. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copyied. 
        * <br>[WebGL]The target file in the "StreamingAssets" folder is copied to the WebGL's virtual filesystem. If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copyied. 
        * 
        * @param filepath a relative file path starting from "StreamingAssets" folder
        * @param completed a callback function that is called when process is completed. Returns the file path in case of success and returns empty in case of error.
        * @param progress a callback function that is called when process is progress. Returns the file path and a value of 0 to 1.
        * @param refresh [Android][WebGL]If refresh flag is false, when the file has already been copied, the file is not copied. If refresh flag is true, the file is always copyied.
        */
        public static IEnumerator getFilePathAsync (string filepath, Action<string> completed, Action<string, float> progress = null, bool refresh = false)
        {
            #if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
            filepath = filepath.TrimStart (chTrims);

            string srcPath = Path.Combine(Application.streamingAssetsPath, filepath);
            #if UNITY_ANDROID
            string destPath = Path.Combine(Application.persistentDataPath, "dlibfacelandmarkdetector");
            #else
            string destPath = Path.Combine(Path.AltDirectorySeparatorChar.ToString(), "dlibfacelandmarkdetector");
            #endif
            destPath = Path.Combine(destPath, filepath);

            if (!refresh && File.Exists(destPath)){
                if (progress != null)
                    progress(destPath, 0);
                yield return null;
                if (progress != null)
                    progress(destPath, 1);
                if (completed != null)
                    completed (destPath);
            } else {
            #if UNITY_WEBGL && UNITY_5_4_OR_NEWER
                using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get (srcPath)) {
                    request.Send ();
                    while (!request.isDone) {
                        if (progress != null)
                        progress(filepath, request.downloadProgress);

                        yield return null;
                    }

            #if UNITY_2017 && UNITY_2017_1_OR_NEWER
                    if (request.isHttpError || request.isNetworkError) {
            #else
                    if (request.isError) {
            #endif
                        Debug.LogWarning (request.error);
                        if (completed != null)
                            completed (String.Empty);
                    }

                    //create Directory
                    String dirPath = Path.GetDirectoryName (destPath);
                    if (!Directory.Exists (dirPath))
                        Directory.CreateDirectory (dirPath);

                    File.WriteAllBytes (destPath, request.downloadHandler.data);
                }
            #else
                using (WWW request = new WWW (srcPath)) {

                    while (!request.isDone) {
                        if (progress != null)
                            progress(filepath, request.progress);

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(request.error)) {
                        Debug.LogWarning (request.error);
                            if (completed != null)
                                completed (String.Empty);
                    }

                    //create Directory
                    String dirPath = Path.GetDirectoryName (destPath);
                    if (!Directory.Exists (dirPath))
                        Directory.CreateDirectory (dirPath);

                    File.WriteAllBytes(destPath, request.bytes);
                }
            #endif

                    if (completed != null) completed (destPath);
            }
            #else
            filepath = filepath.TrimStart (chTrims);

            string destPath = Path.Combine (Application.streamingAssetsPath, filepath);

            if (File.Exists (destPath)) {
                if (progress != null)
                    progress (destPath, 0);
                yield return null;
                if (progress != null)
                    progress (destPath, 1);
                if (completed != null)
                    completed (destPath);
            } else {
                if (progress != null)
                    progress (String.Empty, 0);
                yield return null;
                if (completed != null)
                    completed (String.Empty);
            }
            #endif

            yield break;
        }

        private static char[] chTrims = {
            '.',
            #if UNITY_WINRT_8_1 && !UNITY_EDITOR
            '/',
            '\\'
            #else
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
            #endif
        };

        /**
        * Sets the debug mode.
        * <p>
        * <br>if true, The error log of the Native side DlibFaceLandmarkDetector will be displayed on the Unity Editor Console.
        * <br>This method is supported in WIN, MAC and LINUX.
        * <br>Please use as follows.
        * <br>Utils.setDebugMode(true);
        * <br>aaa
        * <br>bbb
        * <br>ccc
        * <br>Utils.setDebugMode(false);
        * 
        * @param debugMode if true, The error log of the Native side DlibFaceLandmarkDetector will be displayed on the Unity Editor Console
        */
        public static void setDebugMode (bool debugMode)
        {
            #if (UNITY_PRO_LICENSE || UNITY_5 || UNITY_5_3_OR_NEWER) && (UNITY_STANDALONE || UNITY_EDITOR)
            DlibFaceLandmarkDetector_SetDebugMode (debugMode);

            if (debugMode) {
                DlibFaceLandmarkDetector_SetDebugLogFunc (debugLogFunc);
                //DlibFaceLandmarkDetector_DebugLogTest ();

            } else {
                DlibFaceLandmarkDetector_SetDebugLogFunc (null);
            }
            #endif
        }


        #if (UNITY_PRO_LICENSE || UNITY_5 || UNITY_5_3_OR_NEWER) && (UNITY_STANDALONE || UNITY_EDITOR)

        private delegate void DebugLogDelegate (string str);

        private static DebugLogDelegate debugLogFunc = msg => Debug.LogError (msg);

        [DllImport ("dlibfacelandmarkdetector")]
        private static extern void DlibFaceLandmarkDetector_SetDebugMode (bool flag);

        [DllImport ("dlibfacelandmarkdetector")]
        private static extern void DlibFaceLandmarkDetector_SetDebugLogFunc (DebugLogDelegate func);

        [DllImport ("dlibfacelandmarkdetector")]
        private static extern void DlibFaceLandmarkDetector_DebugLogTest ();
        #endif
    }
}