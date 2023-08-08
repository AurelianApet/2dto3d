

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

    public class MeshCreator : UnityEngine.Object
    {
        public static float versionNumber = 0.7f;

        public static void UpdateMesh(GameObject gameObject)
        {
            MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;

            // unity should prevent this from happening to the inspector, but just in case.....
            if (mcd == null)
            {
                Debug.LogError("MeshCreator Error: selected object does not have a MeshCreatorData component. Select an object with a MeshCreatorData component to update.");
                return;
            }

            // add a TextureImporter object here to check whether texture is readable
            // set it to readable if necessary
            if (mcd.outlineTexture == null)
            {
                Debug.LogError("MeshCreator: no texture found. Make sure to have a texture selected before updating mesh.");
                return;
            }

            // if this is a new save, generate a unique idNumber
            if (mcd.idNumber == "")
            {
                mcd.idNumber = MeshCreator.GenerateId();
                // Debug.Log(mcd.gameObject.name + "MeshCreator: set new mesh id number to " + mcd.idNumber);
            }

            // check the id number, if it is used in another scene object
            // generate a new id number
            while (MeshCreator.IdExistsInScene(mcd))
            {
                mcd.idNumber = MeshCreator.GenerateId();
            }

            // check for scene folder
            string[] sceneNames = EditorApplication.currentScene.Split('/');
            if (sceneNames.Length == 1 && sceneNames[0] == "")
            {
                Debug.LogError("MeshCreator Error: please save the scene before creating a mesh.");
                DestroyImmediate(mcd.gameObject);
                return;
            }
            string sceneName = sceneNames[sceneNames.Length - 1];
            string folderName = sceneName.Substring(0, sceneName.Length - 6);
            string folderPath = "Assets/UCLAGameLab/Meshes/" + folderName; // TODO: this should be a preference

            if (!Directory.Exists("Assets/UCLAGameLab/Meshes"))
            {
                if (!Directory.Exists("Assets/UCLAGameLab"))
                {
                    Debug.LogError("MeshCreator: UCLAGameLab folder is missing from your project, please reinstall Mesh Creator.");
                    return;
                }
                AssetDatabase.CreateFolder("Assets/UCLAGameLab", "Meshes");
                Debug.Log("MeshCreator: making new Meshes folder at Assets/Meshes");
            }

            if (!Directory.Exists(folderPath))
            {
                Debug.Log("MeshCreator: making new folder in Meshes folder at " + folderPath);
                AssetDatabase.CreateFolder("Assets/UCLAGameLab/Meshes", folderName);
            }

            string saveName = folderName + "/" + mcd.gameObject.name + "." + mcd.idNumber;

            // stash the rotation value, set back to identity, then switch back later
            Quaternion oldRotation = mcd.gameObject.transform.rotation;
            mcd.gameObject.transform.rotation = Quaternion.identity;

            // stash the scale value, set back to one, then switch back later
            Vector3 oldScale = mcd.gameObject.transform.localScale;
            mcd.gameObject.transform.localScale = Vector3.one;

            // transform the object if needed to account for the new pivot
            if (mcd.pivotHeightOffset != mcd.lastPivotOffset.x || mcd.pivotWidthOffset != mcd.lastPivotOffset.y || mcd.pivotWidthOffset != mcd.lastPivotOffset.z)
            {
                mcd.gameObject.transform.localPosition -= mcd.lastPivotOffset;
                mcd.lastPivotOffset = new Vector3(mcd.pivotWidthOffset, mcd.pivotHeightOffset, mcd.pivotDepthOffset);
                mcd.gameObject.transform.localPosition += mcd.lastPivotOffset;
            }

            // 
            // start mesh renderer setup section
            //

            // mesh for rendering the object
            // will either be flat or full mesh
            Mesh msh = new Mesh();

            // collider for mesh, if used
            Mesh collidermesh = new Mesh();
            if (mcd.uvWrapMesh)
            {
                // Set up game object with mesh;
                AssignMesh(gameObject, ref msh);
                collidermesh = msh;
            }
            else
            {
                AssignPlaneMesh(gameObject, ref msh);
                // if needed, create the 3d mesh collider
                if (mcd.generateCollider && !mcd.usePrimitiveCollider && !mcd.useAABBCollider)
                    AssignMesh(gameObject, ref collidermesh);
            }

            MeshRenderer mr = (MeshRenderer)mcd.gameObject.GetComponent("MeshRenderer");
            if (mr == null)
            {
                //Debug.Log("MeshCreator Warning: no mesh renderer found on update object, adding one.");
                mcd.gameObject.AddComponent(typeof(MeshRenderer));
            }

            // update the front material via renderer
            Material meshmat;
            string materialNameLocation = "Assets/UCLAGameLab/Materials/" + mcd.outlineTexture.name + ".material.mat";
            string transparentMaterialNameLocation = "Assets/UCLAGameLab/Materials/" + mcd.outlineTexture.name + ".trans.material.mat";

            string baseMaterialNameLocation = "Assets/UCLAGameLab/Materials/baseMaterial.mat";
            string transparentBaseMaterialNameLocation = "Assets/UCLAGameLab/Materials/baseTransparentMaterial.mat";

            if (mcd.useAutoGeneratedMaterial)
            {
                // if using uvWrapMesh, use regular material
                if (mcd.uvWrapMesh)
                {
                    meshmat = (Material)AssetDatabase.LoadAssetAtPath(materialNameLocation, typeof(Material));
                    if (meshmat == null)
                    {
                        meshmat = CopyTexture(baseMaterialNameLocation, materialNameLocation, mcd.outlineTexture);
                    }
                    mcd.gameObject.GetComponent<Renderer>().sharedMaterial = meshmat;
                    Debug.Log("gen mat1");
                }
                else
                { // use a transparent material
                    meshmat = (Material)AssetDatabase.LoadAssetAtPath(transparentMaterialNameLocation, typeof(Material));
                    if (meshmat == null)
                    {
                        meshmat = CopyTexture(transparentBaseMaterialNameLocation, transparentMaterialNameLocation, mcd.outlineTexture);
                    }
                    mcd.gameObject.GetComponent<Renderer>().sharedMaterial = meshmat;
                    Debug.Log("gen mat2");
                }

            }
            else
            {
                mcd.gameObject.GetComponent<Renderer>().sharedMaterial = mcd.frontMaterial;
                Debug.Log("auto gen mat");
            }

            MeshFilter mf = (MeshFilter)mcd.gameObject.GetComponent("MeshFilter");
            if (mf == null)
            {
                //Debug.LogWarning("MeshCreator Warning: no mesh filter found on update object, adding one.");
                mf = mcd.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            }

            mf.sharedMesh = msh;

            // save the main mesh
            string meshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".asset";
            AssetDatabase.CreateAsset(msh, meshName);
            /*
            // make the side edges
            if (!mcd.uvWrapMesh && mcd.createEdges)
            {
                Mesh edgemesh = new Mesh();
                MeshCreator.AssignEdgeMesh(gameObject, ref edgemesh);

                // remove the old backside mesh game object
                string edgeName = mcd.gameObject.name + ".edge";
                ArrayList destroyObject = new ArrayList();
                foreach (Transform child in mcd.gameObject.transform)
                {
                    if (child.name == edgeName)
                    {
                        MeshFilter emf = (MeshFilter)child.gameObject.GetComponent("MeshFilter");
                        if (emf != null)
                        {
                            Mesh ems = (Mesh)emf.sharedMesh;
                            if (ems != null)
                            {
                                //DestroyImmediate(ems, true);
                            }
                        }
                        destroyObject.Add(child);
                    }
                }

                while (destroyObject.Count > 0)
                {
                    Transform child = (Transform)destroyObject[0];
                    destroyObject.Remove(child);
                    DestroyImmediate(child.gameObject);
                }

                // create a new game object to attach the backside plane
                GameObject edgeObject = new GameObject();
                edgeObject.transform.parent = mcd.gameObject.transform;
                edgeObject.transform.localPosition = Vector3.zero;
                edgeObject.transform.rotation = Quaternion.identity;
                edgeObject.name = edgeName;
                MeshFilter edgemf = (MeshFilter)edgeObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
                edgemf.sharedMesh = edgemesh;

                // save the mesh in the Assets folder
                string edgeMeshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".Edge" + ".asset";
                AssetDatabase.CreateAsset(edgemesh, edgeMeshName);

                MeshRenderer edgemr = edgeObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

                // for side meshes use the opaque material
                Material edgematerial = (Material)AssetDatabase.LoadAssetAtPath(materialNameLocation, typeof(Material));
                if (edgematerial == null)
                {
                    edgematerial = CopyTexture(baseMaterialNameLocation, materialNameLocation, mcd.outlineTexture);
                }
                edgemr.GetComponent<Renderer>().sharedMaterial = edgematerial;
            }
            else // destroy the old edge objects because they're not needed
            {
                string edgeName = mcd.gameObject.name + ".edge";
                ArrayList destroyObject = new ArrayList();
                foreach (Transform child in mcd.gameObject.transform)
                {
                    if (child.name == edgeName)
                    {
                        destroyObject.Add(child);
                        MeshFilter emf = (MeshFilter)child.gameObject.GetComponent("MeshFilter");
                        if (emf != null)
                        {
                            Mesh ems = (Mesh)emf.sharedMesh;
                            if (ems != null)
                            {
                                //DestroyImmediate(ems, true);
                            }
                        }
                    }
                }
                while (destroyObject.Count > 0)
                {
                    Transform child = (Transform)destroyObject[0];
                    destroyObject.Remove(child);
                    DestroyImmediate(child.gameObject);
                }
            }
            */

            /*
            // make the backside plane
            if (!mcd.uvWrapMesh && mcd.createBacksidePlane)
            {
                Mesh backmesh = new Mesh();
                AssignPlaneMeshBackside(gameObject, ref backmesh);

                // remove the old backside mesh game object
                string backsideName = mcd.gameObject.name + ".backside";
                ArrayList destroyObject = new ArrayList();
                foreach (Transform child in mcd.gameObject.transform)
                {
                    if (child.name == backsideName)
                    {
                        destroyObject.Add(child);
                        MeshFilter emf = (MeshFilter)child.gameObject.GetComponent("MeshFilter");
                        if (emf != null)
                        {
                            Mesh ems = (Mesh)emf.sharedMesh;
                            if (ems != null)
                            {
                                //DestroyImmediate(ems, true);
                            }
                        }
                    }
                }

                while (destroyObject.Count > 0)
                {
                    Transform child = (Transform)destroyObject[0];
                    destroyObject.Remove(child);
                    DestroyImmediate(child.gameObject);
                }

                // create a new game object to attach the backside plane
                GameObject backsideObject = new GameObject();
                backsideObject.transform.parent = mcd.gameObject.transform;
                backsideObject.transform.localPosition = Vector3.zero;
                backsideObject.transform.rotation = Quaternion.identity;
                backsideObject.name = backsideName;
                MeshFilter backmf = (MeshFilter)backsideObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
                backmf.sharedMesh = backmesh;
                // save the mesh in the Assets folder
                string backMeshName = "Assets/UCLAGameLab/Meshes/" + saveName + ".Back" + ".asset";
                AssetDatabase.CreateAsset(backmesh, backMeshName);

                MeshRenderer backmr = backsideObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

                // for backside plane, use the transparent material
                Material backmaterial = (Material)AssetDatabase.LoadAssetAtPath(transparentMaterialNameLocation, typeof(Material));
                if (backmaterial == null)
                {
                    backmaterial = CopyTexture(transparentBaseMaterialNameLocation, transparentMaterialNameLocation, mcd.outlineTexture);
                }
                backmr.GetComponent<Renderer>().sharedMaterial = backmaterial;
            }
            else // remove the old backside mesh game object because it's not needed
            {
                string backsideName = mcd.gameObject.name + ".backside";
                ArrayList destroyObject = new ArrayList();
                foreach (Transform child in mcd.gameObject.transform)
                {
                    if (child.name == backsideName)
                    {
                        destroyObject.Add(child);
                        // get rid of the old mesh from the assets
                        MeshFilter emf = (MeshFilter)child.gameObject.GetComponent("MeshFilter");
                        if (emf != null)
                        {
                            Mesh ems = (Mesh)emf.sharedMesh;
                            if (ems != null)
                            {
                                //DestroyImmediate(ems, true);
                            }
                        }
                    }
                }

                while (destroyObject.Count > 0)
                {
                    Transform child = (Transform)destroyObject[0];
                    destroyObject.Remove(child);
                    DestroyImmediate(child.gameObject);
                }
            }
            */

            mcd.gameObject.transform.rotation = oldRotation;
            mcd.gameObject.transform.localScale = oldScale;

        }
        
        /*
        *	AssignMesh() does calculation of a uv mapped mesh from the raster image.
        */
        public static void AssignMesh(GameObject gameObject, ref Mesh msh)
        {
            MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
            string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path);

            Color[] pixels = mcd.outlineTexture.GetPixels();    // get the pixels to build the mesh from

            // possibly do some size checking
            int imageHeight = mcd.outlineTexture.height;
            int imageWidth = mcd.outlineTexture.width;
            if (((float)imageWidth) / ((float)imageHeight) != mcd.meshWidth / mcd.meshHeight)
            {
                //Debug.LogWarning("Mesh Creator Inspector Warning: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
                //Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
            }

            // make a surface object to create and store data from image
            MC_SimpleSurfaceEdge mcs = new MC_SimpleSurfaceEdge(pixels, imageWidth, imageHeight, mcd.pixelTransparencyThreshold / 255.0f);

            if (mcd.mergeClosePoints) mcs.MergeClosePoints(mcd.mergeDistance);

            // Create the mesh

            if (!mcs.ContainsIslands())
            {
                // need a list of ordered 2d points
                Vector2[] vertices2D = mcs.GetOutsideEdgeVertices();

                // Use the triangulator to get indices for creating triangles
                Triangulator tr = new Triangulator(vertices2D);

                int[] indices = tr.Triangulate(); // these will be reversed for the back side
                Vector2[] uvs = new Vector2[vertices2D.Length * 4];
                // Create the Vector3 vertices
                Vector3[] vertices = new Vector3[vertices2D.Length * 4];

                float halfDepth = -mcd.meshDepth / 2.0f;
                float halfVerticalPixel = 0.5f / imageHeight;
                float halfHorizontalPixel = 0.5f / imageWidth;
                for (int i = 0; i < vertices2D.Length; i++)
                {
                    float vertX = 1.0f - (vertices2D[i].x / imageWidth) - halfHorizontalPixel; // get X point and normalize
                    float vertY = vertices2D[i].y / imageHeight + halfVerticalPixel; // get Y point and normalize
                    vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                    vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                    vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);

                    vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);

                    vertices[i + (vertices2D.Length * 2)] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset); // vertex for side

                    vertices[i + (vertices2D.Length * 3)] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);

                    uvs[i] = mcs.GetUVForIndex(i);
                    uvs[i + vertices2D.Length] = uvs[i];
                    uvs[i + (vertices2D.Length * 2)] = uvs[i];
                    uvs[i + (vertices2D.Length * 3)] = uvs[i];
                }

                // make the back side triangle indices
                // double the indices for front and back, 6 times the number of edges on front
                int[] allIndices = new int[(indices.Length * 2) + ((vertices2D.Length) * 6)];

                // copy over the front and back index data
                for (int i = 0; i < indices.Length; i++)
                {
                    allIndices[i] = indices[i]; // front side uses normal indices returned from the algorithm
                    allIndices[(indices.Length * 2) - i - 1] = indices[i] + vertices2D.Length; // backside reverses the order
                }

                // create the side triangle indices
                // for each edge, create a new set of two triangles
                // edges are just two points from the original set
                for (int i = 0; i < vertices2D.Length - 1; i++)
                {
                    allIndices[(indices.Length * 2) + (6 * i)] = (vertices2D.Length * 2) + i + 1;
                    allIndices[(indices.Length * 2) + (6 * i) + 1] = (vertices2D.Length * 2) + i;
                    allIndices[(indices.Length * 2) + (6 * i) + 2] = (vertices2D.Length * 2) + i + 1 + vertices2D.Length;
                    allIndices[(indices.Length * 2) + (6 * i) + 3] = (vertices2D.Length * 2) + i + 1 + vertices2D.Length;
                    allIndices[(indices.Length * 2) + (6 * i) + 4] = (vertices2D.Length * 2) + i;
                    allIndices[(indices.Length * 2) + (6 * i) + 5] = (vertices2D.Length * 2) + i + vertices2D.Length;
                }

                // wrap around for the last face
                allIndices[allIndices.Length - 6] = (vertices2D.Length * 2) + 0;
                allIndices[allIndices.Length - 5] = (vertices2D.Length * 2) + vertices2D.Length - 1;
                allIndices[allIndices.Length - 4] = (vertices2D.Length * 2) + vertices2D.Length;
                allIndices[allIndices.Length - 3] = (vertices2D.Length * 2) + vertices2D.Length;
                allIndices[allIndices.Length - 2] = (vertices2D.Length * 2) + vertices2D.Length - 1;
                allIndices[allIndices.Length - 1] = (vertices2D.Length * 2) + (vertices2D.Length * 2) - 1;


                msh.vertices = vertices;
                msh.triangles = allIndices;
                msh.uv = uvs;
                msh.RecalculateNormals();
                msh.RecalculateBounds();
                msh.name = mcd.outlineTexture.name + ".asset";

                // this will get the pivot drawing in the correct place
                Bounds oldBounds = msh.bounds;
                msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
            }
            else
            { // there be islands here, so treat mesh creation slightly differently
                ArrayList allVertexLoops = mcs.GetAllEdgeVertices();

                ArrayList completeVertices = new ArrayList();
                ArrayList completeIndices = new ArrayList();
                ArrayList completeUVs = new ArrayList();
                int verticesOffset = 0;
                int indicesOffset = 0;
                int uvOffset = 0;
                int loopCount = 0;
                foreach (Vector2[] vertices2D in allVertexLoops)
                {
                    // TODO: this needs to check if the current list is inside another shape
                    // Use the triangulator to get indices for creating triangles
                    Triangulator tr = new Triangulator(vertices2D);
                    int[] indices = tr.Triangulate(); // these will be reversed for the back side
                    Vector2[] uvs = new Vector2[vertices2D.Length * 4];
                    // Create the Vector3 vertices
                    Vector3[] vertices = new Vector3[vertices2D.Length * 4];

                    float halfDepth = -mcd.meshDepth / 2.0f;
                    float halfVerticalPixel = 0.5f / imageHeight;
                    float halfHorizontalPixel = 0.5f / imageWidth;
                    for (int i = 0; i < vertices2D.Length; i++)
                    {
                        float vertX = 1.0f - (vertices2D[i].x / imageWidth) - halfHorizontalPixel; // get X point and normalize
                        float vertY = vertices2D[i].y / imageHeight + halfVerticalPixel; // get Y point and normalize
                        vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                        vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                        vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);
                        vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);
                        vertices[i + (vertices2D.Length * 2)] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset); // vertex for side
                        vertices[i + (vertices2D.Length * 3)] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);

                        uvs[i] = mcs.GetUVForIndex(loopCount, i);
                        uvs[i + vertices2D.Length] = uvs[i];
                        uvs[i + (vertices2D.Length * 2)] = uvs[i];
                        uvs[i + (vertices2D.Length * 3)] = uvs[i];
                    }

                    // make the back side triangle indices
                    // double the indices for front and back, 6 times the number of edges on front
                    int[] allIndices = new int[(indices.Length * 2) + ((vertices2D.Length) * 6)];

                    // copy over the front and back index data
                    for (int i = 0; i < indices.Length; i++)
                    {
                        allIndices[i] = indices[i] + verticesOffset; // front side uses normal indices returned from the algorithm
                        allIndices[(indices.Length * 2) - i - 1] = indices[i] + vertices2D.Length + verticesOffset; // backside reverses the order
                    }

                    // create the side triangle indices
                    // for each edge, create a new set of two triangles
                    // edges are just two points from the original set
                    for (int i = 0; i < vertices2D.Length - 1; i++)
                    {
                        allIndices[(indices.Length * 2) + (6 * i)] = (vertices2D.Length * 2) + i + 1 + verticesOffset;
                        allIndices[(indices.Length * 2) + (6 * i) + 1] = (vertices2D.Length * 2) + i + verticesOffset;
                        allIndices[(indices.Length * 2) + (6 * i) + 2] = (vertices2D.Length * 2) + i + 1 + vertices2D.Length + verticesOffset;
                        allIndices[(indices.Length * 2) + (6 * i) + 3] = (vertices2D.Length * 2) + i + 1 + vertices2D.Length + verticesOffset;
                        allIndices[(indices.Length * 2) + (6 * i) + 4] = (vertices2D.Length * 2) + i + verticesOffset;
                        allIndices[(indices.Length * 2) + (6 * i) + 5] = (vertices2D.Length * 2) + i + vertices2D.Length + verticesOffset;
                    }

                    // wrap around for the last face
                    allIndices[allIndices.Length - 6] = (vertices2D.Length * 2) + 0 + verticesOffset;
                    allIndices[allIndices.Length - 5] = (vertices2D.Length * 2) + vertices2D.Length - 1 + verticesOffset;
                    allIndices[allIndices.Length - 4] = (vertices2D.Length * 2) + vertices2D.Length + verticesOffset;
                    allIndices[allIndices.Length - 3] = (vertices2D.Length * 2) + vertices2D.Length + verticesOffset;
                    allIndices[allIndices.Length - 2] = (vertices2D.Length * 2) + vertices2D.Length - 1 + verticesOffset;
                    allIndices[allIndices.Length - 1] = (vertices2D.Length * 2) + (vertices2D.Length * 2) - 1 + verticesOffset;

                    foreach (Vector3 v in vertices)
                    {
                        completeVertices.Add(v);
                    }
                    foreach (Vector2 v in uvs)
                    {
                        completeUVs.Add(v);
                    }
                    foreach (int i in allIndices)
                    {
                        completeIndices.Add(i);
                    }

                    verticesOffset += vertices.Length;
                    uvOffset += uvs.Length;
                    indicesOffset += allIndices.Length;
                    loopCount++;
                }
                msh.vertices = (Vector3[])completeVertices.ToArray(typeof(Vector3));
                msh.triangles = (int[])completeIndices.ToArray(typeof(int));
                msh.uv = (Vector2[])completeUVs.ToArray(typeof(Vector2));
                msh.RecalculateNormals();
                msh.RecalculateBounds();
                msh.name = mcd.outlineTexture.name + ".asset";

                // this will get the pivot drawing in the correct place
                Bounds oldBounds = msh.bounds;
                msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
            }
        }
        
        /*
        *	AssignPlaneMesh() does calculation for a simple plane with uv coordinates
        * at the corners of the images. Really simple.
        */
        public static void AssignPlaneMesh(GameObject gameObject, ref Mesh msh)
        {
            MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;

            // get the outline texture
            string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path);

            // do some size checking
            int imageHeight = mcd.outlineTexture.height;
            int imageWidth = mcd.outlineTexture.width;

            if (((float)imageWidth) / ((float)imageHeight) != mcd.meshWidth / mcd.meshHeight)
            {
                Debug.LogWarning("Mesh Creator: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
                Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
            }

            // need a list of ordered 2d points
            Vector2[] vertices2D = { new Vector2(0.0f, 0.0f), new Vector2(0.0f, imageHeight), new Vector2(imageWidth, imageHeight), new Vector2(imageWidth, 0.0f) };

            // 
            int[] indices = { 0, 1, 2, 0, 2, 3 }; // these will be reversed for the back side
            Vector2[] frontUVs = { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f) };
            Vector2[] uvs = new Vector2[vertices2D.Length];
            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];

            float halfDepth = -mcd.meshDepth / 2.0f;
            for (int i = 0; i < vertices2D.Length; i++)
            {
                float vertX = 1.0f - (vertices2D[i].x / imageWidth); // get X point and normalize
                float vertY = vertices2D[i].y / imageHeight; // get Y point and normalize
                vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);

                uvs[i] = frontUVs[i];
            }

            msh.vertices = vertices;
            msh.triangles = indices;
            msh.uv = uvs;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            msh.name = mcd.outlineTexture.name + ".mesh";

            // this will get the pivot drawing in the correct place
            Bounds oldBounds = msh.bounds;
            msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
        }

        /*
        *	AssignPlaneMesh() does calculation for a simple plane with uv coordinates
        * at the corners of the images. Really simple.
        */
        public static void AssignPlaneMeshBackside(GameObject gameObject, ref Mesh msh)
        {
            MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;

            // get the outline texture
            string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path);

            // do some size checking
            int imageHeight = mcd.outlineTexture.height;
            int imageWidth = mcd.outlineTexture.width;

            if (((float)imageWidth) / ((float)imageHeight) != mcd.meshWidth / mcd.meshHeight)
            {
                Debug.LogWarning("Mesh Creator Inspector Warning: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
                Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
            }

            // need a list of ordered 2d points
            Vector2[] vertices2D = { new Vector2(0.0f, 0.0f), new Vector2(0.0f, imageHeight), new Vector2(imageWidth, imageHeight), new Vector2(imageWidth, 0.0f) };

            // 
            int[] indices = { 2, 1, 0, 3, 2, 0 }; // these will be reversed for the back side
            Vector2[] frontUVs = { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f) };
            Vector2[] uvs = new Vector2[vertices2D.Length];
            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];

            float halfDepth = mcd.meshDepth / 2.0f;
            for (int i = 0; i < vertices2D.Length; i++)
            {
                float vertX = 1.0f - (vertices2D[i].x / imageWidth); // get X point and normalize
                float vertY = vertices2D[i].y / imageHeight; // get Y point and normalize
                vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);

                uvs[i] = frontUVs[i];
            }

            msh.vertices = vertices;
            msh.triangles = indices;
            msh.uv = uvs;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            msh.name = mcd.outlineTexture.name + ".asset";

            // this will get the pivot drawing in the correct place
            Bounds oldBounds = msh.bounds;
            msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
        }

        /*
        *	AssignEdgeMesh() does calculation of a uv mapped edge mesh from the raster image.
        *	no front or back planes are included
        */
        public static void AssignEdgeMesh(GameObject gameObject, ref Mesh msh)
        {
            MeshCreatorData mcd = gameObject.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;

            string path = AssetDatabase.GetAssetPath(mcd.outlineTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path);

            Color[] pixels = mcd.outlineTexture.GetPixels();    // get the pixels to build the mesh from

            // possibly do some size checking
            int imageHeight = mcd.outlineTexture.height;
            int imageWidth = mcd.outlineTexture.width;
            if (((float)imageWidth) / ((float)imageHeight) != mcd.meshWidth / mcd.meshHeight)
            {
                Debug.LogWarning("Mesh Creator: selected meshWidth and meshHeight is not the same proportion as source image width and height. Results may be distorted.");
                Debug.LogWarning("    You may want to resize your image to be square, it can be easier that way.");
            }

            // make a surface object to create and store data from image
            MC_SimpleSurfaceEdge mcs = new MC_SimpleSurfaceEdge(pixels, imageWidth, imageHeight, mcd.pixelTransparencyThreshold / 255.0f);

            if (!mcs.ContainsIslands())
            {
                // need a list of ordered 2d points
                Vector2[] vertices2D = mcs.GetOutsideEdgeVertices();

                // Use the triangulator to get indices for creating triangles
                //Triangulator tr = new Triangulator(vertices2D);
                //int[] indices = tr.Triangulate(); // these will be reversed for the back side
                Vector2[] uvs = new Vector2[vertices2D.Length * 2];
                // Create the Vector3 vertices
                Vector3[] vertices = new Vector3[vertices2D.Length * 2];

                float halfDepth = -mcd.meshDepth / 2.0f;
                float halfVerticalPixel = 0.5f / imageHeight;
                float halfHorizontalPixel = 0.5f / imageWidth;
                for (int i = 0; i < vertices2D.Length; i++)
                {
                    float vertX = 1.0f - (vertices2D[i].x / imageWidth) - halfHorizontalPixel; // get X point and normalize
                    float vertY = vertices2D[i].y / imageHeight + halfVerticalPixel; // get Y point and normalize
                    vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                    vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                    vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset); // vertex for side
                    vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);

                    uvs[i] = mcs.GetUVForIndex(i);
                    uvs[i + vertices2D.Length] = uvs[i];
                }

                // make the back side triangle indices
                // double the indices for front and back, 6 times the number of edges on front
                int[] allIndices = new int[vertices2D.Length * 6];

                // create the side triangle indices
                // for each edge, create a new set of two triangles
                // edges are just two points from the original set
                for (int i = 0; i < vertices2D.Length - 1; i++)
                {
                    allIndices[(6 * i)] = i + 1;
                    allIndices[(6 * i) + 1] = i;
                    allIndices[(6 * i) + 2] = i + 1 + vertices2D.Length;
                    allIndices[(6 * i) + 3] = i + 1 + vertices2D.Length;
                    allIndices[(6 * i) + 4] = i;
                    allIndices[(6 * i) + 5] = i + vertices2D.Length;
                }

                // wrap around for the last face
                allIndices[allIndices.Length - 6] = 0;
                allIndices[allIndices.Length - 5] = vertices2D.Length - 1;
                allIndices[allIndices.Length - 4] = vertices2D.Length;
                allIndices[allIndices.Length - 3] = vertices2D.Length;
                allIndices[allIndices.Length - 2] = vertices2D.Length - 1;
                allIndices[allIndices.Length - 1] = (vertices2D.Length * 2) - 1;


                msh.vertices = vertices;
                msh.triangles = allIndices;
                msh.uv = uvs;
                msh.RecalculateNormals();
                msh.RecalculateBounds();
                msh.name = mcd.outlineTexture.name + ".asset";

                // this will get the pivot drawing in the correct place
                Bounds oldBounds = msh.bounds;
                msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
            }
            else
            { // there be islands here, so treat mesh creation slightly differently
                ArrayList allVertexLoops = mcs.GetAllEdgeVertices();

                ArrayList completeVertices = new ArrayList();
                ArrayList completeIndices = new ArrayList();
                ArrayList completeUVs = new ArrayList();
                int verticesOffset = 0;
                int indicesOffset = 0;
                int uvOffset = 0;
                int loopCount = 0;
                foreach (Vector2[] vertices2D in allVertexLoops)
                {
                    Vector2[] uvs = new Vector2[vertices2D.Length * 4];
                    // Create the Vector3 vertices
                    Vector3[] vertices = new Vector3[vertices2D.Length * 4];

                    float halfDepth = -mcd.meshDepth / 2.0f;
                    float halfVerticalPixel = 0.5f / imageHeight;
                    float halfHorizontalPixel = 0.5f / imageWidth;
                    for (int i = 0; i < vertices2D.Length; i++)
                    {
                        float vertX = 1.0f - (vertices2D[i].x / imageWidth) - halfHorizontalPixel; // get X point and normalize
                        float vertY = vertices2D[i].y / imageHeight + halfVerticalPixel; // get Y point and normalize
                        vertX = (vertX * mcd.meshWidth) - (mcd.meshWidth / 2.0f);  // scale X and position centered
                        vertY = (vertY * mcd.meshHeight) - (mcd.meshHeight / 2.0f);

                        vertices[i] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, -halfDepth - mcd.pivotDepthOffset);
                        vertices[i + vertices2D.Length] = new Vector3(vertX - mcd.pivotWidthOffset, vertY - mcd.pivotHeightOffset, halfDepth - mcd.pivotDepthOffset);

                        uvs[i] = mcs.GetUVForIndex(loopCount, i);
                        uvs[i + vertices2D.Length] = uvs[i];
                    }

                    // make the back side triangle indices
                    // double the indices for front and back, 6 times the number of edges on front
                    int[] allIndices = new int[vertices2D.Length * 6];

                    // create the side triangle indices
                    // for each edge, create a new set of two triangles
                    // edges are just two points from the original set
                    for (int i = 0; i < vertices2D.Length - 1; i++)
                    {
                        allIndices[(6 * i)] = i + 1 + verticesOffset;
                        allIndices[(6 * i) + 1] = i + verticesOffset;
                        allIndices[(6 * i) + 2] = i + 1 + vertices2D.Length + verticesOffset;
                        allIndices[(6 * i) + 3] = i + 1 + vertices2D.Length + verticesOffset;
                        allIndices[(6 * i) + 4] = i + verticesOffset;
                        allIndices[(6 * i) + 5] = i + vertices2D.Length + verticesOffset;
                    }

                    // wrap around for the last face
                    allIndices[allIndices.Length - 6] = 0 + verticesOffset;
                    allIndices[allIndices.Length - 5] = vertices2D.Length - 1 + verticesOffset;
                    allIndices[allIndices.Length - 4] = vertices2D.Length + verticesOffset;
                    allIndices[allIndices.Length - 3] = vertices2D.Length + verticesOffset;
                    allIndices[allIndices.Length - 2] = vertices2D.Length - 1 + verticesOffset;
                    allIndices[allIndices.Length - 1] = (vertices2D.Length * 2) - 1 + verticesOffset;

                    foreach (Vector3 v in vertices)
                    {
                        completeVertices.Add(v);
                    }
                    foreach (Vector2 v in uvs)
                    {
                        completeUVs.Add(v);
                    }
                    foreach (int i in allIndices)
                    {
                        completeIndices.Add(i);
                    }

                    verticesOffset += vertices.Length;
                    uvOffset += uvs.Length;
                    indicesOffset += allIndices.Length;
                    loopCount++;
                }
                msh.vertices = (Vector3[])completeVertices.ToArray(typeof(Vector3));
                msh.triangles = (int[])completeIndices.ToArray(typeof(int));
                msh.uv = (Vector2[])completeUVs.ToArray(typeof(Vector2));
                msh.RecalculateNormals();
                msh.RecalculateBounds();
                msh.name = mcd.outlineTexture.name + ".asset";

                // this will get the pivot drawing in the correct place
                Bounds oldBounds = msh.bounds;
                msh.bounds = new Bounds(Vector3.zero, new Vector3(oldBounds.size.x, oldBounds.size.y, oldBounds.size.z));
            }
        }
    
        // copies a texture and saves into the project
        public static Material CopyTexture(string baseNameLocation,
            string newNameLocation,
            Texture texture)
        {
            Material mat;
            AssetDatabase.CopyAsset(baseNameLocation, newNameLocation);
            AssetDatabase.ImportAsset(newNameLocation);
            mat = (Material)AssetDatabase.LoadAssetAtPath(newNameLocation, typeof(Material));
            // mat.name = mcd.outlineTexture.name + ".Material"; // this probably isn't needed
            mat.mainTexture = texture;
            AssetDatabase.SaveAssets();
            return mat;
        }

        // generates a unique string for mesh naming
        // from http://madskristensen.net/post/Generate-unique-strings-and-numbers-in-C.aspx
        public static string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        public static bool IdExistsInScene(MeshCreatorData mcd)
        {
            // check all objects in this scene for a matching unique number
            object[] objs = GameObject.FindObjectsOfType(typeof(GameObject));
            foreach (GameObject go in objs)
            {
                MeshCreatorData meshcd = go.GetComponent(typeof(MeshCreatorData)) as MeshCreatorData;
                if (meshcd && go != mcd.gameObject)
                {
                    if (meshcd.idNumber == mcd.idNumber) return true;
                }
            }
            return false;
        }
    }