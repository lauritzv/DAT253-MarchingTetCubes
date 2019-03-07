using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace MarchingCubesProject
{
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

            // doCube(...)
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

                MarchTetrahedron(vertList, indexList); // for each tetra
                //doTetrahedron(tetravecs, tetravalues, vertList, indexList);
            }
        }


        void doTetrahedron(List<Vector3> tetravecs, List<float> tetravals, IList<Vector3> vertList, IList<int> indexList)
        {

            float d1 = tetravals[0];
            float d2 = tetravals[1];
            float d3 = tetravals[2];
            float d4 = tetravals[3];
            Vector3 v1 = tetravecs[0];
            Vector3 v2 = tetravecs[1];
            Vector3 v3 = tetravecs[2];
            Vector3 v4 = tetravecs[3];
            Vector3 p1;
            Vector3 p2;
            Vector3 p3;
            Vector3 p4;

            string segment =   (d1 > Surface ? "1" : "0")
                             + (d2 > Surface ? "1" : "0")
                             + (d3 > Surface ? "1" : "0")
                             + (d4 > Surface ? "1" : "0");

            switch (segment)
            {
                // p14 p24 p34
                case "1110":
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
                // p13 p34 p23
                case "1101":
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
                // p12 p23 p24
                case "1011":
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
                // p12 p13 p14
                case "1000":
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
                // p13 p14 p24 p23
                case "1100":
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
                // p12 p23 p34 p14
                case "1010":
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
                // p12 p13 p34 p24
                case "1001":
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
                default:
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

        
        private void MarchTetrahedron(IList<Vector3> vertList, IList<int> indexList)
        {
            // doTetra(...)
            string cases = "";
            //Find which vertices are inside of the surface and which are outside
            for (int i = 0; i < 4; i++)
                if (TetrahedronValue[i] < Surface)
                    cases += '1'; else cases += '0';


            bool inverted = cases[0] == '0';
            //int flagIndex = keyList.IndexOf(cases);

            int vert;
            //Find the point of intersection of the surface with each edge
            bool hasIntersections = !cases.Equals("0000") && !cases.Equals("1111");

            //{ "p12", 0}, { "p23", 1}, { "p13", 2}, { "p14",3}, { "p24", 4}, { "p34", 5}

            //var p12 = 

            for (int i = 0; i < 6; i++)
            {
                if (hasIntersections)
                {
                    var vert0 = TetrahedronEdgeConnection[i, 0];
                    var vert1 = TetrahedronEdgeConnection[i, 1];

                    // Interpolation

                    bool origInterp = false;


                    if (origInterp)
                    {
                        float offset = GetOffset(TetrahedronValue[vert0], TetrahedronValue[vert1]);
                        float invOffset = 1.0f - offset;

                        EdgeVertex[i].x = invOffset * TetrahedronPosition[vert0].x + offset * TetrahedronPosition[vert1].x;
                        EdgeVertex[i].y = invOffset * TetrahedronPosition[vert0].y + offset * TetrahedronPosition[vert1].y;
                        EdgeVertex[i].z = invOffset * TetrahedronPosition[vert0].z + offset * TetrahedronPosition[vert1].z;
                    }
                    else
                    {
                        EdgeVertex[i] = TetraInterpolate(
                            TetrahedronPosition[vert0],
                            TetrahedronPosition[vert1],
                            TetrahedronValue[vert0],
                            TetrahedronValue[vert1]);

                    }

                    //int idx = vertList.Count;
                    //indexList.Add(idx+WindingOrder[0]);
                    //vertList.Add(EdgeVertex[i]);
                }
            }

            if (hasIntersections)
            {
                int[] edgeIndices = CaseEdges[cases];

                if (edgeIndices.Length > 3)
                    MakeQuad(edgeIndices, inverted, vertList, indexList);

                else MakeTri(edgeIndices, inverted, vertList, indexList);

                //for (int i = 0; i < 2; i++) // makequad
                //{
                //    if (edgeIndices.Length == 3) break;
                //    int idx = vertList.Count;

                //    for (int j = 0; j < 3; j++) // maketri
                //    {
                //        if ((3 * i + j) -1 >= edgeIndices.Length)
                //        {
                //            var scream = (3 * i + j) - 1;
                //        }

                //        int edgeIndex = edgeIndices[(3 * i + j) -1];
                //        Vector3 verti = EdgeVertex[edgeIndex];
                //        //vert = TetrahedronTriangles[flagIndex, 3 * i + j];

                //        //indexList.Add(idx);
                //        indexList.Add(idx + WindingOrder[j]);
                //        vertList.Add(verti);
                //    }
                //}



            }
            //if (hasIntersections)
            //{
            //    var edgelist = EdgeConnection[cases];

            //    //if (edgelist.Length == 4)
            //    //    MakeQuad(edgelist, inverted, vertList, indexList);

            //    //if (edgelist.Length == 3)
            //    //    MakeTri(edgelist, inverted, vertList, indexList);

            //    for (int i = 0; i < 2; i++) // makequad
            //    {
            //        if (edgelist.Length == 3) break;

            //        int idx = vertList.Count;

            //        for (int j = 0; j < 3; j++) // maketri
            //        {
            //            vert = TetrahedronTriangles[flagIndex, 3 * i + j];
            //            //indexList.Add(idx);
            //            indexList.Add(idx + WindingOrder[j]);
            //            vertList.Add(EdgeVertex[vert]);
            //        }
            //    }
            //}


            //Save the triangles that were found. There can be up to 2 per tetrahedron
            //for (i = 0; i < 2; i++)
            //{
            //    if (TetrahedronTriangles[flagIndex, 3 * i] < 0) break;

            //    idx = vertList.Count;

            //    for (j = 0; j < 3; j++)
            //    {
            //        vert = TetrahedronTriangles[flagIndex, 3 * i + j];
            //        indexList.Add(idx + WindingOrder[j]);
            //        vertList.Add(EdgeVertex[vert]);
            //    }
            //}



        }
    
        private Vector3 derp(Vector3 v0, Vector3 v1, float d0, float d1)
        {
            //float delta;
            //if (d0 < d1) delta = 1f - (Surface - d0) / (d1 - d0);
            //else delta = (Surface - d1) / (d0 - d1);
            return Vector3.Lerp(v0, v1, .5f);
        }

        Vector3 TetraInterpolate(Vector3 p1, Vector3 p2, float d1, float d2)
        {
            float delta = 0.5f;
            delta = (Surface - Mathf.Min(d1, d2)) / (Mathf.Max(d1, d2) - Mathf.Min(d1, d2));
            if (d2 < d1)
                delta = 1.0f - delta;

            return Vector3.Lerp(p1, p2, delta);
        }
        

        private static readonly int[,] TetrahedronEdgeConnection = new int[,]
        {   //p12,  //p23   //p31=13 //p14  //p24   //p34
            {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
        };

        private static readonly Dictionary<string, int> TEdgeInd = new Dictionary<string, int>
        {
            {"p12", 0}, {"p23", 1}, {"p13", 2}, {"p14",3}, {"p24", 4}, {"p34", 5}
        };

        ////private static readonly int[,] TetrahedronEdgeConnection = new int[,]
        //private static readonly Dictionary<string, int[]> EdConD = new Dictionary<string, int[]>
        //{
        //    { "p12", new[]{ TetrahedronEdgeConnection[0, 0], TetrahedronEdgeConnection[0, 1] } },
        //    { "p23", new[]{ TetrahedronEdgeConnection[1, 0], TetrahedronEdgeConnection[1, 1] } },
        //    { "p13", new[]{ TetrahedronEdgeConnection[2, 0], TetrahedronEdgeConnection[2, 1] } }, //p31
        //    { "p14", new[]{ TetrahedronEdgeConnection[3, 0], TetrahedronEdgeConnection[3, 1] } },
        //    { "p24", new[]{ TetrahedronEdgeConnection[4, 0], TetrahedronEdgeConnection[4, 1] } },
        //    { "p34", new[]{ TetrahedronEdgeConnection[5, 0], TetrahedronEdgeConnection[5, 1] } }
        //};

        //private static readonly Dictionary<string, int[][]> EdgeConnection = new Dictionary<string, int[][]>
        //{
        //    {"0001", new[]{ EdConD["p12"], EdConD["p24"], EdConD["p34"] } },
        //    {"0010", new[]{ EdConD["p13"], EdConD["p34"], EdConD["p23"] } },
        //    {"0100", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p24"] } },
        //    {"0111", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p14"] } },
        //    {"0011", new[]{ EdConD["p13"], EdConD["p14"], EdConD["p24"], EdConD["p23"] } },
        //    {"0101", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p34"], EdConD["p14"] } },
        //    {"0110", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p34"], EdConD["p24"] } },

        //    // Flipped:
        //    {"1001", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p34"], EdConD["p24"] } },
        //    {"1010", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p34"], EdConD["p14"] } },
        //    {"1100", new[]{ EdConD["p13"], EdConD["p14"], EdConD["p24"], EdConD["p23"] } },
        //    {"1000", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p14"] } },
        //    {"1011", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p24"] } },
        //    {"1101", new[]{ EdConD["p13"], EdConD["p34"], EdConD["p23"] } },
        //    {"1110", new[]{ EdConD["p12"], EdConD["p24"], EdConD["p34"] } }

        //};

        private static readonly Dictionary<string, int[]> CaseEdges = new Dictionary<string, int[]>
        {
            {"0001", new[]{ TEdgeInd["p12"], TEdgeInd["p24"], TEdgeInd["p34"] } },
            {"0010", new[]{ TEdgeInd["p13"], TEdgeInd["p34"], TEdgeInd["p23"] } },
            {"0100", new[]{ TEdgeInd["p12"], TEdgeInd["p23"], TEdgeInd["p24"] } },
            {"0111", new[]{ TEdgeInd["p12"], TEdgeInd["p13"], TEdgeInd["p14"] } },
            {"0011", new[]{ TEdgeInd["p13"], TEdgeInd["p14"], TEdgeInd["p24"], TEdgeInd["p23"] } },
            {"0101", new[]{ TEdgeInd["p12"], TEdgeInd["p23"], TEdgeInd["p34"], TEdgeInd["p14"] } },
            {"0110", new[]{ TEdgeInd["p12"], TEdgeInd["p13"], TEdgeInd["p34"], TEdgeInd["p24"] } },

            {"1001", new[]{ TEdgeInd["p34"], TEdgeInd["p13"], TEdgeInd["p12"], TEdgeInd["p24"] } },
            {"1010", new[]{ TEdgeInd["p34"], TEdgeInd["p23"], TEdgeInd["p12"], TEdgeInd["p14"] } },
            {"1100", new[]{ TEdgeInd["p24"], TEdgeInd["p14"], TEdgeInd["p13"], TEdgeInd["p23"] } },
            {"1000", new[]{ TEdgeInd["p14"], TEdgeInd["p13"], TEdgeInd["p12"] } },
            {"1011", new[]{ TEdgeInd["p24"], TEdgeInd["p23"], TEdgeInd["p12"] } },
            {"1101", new[]{ TEdgeInd["p23"], TEdgeInd["p34"], TEdgeInd["p13"] } },
            {"1110", new[]{ TEdgeInd["p34"], TEdgeInd["p24"], TEdgeInd["p12"] } }

            //{"1001", new[]{ TEdgeInd["p12"], TEdgeInd["p13"], TEdgeInd["p34"], TEdgeInd["p24"] } },
            //{"1010", new[]{ TEdgeInd["p12"], TEdgeInd["p23"], TEdgeInd["p34"], TEdgeInd["p14"] } },
            //{"1100", new[]{ TEdgeInd["p13"], TEdgeInd["p14"], TEdgeInd["p24"], TEdgeInd["p23"] } },
            //{"1000", new[]{ TEdgeInd["p12"], TEdgeInd["p13"], TEdgeInd["p14"] } },
            //{"1011", new[]{ TEdgeInd["p12"], TEdgeInd["p23"], TEdgeInd["p24"] } },
            //{"1101", new[]{ TEdgeInd["p13"], TEdgeInd["p34"], TEdgeInd["p23"] } },
            //{"1110", new[]{ TEdgeInd["p12"], TEdgeInd["p24"], TEdgeInd["p34"] } }

        };

        //private static readonly int[,] TetrahedronTriangles = new int[,]
        //{
        //    { 0,  3,  2, -1, -1, -1, -1}, //p14 p24 p34     = 03 13 23
        //    { 0,  1,  4, -1, -1, -1, -1}, //p13 p34 p23     = 02 23 12
        //    { 1,  2,  5, -1, -1, -1, -1}, //p12 p13 p14     = 01 02 03
        //    { 5,  4,  3, -1, -1, -1, -1}, //p12 p13 p34 p24 = 01 02 23 13
        //    { 0,  3,  5,  0,  5,  1, -1}, //p13 p14 p24 p23 = 02 03 13 12
        //    { 0,  2,  5,  0,  5,  4, -1}, //p12 p23 p34 p14 = 01 12 23 03
        //    { 1,  4,  2,  2,  4,  3, -1}, //p12 p23 p24     = 01 12 13

        //    { 3,  4,  2,  2,  4,  1, -1},
        //    { 4,  5,  0,  5,  2,  0, -1},
        //    { 1,  5,  0,  5,  3,  0, -1},

        //    { 3,  4,  5, -1, -1, -1, -1},
        //    { 5,  2,  1, -1, -1, -1, -1},
        //    { 4,  1,  0, -1, -1, -1, -1},
        //    { 2,  3,  0, -1, -1, -1, -1}
        //};

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

        //private static readonly int[] TetrahedronEdgeFlags = new int[]
        //{
        //    0x00,   // 0000 0000
        //    0x0d,   // 0000 1101
        //    0x13,   // 0001 0011
        //    0x1e,   // 0001 1110 p13 p34 p23
        //    0x26,   // 0010 0110
        //    0x2b,   // 0010 1011
        //    0x35,   // 0011 0101
        //    0x38,   // 0011 1000

        //    // mirrored for inverted cases:

        //    0x38,   // 0011 1000
        //    0x35,   // 0011 0101
        //    0x2b,   // 0010 1011
        //    0x26,   // 0010 0110
        //    0x1e,   // 0001 1110 p14 p24 p34
        //    0x13,   // 0001 0011
        //    0x0d,   // 0000 1101
        //    0x00    // 0000 0000
        //};

        //switch (cases)
        //{
        //    case "0000":
        //    case "1111":
        //        // nothing
        //        break;
        //    case "0001":
        //    case "1110":
        //        //p14 p24 p34

        //        break;
        //    case "0010":
        //    case "1101":
        //        //p13 p34 p23
        //        break;
        //    case "0100":
        //    case "1011":
        //        //p12 p23 p24
        //        break;
        //    case "0111":
        //    case "1000":
        //        //p12 p13 p14
        //        break;
        //    case "0011":
        //    case "1100":
        //        // p13 p14 p24 p23
        //        break;
        //    case "0101":
        //    case "1010":
        //        //p12 p23 p34 p14
        //        break;
        //    case "0110":
        //    case "1001":
        //        // p12 p13 p34 p24
        //        break;
        //}


        private void MakeTri(int[] edges, bool inverted, IList<Vector3> vertList, IList<int> indexList)
        {

            for (int j = 0; j < 3; j++)
            {
                int idx = vertList.Count;
                int edgeIndex = edges[j];
                Vector3 verti = EdgeVertex[edgeIndex];
                indexList.Add(idx);
                vertList.Add(verti);
            }
            //vert = TetrahedronTriangles[flagIndex, 3 * i + 1j];

            //indexList.Add(idx);

        }

        private void MakeQuad(int[] edges, bool inverted, IList<Vector3> vertList, IList<int> indexList)
        {
            int[] edgeset1, edgeset2;
            if (!inverted)
            {
                edgeset1 = new []{edges[0], edges[1], edges[2]};
                edgeset2 = new []{edges[0], edges[2], edges[3]};
            }
            else
            {
                edgeset1 = new[] { edges[2], edges[1], edges[0] };
                edgeset2 = new[] { edges[3], edges[2], edges[0] };
            }

            MakeTri(edgeset1, inverted, vertList, indexList);
            MakeTri(edgeset2, inverted, vertList, indexList);
        }
    }
}