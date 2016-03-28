using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace Veldrid.Graphics
{
    public class ObjImporter
    {
        public static ObjMeshInfo LoadFromPath(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return Import(fs).Result;
            }
        }

        public static async Task<ObjMeshInfo> Import(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoords = new List<Vector2>();
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();

            int lastIndexUsed = -1;
            Dictionary<ObjVertex, int> objVertexIndices = new Dictionary<ObjVertex, int>();

            while (!sr.EndOfStream)
            {
                string line = await sr.ReadLineAsync();
                if (line.StartsWith("#"))
                {
                    continue;
                }
                else if (line.StartsWith("v "))
                {
                    string[] split = line.Split(' ');
                    positions.Add(ParseVector3FromLine(split));
                }
                else if (line.StartsWith("vt "))
                {
                    string[] split = line.Split(' ');
                    textureCoords.Add(ParseVector2FromLine(split));
                }
                else if (line.StartsWith("vn "))
                {
                    string[] split = line.Split(' ');
                    normals.Add(ParseVector3FromLine(split));
                }
                else if (line.StartsWith("f"))
                {
                    string[] words = line.Split(' ');

                    ObjVertex[] objVertices = new ObjVertex[3];
                    for (int i = 0; i < words.Length - 3; i++)
                    {
                        var v1Split = words[1].Split('/');
                        var v2Split = words[i + 2].Split('/');
                        var v3Split = words[i + 3].Split('/');

                        ObjVertex v1 = ParseObjVertexFromElements(v1Split, positions, normals, textureCoords);
                        ObjVertex v2 = ParseObjVertexFromElements(v2Split, positions, normals, textureCoords);
                        ObjVertex v3 = ParseObjVertexFromElements(v3Split, positions, normals, textureCoords);
                        // Convert from counter-clockwise winding -> clockwise
                        objVertices[2] = v1;
                        objVertices[1] = v2;
                        objVertices[0] = v3;

                        foreach (ObjVertex objV in objVertices)
                        {
                            int vertexIndex;
                            if (!objVertexIndices.TryGetValue(objV, out vertexIndex))
                            {
                                vertexIndex = ++lastIndexUsed;
                                objVertexIndices.Add(objV, vertexIndex);

                                var position = positions[objV.Position];
                                var normal = objV.Normal != -2
                                        ? normals[objV.Normal]
                                        : normals.Count == 0
                                            ? ComputeNormal(positions, v1, v2, v3)
                                            : Vector3.Zero;
                                var texCoord = objV.TextureCoord != -2 ? textureCoords[objV.TextureCoord] : Vector2.Zero;

                                var vertex = new VertexPositionNormalTexture(position, normal,texCoord);
                                vertices.Add(vertex);
                                Debug.Assert(vertices.Count == lastIndexUsed + 1);
                            }

                            indices.Add(vertexIndex);
                        }
                    }
                }
            }

            return new ObjMeshInfo(vertices.ToArray(), indices.ToArray());
        }

        private static Vector3 ComputeNormal(List<Vector3> positions, ObjVertex v1, ObjVertex v2, ObjVertex v3)
        {
            Vector3 pos1 = positions[v1.Position];
            Vector3 pos2 = positions[v2.Position];
            Vector3 pos3 = positions[v3.Position];

            return Vector3.Normalize(Vector3.Cross(pos1 - pos2, pos1 - pos3));
        }

        private static ObjVertex ParseObjVertexFromElements(string[] elements, List<Vector3> positions, List<Vector3> normals, List<Vector2> textureCoords)
        {
            if (elements.Length < 1)
            {
                throw new NotSupportedException("Can't parse this obj file");
            }

            int posIndex = -1;
            int texIndex = -1;
            int normalIndex = -1;

            // Vertex Position is not optional.
            posIndex = int.Parse(elements[0]);

            // Texture-coordinates and normal are optional.
            if (elements.Length >= 2 && !string.IsNullOrWhiteSpace(elements[1]))
            {
                texIndex = int.Parse(elements[1]);
            }
            if (elements.Length >= 3 && !string.IsNullOrWhiteSpace(elements[2]))
            {
                normalIndex = int.Parse(elements[2]);
            }

            return new ObjVertex(posIndex, normalIndex, texIndex);
        }

        private static Vector3 ParseVector3FromLine(string[] words)
        {
            return new Vector3(
                float.Parse(words[1]),
                float.Parse(words[2]),
                float.Parse(words[3]));
        }

        private static Vector2 ParseVector2FromLine(string[] words)
        {
            return new Vector2(
                float.Parse(words[1]),
                float.Parse(words[2]));
        }
    }

    public class ObjMeshInfo
    {
        public VertexPositionNormalTexture[] Vertices { get; }
        public int[] Indices { get; }

        public ObjMeshInfo(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }

    internal struct ObjVertex
    {
        public readonly int Position;
        public readonly int Normal;
        public readonly int TextureCoord;

        public ObjVertex(int pos, int normal, int texCoord)
        {
            Position = pos - 1;
            Normal = normal - 1;
            TextureCoord = texCoord - 1;
        }
    }
}
