using System;
using UnityEngine;
using System.Collections.Generic;

using ProceduralNoiseProject;

namespace MarchingCubesProject
{
    public enum MARCHING_MODE { Cubes, Tetrahedron, NaiveTetrahedron };
    public enum MARCHING_OBJECT { Sphere, Fractal, DicomScan }
    public class VoxelScript : MonoBehaviour
    {
        private MeshScript _meshScript;
        [HideInInspector] public MARCHING_MODE Mode = MARCHING_MODE.Tetrahedron;
        [HideInInspector] public MARCHING_OBJECT MObject = MARCHING_OBJECT.Sphere;

        public bool AddPaddingToDicom = true;
        /// <summary>
        /// seed used for fractal noise generation
        /// </summary>
        public int seed = 0;
        [HideInInspector] public float Iso = 0f;

        [HideInInspector] public int Width;
        [HideInInspector] public int Height;
        [HideInInspector] public int Length;
        [HideInInspector] public float Radius = 8f;

        private List<GameObject> meshes = new List<GameObject>();
        //private Marching marching;
        private GameObject go;
        [HideInInspector] public float[] _voxels = null;

        /// <summary>
        /// voxels must be recalculated if Width/Heigth/Length or object has changed
        /// value set from UiInputScript and CreateNewVoxels
        /// </summary>
        [HideInInspector] public bool NewVoxelsNeeded = true;

        // Dicom Voxels:
        private Slice[] _slices;
        private int _numSlices;
        private int _minIntensity;
        private int _maxIntensity;
        private int _xdim;
        private int _ydim;
        private int _zdim;
        //private int _sliceNum;
        //private float _iso;
        //private float _brightness;
        //private Vector3 _voxelSize;

        private SliceInfo _info;
        private bool _dicomSetLoaded = false;

        void Start()
        {
            _meshScript = GetComponent<MeshScript>();
        }

        private void LoadDicomData()
        {
            if (!_dicomSetLoaded)
            {
                Slice.initDicom();

                //relative path :
                string dicomfilepath = Application.dataPath +
                    @"/../dicomdata/"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up

                if (AddPaddingToDicom)
                    _numSlices = _numSlices = Slice.getnumslices(dicomfilepath) + 2;
                else _numSlices = _numSlices = Slice.getnumslices(dicomfilepath);

                _slices = new Slice[_numSlices];
                float min = 0;
                float max = 0;
                Slice.getSlices(dicomfilepath, _numSlices, out _slices, out min, out max);

                _info = _slices[0].sliceInfo;

                _minIntensity = (int) min;
                _maxIntensity = (int) max;

                _xdim = _info.Rows;
                _ydim = _info.Columns;
                _zdim = _numSlices;
                print("Number of slices read:" + _numSlices);
            }
            else print("Dicom set already loaded.");
        }

        public void UpdateButtonPushed()
        {
            if (NewVoxelsNeeded)
                CreateNewVoxels();

            _meshScript.CreateNewSurface(_voxels, Mode, Iso, Width, Height, Length);
        }

        private void CreateNewVoxels()
        {
            _voxels = null;
            if (MObject == MARCHING_OBJECT.Fractal)
            {
                _voxels = GenerateFractalVoxels(Width, Height, Length);
            }
            else if (MObject == MARCHING_OBJECT.Sphere)
            {
                Vector3 origo = new Vector3(Radius, Radius, Radius);
                _voxels = GenerateSphereVoxels(Width, Height, Length, Radius, origo);
            }
            else if (MObject == MARCHING_OBJECT.DicomScan)
            {
                if (!_dicomSetLoaded)
                {
                    LoadDicomData();
                    _dicomSetLoaded = true;
                }

                if (AddPaddingToDicom)
                {
                    Width += 2; Height += 2; Length += 2; // added padding
                }
                _voxels = GenerateDicomVoxels(Width, Height, Length);
            }
            NewVoxelsNeeded = false;
        }

        private float[] GenerateDicomVoxels(int width, int height, int length)
        {
            float stepsizeX, stepsizeY, stepsizeZ;
            if (AddPaddingToDicom)
            {
                stepsizeX = (float) (_xdim) /  (width -1 );
                stepsizeY = (float) (_ydim) / (height -1 );
                stepsizeZ = (float) (_zdim) / (length -1 );
            }
            else
            {
                stepsizeX = (float)(_xdim) / (width);
                stepsizeY = (float)(_ydim) / (height);
                stepsizeZ = (float)(_zdim) / (length -1 );
            }
            float[] voxels = new float[width * height * length];
            print("generating " + voxels.Length + " voxels"
                  +(AddPaddingToDicom ? " (including edge padding blocks)..." : "..."));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        int idx = x + y * width + z * width * height;

                        if (AddPaddingToDicom && (x == 0 || y == 0 || z == 0 || x == width - 1 || y == height - 1 || z == length - 1))
                        {
                            // add a "padding block" for edge voxels
                            voxels[idx] = 2f * (float)_minIntensity - 1f;
                        }

                        else
                        {
                            if (AddPaddingToDicom)
                                voxels[idx] = GetDicomLocVal(
                            (int) (((float)x)*stepsizeX - 1f),  // -1 to compensate for padding block
                            (int) (((float)y)*stepsizeY - 1f),
                            (int) (((float)z)*stepsizeZ - 1f));

                            else voxels[idx] = GetDicomLocVal(
                                (int)(((float)x) * stepsizeX),
                                (int)(((float)y) * stepsizeY),
                                (int)(((float)z) * stepsizeZ));
                        }
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
        /// Generates voxels in a sphere shape
        /// radius and origo are co-dependent, and probably superfluous
        /// </summary>
        private float[] GenerateSphereVoxels(int width, int height, int length, float radius, Vector3 origo)
        {
            float[] voxels = new float[width * height * length];
            float stepsizeX = 2f * radius / (width  - 1f);
            float stepsizeY = 2f * radius / (height - 1f);
            float stepsizeZ = 2f * radius / (length - 1f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        int idx = x + y * width + z * width * height;
                        float val = GetSphereLocVal(new Vector3(x * stepsizeX, y * stepsizeY, z * stepsizeZ), radius, origo);

                        voxels[idx] = val;
                    }
                }
            }
            return voxels;
        }

        private float GetSphereLocVal(Vector3 p, float radius, Vector3 origo)
        {
            return 1f - Vector3.Magnitude(p - origo) / radius;
        }

        private float GetDicomLocVal(int x, int y, int slicenr)
        {
            int pxVal = (_slices[slicenr].getPixels()[x + (y * _xdim)]);

            // rescale to -1 to +1 range for better results with marching.Surface
            return 2f *(((float)pxVal - (float)_minIntensity) / ((float)_maxIntensity - (float)_minIntensity)) -1f;
        }
    }

}
