using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    public class NaiveMarchingTetrahedron : Marching
    {
        private Vector3[] EdgeVertex { get; set; }
        private Vector3[] CubePosition { get; set; }
        private Vector3[] TetrahedronPosition { get; set; }
        private float[] TetrahedronValue { get; set; }

        private List<string> keyList = new List<string>(EdgeConnection.Keys);

        public NaiveMarchingTetrahedron(float surface = 0.5f) : base(surface)
        {
            EdgeVertex = new Vector3[6];
            CubePosition = new Vector3[8];
            TetrahedronPosition = new Vector3[4];
            TetrahedronValue = new float[4];
        }

        protected override void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList) // for each cube
        {
            int tetraIndex;
            //Make a local copy of the cube's corner positions
            for (tetraIndex = 0; tetraIndex < 8; tetraIndex++)
            {
                CubePosition[tetraIndex].x = x + VertexOffset[tetraIndex, 0];
                CubePosition[tetraIndex].y = y + VertexOffset[tetraIndex, 1];
                CubePosition[tetraIndex].z = z + VertexOffset[tetraIndex, 2];
            }

            // doCube(...)
            for (tetraIndex = 0; tetraIndex < 6; tetraIndex++) // for each tetra
            {
                for (int vertInTetraIndex = 0; vertInTetraIndex < 4; vertInTetraIndex++) // for each tri on tetra
                {
                    var vertexInACube = TetrahedronsInACube[tetraIndex, vertInTetraIndex];

                    TetrahedronPosition[vertInTetraIndex] = CubePosition[vertexInACube];
                    TetrahedronValue[vertInTetraIndex] = cube[vertexInACube];
                }

                MarchTetrahedron(vertList, indexList); // for each tetra
            }
        }

        private void MarchTetrahedron(IList<Vector3> vertList, IList<int> indexList)
        {
            // doTetra(...)
            string cases = "";
            //Find which vertices are inside of the surface and which are outside
            for (int i = 0; i < 4; i++)
                if (TetrahedronValue[i] <= Surface)
                    cases += '0'; else cases += '1';


            bool inverted = cases[0] == '0';
            int flagIndex = keyList.IndexOf(cases);

            int vert;
            //Find the point of intersection of the surface with each edge
            bool hasIntersections = !cases.Equals("0000") && !cases.Equals("1111");

            for (int i = 0; i < 6; i++)
            {
                if (hasIntersections)
                {
                    var vert0 = TetrahedronEdgeConnection[i, 0];
                    var vert1 = TetrahedronEdgeConnection[i, 1];

                    // Interpolation
                    float offset = GetOffset(TetrahedronValue[vert0], TetrahedronValue[vert1]);
                    float invOffset = 1.0f - offset;

                    EdgeVertex[i].x = invOffset * TetrahedronPosition[vert0].x + offset * TetrahedronPosition[vert1].x;
                    EdgeVertex[i].y = invOffset * TetrahedronPosition[vert0].y + offset * TetrahedronPosition[vert1].y;
                    EdgeVertex[i].z = invOffset * TetrahedronPosition[vert0].z + offset * TetrahedronPosition[vert1].z;

                }

            }

            if (hasIntersections)
            {
                var edgelist = EdgeConnection[cases];

                //if (edgelist.Length == 4)
                //    MakeQuad(edgelist, inverted, vertList, indexList);

                //if (edgelist.Length == 3)
                //    MakeTri(edgelist, inverted, vertList, indexList);

                for (int i = 0; i < 2; i++) // makequad
                {
                    if (edgelist.Length == 3) break;

                    int idx = vertList.Count;

                    for (int j = 0; j < 3; j++) // maketri
                    {
                        vert = TetrahedronTriangles[flagIndex, 3 * i + j];
                        indexList.Add(idx + WindingOrder[j]);
                        vertList.Add(EdgeVertex[vert]);
                    }
                }
            }


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


        private static readonly int[,] TetrahedronEdgeConnection = new int[,]
        {   //p12,  //p23   //p31=13 //p14  //p24   //p34
            {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
        };
        //private static readonly int[,] TetrahedronEdgeConnection = new int[,]
        private static readonly Dictionary<string, int[]> EdConD = new Dictionary<string, int[]>
        {
            { "p12", new[]{ TetrahedronEdgeConnection[0, 0], TetrahedronEdgeConnection[0, 1] } },
            { "p23", new[]{ TetrahedronEdgeConnection[1, 0], TetrahedronEdgeConnection[1, 1] } },
            { "p13", new[]{ TetrahedronEdgeConnection[2, 0], TetrahedronEdgeConnection[2, 1] } }, //p31
            { "p14", new[]{ TetrahedronEdgeConnection[3, 0], TetrahedronEdgeConnection[3, 1] } },
            { "p24", new[]{ TetrahedronEdgeConnection[4, 0], TetrahedronEdgeConnection[4, 1] } },
            { "p34", new[]{ TetrahedronEdgeConnection[5, 0], TetrahedronEdgeConnection[5, 1] } }
        };

        private static readonly Dictionary<string, int[][]> EdgeConnection = new Dictionary<string, int[][]>
        {
            {"0001", new[]{ EdConD["p12"], EdConD["p24"], EdConD["p34"] } },
            {"0010", new[]{ EdConD["p13"], EdConD["p34"], EdConD["p23"] } },
            {"0100", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p24"] } },
            {"0111", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p14"] } },
            {"0011", new[]{ EdConD["p13"], EdConD["p14"], EdConD["p24"], EdConD["p23"] } },
            {"0101", new[]{ EdConD["p12"], EdConD["p23"], EdConD["p34"], EdConD["p14"] } },
            {"0110", new[]{ EdConD["p12"], EdConD["p13"], EdConD["p34"], EdConD["p24"] } },

            // Flipped:
            {"1001", new[]{ EdConD["p34"], EdConD["p13"], EdConD["p12"], EdConD["p24"] } },
            {"1010", new[]{ EdConD["p34"], EdConD["p23"], EdConD["p12"], EdConD["p14"] } },
            {"1100", new[]{ EdConD["p24"], EdConD["p14"], EdConD["p13"], EdConD["p23"] } },
            {"1000", new[]{ EdConD["p14"], EdConD["p13"], EdConD["p12"] } },
            {"1011", new[]{ EdConD["p24"], EdConD["p23"], EdConD["p12"] } },
            {"1101", new[]{ EdConD["p23"], EdConD["p34"], EdConD["p13"] } },
            {"1110", new[]{ EdConD["p34"], EdConD["p24"], EdConD["p12"] } }


        };

        private static readonly int[,] TetrahedronTriangles = new int[,]
        {
            { 0,  3,  2, -1, -1, -1, -1}, //p14 p24 p34     = 03 13 23
            { 0,  1,  4, -1, -1, -1, -1}, //p13 p34 p23     = 02 23 12
            { 1,  2,  5, -1, -1, -1, -1}, //p12 p13 p14     = 01 02 03
            { 5,  4,  3, -1, -1, -1, -1}, //p12 p13 p34 p24 = 01 02 23 13

            { 0,  3,  5,  0,  5,  1, -1}, //p13 p14 p24 p23 = 02 03 13 12
            { 0,  2,  5,  0,  5,  4, -1}, //p12 p23 p34 p14 = 01 12 23 03
            { 1,  4,  2,  2,  4,  3, -1}, //p12 p23 p24     = 01 12 13

            { 3,  4,  2,  2,  4,  1, -1},
            { 4,  5,  0,  5,  2,  0, -1},
            { 1,  5,  0,  5,  3,  0, -1},

            { 3,  4,  5, -1, -1, -1, -1},
            { 5,  2,  1, -1, -1, -1, -1},
            { 4,  1,  0, -1, -1, -1, -1},
            { 2,  3,  0, -1, -1, -1, -1}
        };

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


        //private void MakeTri(int[][] edgelist, bool inverted, IList<Vector3> vertList, IList<int> indexList)
        //{
        //    int idx = vertList.Count;
        //    int vert = edgelist[0][0];
        //    for (int j = 0; j < 3; j++) // maketri
        //    {
        //        //vert = edgelist[i + j][0];
        //        //vert = TetrahedronTriangles[flagIndex, 3 * i + j];
        //        //indexList.Add(idx + WindingOrder[j]);
        //        //vertList.Add(EdgeVertex[vert]);
        //    }

        //}

        //private void MakeQuad(int[][] pts, bool inverted, IList<Vector3> vertList, IList<int> indexList)
        //{
        //    //maketri(...)
        //    //maketri(...)
        //}
    }
}