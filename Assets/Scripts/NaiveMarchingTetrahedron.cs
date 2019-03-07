using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace MarchingCubesProject
{
    /// <summary>
    /// An implementation of Marching Tetrahedron that's based on slides from the DAT253 course at HVL
    /// </summary>
    public class NaiveMarchingTetrahedron : Marching
    {
        private Vector3[] EdgeVertex { get; set; }
        private Vector3[] CubePosition { get; set; }
        private Vector3[] TetrahedronPosition { get; set; }
        private float[] TetrahedronValue { get; set; }

        //private List<string> keyList = new List<string>(EdgeConnection.Keys);

        private List<Vector3> tetravecs = new List<Vector3>();
        private List<float> tetravalues = new List<float>();

        public NaiveMarchingTetrahedron(float surface = 0.5f) : base(surface)
        {
            EdgeVertex = new Vector3[6];
            CubePosition = new Vector3[8];
            TetrahedronPosition = new Vector3[4];
            TetrahedronValue = new float[4];
            UseNaiveTetrahedronOffsets = true;
        }

        private static readonly int[,] TetrahedronsInACube = new int[,]
        {
            // Cube to 6 tetra
            {4, 6, 0, 7},
            {6, 0, 7, 2},
            {0, 7, 2, 3},
            {4, 5, 7 ,0},
            {1 ,7 ,0 ,3},
            {0 ,5 ,7 ,1}
        };

        protected override void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList) // for each cube
        {
            int tetraIndex;
            //Make a local copy of the cube's corner positions
            for (tetraIndex = 0; tetraIndex < 8; tetraIndex++)
            {
                CubePosition[tetraIndex].x = x + VertexOffsetNaiveTetrahedron[tetraIndex, 0];
                CubePosition[tetraIndex].y = y + VertexOffsetNaiveTetrahedron[tetraIndex, 1];
                CubePosition[tetraIndex].z = z + VertexOffsetNaiveTetrahedron[tetraIndex, 2];
            }

            for (tetraIndex = 0; tetraIndex < 6; tetraIndex++) // for each tetra
            {
                tetravecs.Clear();
                tetravalues.Clear();
                for (int vertInTetraIndex = 0; vertInTetraIndex < 4; vertInTetraIndex++) // for each tri on tetra
                {
                    var vertexInACube = TetrahedronsInACube[tetraIndex, vertInTetraIndex];

                    TetrahedronPosition[vertInTetraIndex] = CubePosition[vertexInACube];
                    TetrahedronValue[vertInTetraIndex] = cube[vertexInACube];

                    tetravecs.Add(TetrahedronPosition[vertInTetraIndex]);
                    tetravalues.Add(TetrahedronValue[vertInTetraIndex]);
                }
                DoTetrahedron(tetravecs, tetravalues, vertList, indexList);
            }
        }

        void DoTetrahedron(List<Vector3> tetravecs, List<float> tetravals, IList<Vector3> vertList, IList<int> indexList)
        {
            float d1 = tetravals[0];
            float d2 = tetravals[1];
            float d3 = tetravals[2];
            float d4 = tetravals[3];
            Vector3 v1 = tetravecs[0];
            Vector3 v2 = tetravecs[1];
            Vector3 v3 = tetravecs[2];
            Vector3 v4 = tetravecs[3];
            Vector3 p1, p2, p3, p4;

            string cases =   (d1 > Surface ? "1" : "0") 
                           + (d2 > Surface ? "1" : "0")
                           + (d3 > Surface ? "1" : "0")
                           + (d4 > Surface ? "1" : "0");

            switch (cases)
            {
                case "1110": // p14 p24 p34
                    p1 = TetraInterpolate(v1, v4, d1, d4);
                    p2 = TetraInterpolate(v2, v4, d2, d4);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    doTriangle(p1, p3, p2, vertList, indexList);
                    break;
                case "0001":
                    p1 = TetraInterpolate(v1, v4, d1, d4);
                    p2 = TetraInterpolate(v2, v4, d2, d4);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    doTriangle(p1, p2, p3, vertList, indexList);
                    break;
                case "1101": // p13 p34 p23
                    p1 = TetraInterpolate(v1, v3, d1, d3);
                    p2 = TetraInterpolate(v3, v4, d3, d4);
                    p3 = TetraInterpolate(v2, v3, d2, d3);
                    doTriangle(p1, p3, p2, vertList, indexList);
                    break;
                case "0010":
                    p1 = TetraInterpolate(v1, v3, d1, d3);
                    p2 = TetraInterpolate(v3, v4, d3, d4);
                    p3 = TetraInterpolate(v2, v3, d2, d3);
                    doTriangle(p1, p2, p3, vertList, indexList);
                    break;
                
                case "1011": // p12 p23 p24
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v2, v3, d2, d3);
                    p3 = TetraInterpolate(v2, v4, d2, d4);
                    doTriangle(p1, p3, p2, vertList, indexList);
                    break;
                case "0100":
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v2, v3, d2, d3);
                    p3 = TetraInterpolate(v2, v4, d2, d4);
                    doTriangle(p1, p2, p3, vertList, indexList);
                    break;
                
                case "1000": // p12 p13 p14
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v1, v3, d1, d3);
                    p3 = TetraInterpolate(v1, v4, d1, d4);
                    doTriangle(p1, p3, p2, vertList, indexList);
                    break;
                case "0111":
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v1, v3, d1, d3);
                    p3 = TetraInterpolate(v1, v4, d1, d4);
                    doTriangle(p1, p2, p3, vertList, indexList);
                    break;
                
                case "1100": // p13 p14 p24 p23
                    p1 = TetraInterpolate(v1, v3, d1, d3);
                    p2 = TetraInterpolate(v1, v4, d1, d4);
                    p3 = TetraInterpolate(v2, v4, d2, d4);
                    p4 = TetraInterpolate(v2, v3, d2, d3);
                    doQuad(p1, p2, p3, p4, true, vertList, indexList);
                    break;
                case "0011":
                    p1 = TetraInterpolate(v1, v3, d1, d3);
                    p2 = TetraInterpolate(v1, v4, d1, d4);
                    p3 = TetraInterpolate(v2, v4, d2, d4);
                    p4 = TetraInterpolate(v2, v3, d2, d3);
                    doQuad(p1, p2, p3, p4, false, vertList, indexList);
                    break;
                
                case "1010": // p12 p23 p34 p14
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v2, v3, d2, d3);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    p4 = TetraInterpolate(v1, v4, d1, d4);
                    doQuad(p1, p2, p3, p4, true, vertList, indexList);
                    break;
                case "0101":
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v2, v3, d2, d3);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    p4 = TetraInterpolate(v1, v4, d1, d4);
                    doQuad(p1, p2, p3, p4, false, vertList, indexList);
                    break;
                case "1001": // p12 p13 p34 p24
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v1, v3, d1, d3);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    p4 = TetraInterpolate(v2, v4, d2, d4);
                    doQuad(p1, p2, p3, p4, true, vertList, indexList);
                    break;
                case "0110":
                    p1 = TetraInterpolate(v1, v2, d1, d2);
                    p2 = TetraInterpolate(v1, v3, d1, d3);
                    p3 = TetraInterpolate(v3, v4, d3, d4);
                    p4 = TetraInterpolate(v2, v4, d2, d4);
                    doQuad(p1, p2, p3, p4, false, vertList, indexList);
                    break;
                default: //"0000", "1111", or (hopefully not) errors
                    break;
            }
        }

        void doTriangle(Vector3 p1, Vector3 p2, Vector3 p3, IList<Vector3> vertList, IList<int> indexList)
        {
            int index = indexList.Count -1;
            vertList.Add(p1);
            vertList.Add(p2);
            vertList.Add(p3);
            indexList.Add(index++);
            indexList.Add(index++);
            indexList.Add(index++);
        }

        void doQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, bool flip, IList<Vector3> vertList, IList<int> indexList)
        {
            if (flip)
            {
                doTriangle(p1, p4, p2, vertList, indexList);
                doTriangle(p2, p4, p3,vertList, indexList);
            }
            else
            {
                doTriangle(p1, p2, p4, vertList, indexList);
                doTriangle(p2, p3, p4, vertList, indexList);
            }
        }

        Vector3 TetraInterpolate(Vector3 p1, Vector3 p2, float d1, float d2)
        {
            float delta = (Surface - Mathf.Min(d1, d2)) / (Mathf.Max(d1, d2) - Mathf.Min(d1, d2));
            if (d2 < d1)
                delta = 1.0f - delta;

            return Vector3.Lerp(p1, p2, delta);
        }       
    }
}