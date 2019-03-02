using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Collections;
using System.IO;
using System.Text;

namespace MarchingCubesProject
{
    public class MeshScript : MonoBehaviour
    {

        private List<GameObject> meshes = new List<GameObject>();
        private GameObject go;
        public Material m_material;
        private Marching marching;
        private List<Vector3> verts = new List<Vector3>();
        private List<int> indices = new List<int>();
        private Mesh _isolineMesh;
        [SerializeField] private int _maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle

        void Start()
        {
            //PrepareGameobjectForIsolines();
        }

        /// <summary>
        /// programatically create meshfilter and meshrenderer and add to gameobject this script is attached to.
        /// </summary>
        private void PrepareGameobjectForIsolines()
        {
            GameObject isoGo = gameObject;
            MeshFilter meshFilter = (MeshFilter)isoGo.AddComponent(typeof(MeshFilter));
            MeshRenderer renderer = isoGo.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        }

        public void createIsolineGeometry(List<Vector3> lineVertices, List<int> lineIndices)
        {
            _isolineMesh = GetComponent<MeshFilter>().mesh;
            _isolineMesh.Clear();
            _isolineMesh.SetVertices(lineVertices);

            // https://docs.unity3d.com/ScriptReference/MeshTopology.html
            _isolineMesh.SetIndices(lineIndices.ToArray(), MeshTopology.Lines,0); // MeshTopology.Points  MeshTopology.LineStrip   MeshTopology.Triangles
            _isolineMesh.RecalculateBounds();
        }

        public void MeshToFile(string filename)
        {
            if (verts.Count > 0 && indices.Count > 0 && filename.Length > 0)
            {
                StreamWriter stream = new StreamWriter(filename);
                stream.WriteLine("g " + "Mesh");
                System.Globalization.CultureInfo dotasDecimalSeparator = new System.Globalization.CultureInfo("en-US");

                foreach (Vector3 v in verts)
                    stream.WriteLine(string.Format(dotasDecimalSeparator, "v {0} {1} {2}", v.x, v.y, v.z));

                stream.WriteLine();

                for (int i = 0; i < indices.Count; i += 3)
                    stream.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", indices[i] + 1, indices[i + 1] + 1, indices[i + 2] + 1));

                stream.Close();
                print("Mesh saved to file: " + filename);
            }
        }

        public void CreateNewSurface(float[] voxels, MARCHING_MODE mode, float iso, int width, int height, int length)
        {
            // Reset meshes
            foreach (GameObject lgo in meshes)
                Destroy(lgo);

            meshes.Clear();

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            //print("generating surface..");
            marching = null;
            if (mode == MARCHING_MODE.Tetrahedron)
                marching = new MarchingTertrahedron();

            else marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = iso;

            verts = new List<Vector3>();
            indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.

            print("w:" + width + " h:" + height + " l:" + length);
            marching.Generate(voxels, width, height, length, verts, indices);

            print("marching alg complete. Creating meshes from " + verts.Count + " vertices...");

            //A mesh in unity can only be made up of 65000 verts.
            //Need to split the verts between multiple meshes.

            int numMeshes = verts.Count / _maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < _maxVertsPerMesh; j++)
                {
                    int idx = i * _maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);
                print("triangle count: " + splitIndices.Count);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                go = new GameObject("Mesh");
                go.transform.parent = transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = m_material;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.transform.localPosition = new Vector3(-width / 2f, -height / 2f, -length / 2f);

                meshes.Add(go);

                print("surfaces generated!");
            }
        }

    }
}