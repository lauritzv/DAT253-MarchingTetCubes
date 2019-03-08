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
        //public float[] Voxels;
        //public int Width, Heigth, Length;

        //private Texture2D texture;

        //private int segments = 36;
        public MeshScript mscript;
        public VoxelScript vscript;

        public bool lerp = true;

        private float m_slice = 0.5f;
        private int sliceNr = 0;
        public float slice
        {
            get { return m_slice; }
            set
            {
                if (Math.Abs(m_slice - value) < .0001f)
                    return;

                m_slice = value;
                sliceNr = setSlice(value);
                CreateIsoLine(sliceNr);
            }
        }

        private float m_iso = 0f;

        public float iso
        {
            get { return m_iso; }
            set
            {
                if (Math.Abs(m_iso - value) < .0001f)
                    return;

                print(value);
                m_iso = value;
                CreateIsoLine(sliceNr);
            }
        }

        // location of center of 'imaginary' sphere
        //private Vector3 ORIGO = new Vector3(50f, 50f, 50f);

        //size of a texture pixel
        private float pxsize = 1f;

        // Start is called once when the application is run
        void Start()
        {

            mscript = GetComponent<MeshScript>();
            vscript = GetComponent<VoxelScript>();
            sliceNr = setSlice(slice);
            CreateIsoLine(sliceNr);
        }

        void CreateIsoLine(int z)
        {
            //if (tex == null)
            //    tex = (Texture2D) GetComponent<Renderer>().material.mainTexture;

            //int ht = tex.height;
            //int width = tex.width;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            int index = 0;


            
            
            for (int i = 1; i < vscript.Width; i++)
            {
                for (int j = 1; j < vscript.Height; j++)
                {
                    
                    // int idx = i + j * width + z * width * height;
                    /*  d------c
                     *  |      |
                     *  |      |
                     *  a------b
                     */

                    // bottom left A
                    float bl = vscript._voxels[(i - 1) + (j - 1) * vscript.Width + z * vscript.Width * vscript.Height];
                    bool pa = bl > m_iso;

                    // bottom right B
                    float br = vscript._voxels[(i) + (j - 1) * vscript.Width + z * vscript.Width * vscript.Height];
                    bool pb = br > m_iso;

                    // top right C
                    float tr = vscript._voxels[(i) + (j) * vscript.Width + z * vscript.Width * vscript.Height];
                    bool pc = tr > m_iso;

                    // top left D
                    float tl = vscript._voxels[(i - 1) + (j) * vscript.Width + z * vscript.Width * vscript.Height];
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

                    string tup = (pa ? "1" : "0")
                                   + (pb ? "1" : "0")
                                   + (pc ? "1" : "0")
                                   + (pd ? "1" : "0");

                    Vector3 ORIGO = transform.localPosition;
                    ORIGO.x -= vscript.Width *.5f;
                    ORIGO.y -= vscript.Height *.5f;

                    Vector3 px = new Vector3(i - pxsize * .5f + ORIGO.x, j - pxsize * .5f + ORIGO.y);

                    Vector3 l = new Vector3(px.x, px.y + dl);
                    Vector3 r = new Vector3(px.x + pxsize, px.y + dr);
                    Vector3 t = new Vector3(px.x + dt, px.y + pxsize);
                    Vector3 b = new Vector3(px.x + db, px.y);

                    switch (tup)
                    {
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
                        default:
                            break;
                    }
                }
            }

            if (mscript == null)
                mscript = GameObject.Find("GameObjectMesh").GetComponent<MeshScript>();

            mscript.createIsolineGeometry(vertices, indices, vscript.Width, vscript.Height);

        }

        private int setSlice(float val)
        {
            return (int)( (float) vscript.Length * val);
        }

     
    }
}