using System;
using UnityEngine;
using System.Collections.Generic;

using ProceduralNoiseProject;

namespace MarchingCubesProject
{
    public enum MARCHING_MODE { Cubes, Tetrahedron };
    public enum MARCHING_OBJECT { Sphere, Fractal, DicomScan }
    public class VoxelScript : MonoBehaviour
    {
        public Material m_material;

        public MARCHING_MODE Mode = MARCHING_MODE.Tetrahedron;
        public MARCHING_OBJECT MObject = MARCHING_OBJECT.Sphere;

        public int seed = 0;
        public float Iso = 0f;

        public int Width = 16;
        public int Height = 16;
        public int Length = 16;
        public float Radius = 8f;

        private List<GameObject> meshes = new List<GameObject>();
        private Marching marching;
        private GameObject go;
        private float[] voxels = null;
        /// <summary>
        /// voxels must be recalculated if Width/Heigth/Length/Radius has changed
        /// value set from UiInputScript and CreateNewVoxels
        /// </summary>
        public bool NewVoxelsNeeded = true;

        // Dicom Voxels:
        Slice[] _slices;
        int _numSlices;
        int _minIntensity;
        int _maxIntensity;
        int _sliceNum;
        float _iso;
        float _brightness;
        int _xdim;
        int _ydim;
        int _zdim;
        Vector3 _voxelSize;
        private SliceInfo info;

        private bool valueFound = false;

        private bool dicomSetLoaded = false;

        private void LoadDicomData()
        {
            Slice.initDicom();

            // absolute path: string dicomfilepath = @"D:\Hoyskolen\ctScans++\Bag scan\55540002";
            //relative path :
            string dicomfilepath = Application.dataPath + @"/../dicomdata/"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up

            _numSlices = _numSlices = Slice.getnumslices(dicomfilepath);
            _slices = new Slice[_numSlices];

            float min = 0;
            float max = 0;
            Slice.getSlices(dicomfilepath, _numSlices, out _slices, out min, out max);

            info = _slices[0].sliceInfo;

            _minIntensity = (int)min;
            _maxIntensity = (int)max;

            _xdim = info.Rows;
            _ydim = info.Columns;
            _zdim = _numSlices;
            print("Number of slices read:" + _numSlices);
        }


        void Start()
        {
            CreateNewSurface();
        }

        public void UpdateButtonPushed()
        {
            if (NewVoxelsNeeded)
                CreateNewVoxels();

            CreateNewSurface();
        }

        private void CreateNewVoxels()
        {
            voxels = null;
            if (MObject == MARCHING_OBJECT.Fractal)
            {
                voxels = GenerateFractalVoxels(Width, Height, Length);
            }
            else if (MObject == MARCHING_OBJECT.Sphere)
            {
                Vector3 origo = new Vector3(Radius, Radius, Radius);
                voxels = GenerateSphereVoxels(Radius, Width, Height, Length, origo);
            }
            else if (MObject == MARCHING_OBJECT.DicomScan)
            {
                if (!dicomSetLoaded)
                {
                    LoadDicomData();
                    dicomSetLoaded = true;
                }
                //voxels = GenerateDicomSliceVoxels();
                voxels = GenerateDicomSliceVoxels(Width, Height, Length);
            }
            NewVoxelsNeeded = false;
        }

        private void CreateNewSurface()
        {

            if (voxels == null)
            {
                CreateNewVoxels();
            }

            foreach (GameObject lgo in meshes)
            {
                Destroy(lgo);
            }
            meshes.Clear();
            marching = null;
            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            print("generating surface..");

            if (Mode == MARCHING_MODE.Tetrahedron)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = Iso;


            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.

            print("w:" + Width + " h:" + Height + " l:" + Length);
            marching.Generate(voxels, Width, Height, Length, verts, indices);

            print("marching alg complete. Creating meshes from " + verts.Count + " vertices...");

            //A mesh in unity can only be made up of 65000 verts.
            //Need to split the verts between multiple meshes.

            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

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
                go.transform.localPosition = new Vector3(-Width / 2, -Height / 2, -Length / 2);

                meshes.Add(go);

                print("surfaces generated!");
            }
        }

        private float[] GenerateDicomSliceVoxels(int width, int height, int length)
        {
            float stepsizeX = _xdim / (width);
            float stepsizeY = _ydim / (height);
            float stepsizeZ = _zdim / (length);

            float[] voxels = new float[width * height * length];

            print("generating " + voxels.Length + " voxels...");
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        int idx = x + y * width + z * width * height; // xxxxxyyyyyzzzzz
                        voxels[idx] = GetDicomLocVal((int) (((float)x)*stepsizeX),(int) (((float)y)*stepsizeY),(int) (((float)z)*stepsizeZ));
                    }
                }
            }
            return voxels;
        }

        private float[] GenerateFractalVoxels(int width, int height, int length)
        {
            INoise perlin = new PerlinNoise(seed, 2.0f);
            FractalNoise fractal = new FractalNoise(perlin, 3, .5f);

            float[] voxels = new float[width * height * length];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        float fx = x / (width - 1f);
                        float fy = y / (height - 1f);
                        float fz = z / (length - 1f);

                        int idx = x + y * width + z * width * height;

                        voxels[idx] = fractal.Sample3D(fx, fy, fz);
                    }
                }
            }
            return voxels;
        }
        /// <summary>
        /// Generates voxelt in a sphere shape
        /// Bug: Radius doesn't have the desired effect of scaling the sphere
        /// </summary>
        private float[] GenerateSphereVoxels(float radius, int width, int height, int length, Vector3 origo)
        {
            float[] voxels = new float[width * height * length];
            float stepsizeX = 2f * radius / (width - 1);
            float stepsizeY = 2f * radius / (height - 1);
            float stepsizeZ = 2f * radius / (length - 1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        //float fx = x / (width - 1f);
                        //float fy = y / (height - 1f);
                        //float fz = z / (length - 1f);

                        int idx = x + y * width + z * width * height;
                        //float val = GetSphereLocVal(new Vector3(fx, fy, fz), radius, origo);
                        float val = GetSphereLocVal(new Vector3(x * stepsizeX, y * stepsizeY, z * stepsizeZ), radius, origo);

                        voxels[idx] = val;
                    }
                }
            }
            return voxels;
        }

        private float GetSphereLocVal(Vector3 p, float radius, Vector3 origo)
        {
            //return Vector3.Magnitude(p - origo);
            return 1 - Vector3.Magnitude(p - origo) / radius;
        }

        private float GetDicomLocVal(int x, int y, int slicenr)
        {
            int pxVal = - (_slices[slicenr].getPixels()[x + (y * _ydim)]);
            //if (pxVal > 0 && !valueFound)
            //{
            //    print(pxVal);
            //    valueFound = true;
            //}

            //return 1 - 2*Mathf.Lerp(_minIntensity, _maxIntensity, Mathf.InverseLerp(0, 1, (float)pxVal));


            return 1 - (((float)pxVal - (float)_minIntensity) / ((float)_maxIntensity - (float)_minIntensity));

        }

    }

}
