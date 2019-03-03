using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using MarchingCubesProject;
using UnityEditorInternal;

namespace MarchingCubesProject
{
    public class QuadScript : MonoBehaviour
    {
        public float[] Voxels;
        public int Width, Heigth, Length;

        private Texture2D texture;

        //private int segments = 36;
        private MeshScript mscript;

        public bool lerp = true;

        private float m_slice = 50f;

        public float slice
        {
            get { return m_slice; }
            set
            {
                if (m_slice == value)
                    return;

                m_slice = value;
                setSlice((int) value);
            }
        }

        private float m_iso = 0.5f;

        public float iso
        {
            get { return m_iso; }
            set
            {
                if (m_iso == value)
                    return;

                m_iso = value;
                CreateIsoLine(texture);
            }
        }

        // location of center of 'imaginary' sphere
        private Vector3 ORIGO = new Vector3(50f, 50f, 50f);

        //size of half a texture pixel
        private const float pxsize = 1f;

        // Start is called once when the application is run
        void Start()
        {
            print("void Start was called");
            setSlice(50); // shows a slice
            mscript = GameObject.Find("GameObjectMesh").GetComponent<MeshScript>();
        }

        void CreateIsoLine(Texture2D tex)
        {
            if (tex == null)
                tex = (Texture2D) GetComponent<Renderer>().material.mainTexture;

            int ht = tex.height;
            int width = tex.width;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            int index = 0;

            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < ht; j++)
                {
                    /*  d------c
                     *  |      |
                     *  |      |
                     *  a------b
                     */

                    float bl = tex.GetPixel(i - 1, j - 1).grayscale; // bottom left A
                    bool pa = bl > m_iso;

                    float br = tex.GetPixel(i, j - 1).grayscale; // bottom right B
                    bool pb = br > m_iso;

                    float tr = tex.GetPixel(i, j).grayscale; // top right C
                    bool pc = tr > m_iso;

                    float tl = tex.GetPixel(i - 1, j).grayscale; // top left D
                    bool pd = tl > m_iso;

                    // delta offsets
                    float dl = 0f, dr = 0f, dt = 0f, db = 0f;

                    if (lerp)
                    {
                        if (tl < bl) dl = 1f - (iso - tl) / (bl - tl);
                        else dl = (iso - bl) / (tl - bl);

                        if (tr < br) dr = 1f - (iso - tr) / (br - tr);
                        else dr = (iso - br) / (tr - br);

                        if (tl < tr) dt = 1f - (iso - tr) / (tl - tr);
                        else dt = (iso - tl) / (tr - tl);

                        if (bl < br) db = 1f - (iso - br) / (bl - br);
                        else db = (iso - bl) / (br - bl);
                    }

                    string tup = "";
                    if (pa) tup += '1';
                    else tup += '0';
                    if (pb) tup += '1';
                    else tup += '0';
                    if (pc) tup += '1';
                    else tup += '0';
                    if (pd) tup += '1';
                    else tup += '0';

                    Vector3 px = new Vector3(i - pxsize * .5f + ORIGO.x, j - pxsize * .5f + ORIGO.y);

                    Vector3 l = new Vector3(px.x, px.y + dl);
                    Vector3 r = new Vector3(px.x + pxsize, px.y + dr);
                    Vector3 t = new Vector3(px.x + dt, px.y + pxsize);
                    Vector3 b = new Vector3(px.x + db, px.y);

                    switch (tup)
                    {
                        case "0000": //0
                        case "1111": //15
                            break;
                        case "1000": //1
                            vertices.Add(b);
                            vertices.Add(l);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "0100": //2
                            vertices.Add(r);
                            vertices.Add(b);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "1100": //3
                            vertices.Add(r);
                            vertices.Add(l);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "0001": //4
                            vertices.Add(l);
                            vertices.Add(t);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "1001": //5
                            vertices.Add(b);
                            vertices.Add(t);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        //case "0101": //6
                        //    vertices.Add(new Vector3(px.x + hlfpx, px.y));
                        //    vertices.Add(bb);
                        //    vertices.Add(new Vector3(px.x - hlfpx, px.y));
                        //    vertices.Add(new Vector3(px.x, px.y + hlfpx));
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    break;
                        case "1101": //7
                            vertices.Add(r);
                            vertices.Add(t);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "0010": //8
                            vertices.Add(t);
                            vertices.Add(r);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        //case "1010": //9
                        //    vertices.Add(bb);
                        //    vertices.Add(new Vector3(px.x - hlfpx, px.y));
                        //    vertices.Add(tt);
                        //    vertices.Add(rr);
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    indices.Add(index++);
                        //    break;
                        case "0110": //10
                            vertices.Add(t);
                            vertices.Add(b);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "1110": //11
                            vertices.Add(t);
                            vertices.Add(l);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "0011": //12
                            vertices.Add(l);
                            vertices.Add(r);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "1011": //13
                            vertices.Add(b);
                            vertices.Add(r);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                        case "0111": //14
                            vertices.Add(b);
                            vertices.Add(l);
                            indices.Add(index++);
                            indices.Add(index++);
                            break;
                    }
                }
            }

            if (mscript == null)
                mscript = GameObject.Find("GameObjectMesh").GetComponent<MeshScript>();

            mscript.createIsolineGeometry(vertices, indices);

        }

        void setSlice(int z)
        {
            const int xdim = 100;
            const int ydim = 100;

            texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false); // garbage collector will tackle that it is new'ed


            for (int y = 0; y < ydim; y++)
                for (int x = 0; x < xdim; x++)
                {
                    //float v = pixelval(new Vector3(x, y, z));
                    //texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
                }

            texture.filterMode =
                FilterMode.Point; // nearest neigbor interpolation is used.  (alternative is FilterMode.Bilinear)
            texture.Apply(); // Apply all SetPixel calls
            GetComponent<Renderer>().material.mainTexture = texture;
            CreateIsoLine(texture);

        }

        //float pixelval(Vector3 p)
        //{
        //    return Vector3.Magnitude(p - ORIGO)/50f;
        //}

        //public void slicePosSliderChange(float val)
        //{
        //    slice = val;
        //}

        //public void sliceIsoSliderChange(float val)
        //{
        //    iso = val;
        //}

        //public void button1Pushed()
        //{
        //      print("button1Pushed");
        //}

        //public void button2Pushed()
        //{
        //    lerp = !lerp;
        //    CreateIsoLine2(texture);
        //}
    }
}