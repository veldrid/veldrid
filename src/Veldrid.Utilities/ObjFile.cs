using System;
using System.Collections.Generic;
using System.Numerics;

namespace Veldrid.Utilities
{
    /// <summary>
    /// Represents a parset Wavefront OBJ file.
    /// </summary>
    public class ObjFile
    {
        public Vector3[] Positions { get; }
        public Vector3[] Normals { get; }
        public Vector2[] TexCoords { get; }
        public MeshGroup[] MeshGroups { get; }
        public string? MaterialLibName { get; }

        public ObjFile(Vector3[] positions, Vector3[] normals, Vector2[] texCoords, MeshGroup[] meshGroups, string? materialLibName)
        {
            Positions = positions ?? throw new ArgumentNullException(nameof(positions));
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            TexCoords = texCoords ?? throw new ArgumentNullException(nameof(texCoords));
            MeshGroups = meshGroups ?? throw new ArgumentNullException(nameof(meshGroups));
            MaterialLibName = materialLibName;
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMesh16"/> for the given OBJ <see cref="MeshGroup"/>.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <param name="reduce">Whether to simplify the mesh by sharing identical vertices.</param>
        /// <returns>A new <see cref="ConstructedMesh16"/>.</returns>
        public ConstructedMesh16 GetMesh16(MeshGroup group, bool reduce = true)
        {
            ushort[] indices = new ushort[group.Faces.Length * 3];
            Dictionary<FaceVertex, ushort> vertexMap = new(group.Faces.Length * (reduce ? 2 : 0));
            List<VertexPositionNormalTexture> vertices = new(group.Faces.Length * (reduce ? 2 : 3));

            for (int i = 0; i < group.Faces.Length; i++)
            {
                Face face = group.Faces[i];
                ushort index0;
                ushort index1;
                ushort index2;

                if (reduce)
                {
                    index0 = GetOrCreate16(vertexMap, vertices, face.Vertex0, face.Vertex1, face.Vertex2);
                    index1 = GetOrCreate16(vertexMap, vertices, face.Vertex1, face.Vertex2, face.Vertex0);
                    index2 = GetOrCreate16(vertexMap, vertices, face.Vertex2, face.Vertex0, face.Vertex1);
                }
                else
                {
                    index0 = checked((ushort)(i * 3 + 0));
                    index1 = checked((ushort)(i * 3 + 1));
                    index2 = checked((ushort)(i * 3 + 2));
                    vertices.Add(ConstructVertex(face.Vertex0, face.Vertex1, face.Vertex2));
                    vertices.Add(ConstructVertex(face.Vertex1, face.Vertex2, face.Vertex0));
                    vertices.Add(ConstructVertex(face.Vertex2, face.Vertex0, face.Vertex1));
                }

                // Reverse winding order here.
                indices[(i * 3) + 0] = index0;
                indices[(i * 3) + 2] = index1;
                indices[(i * 3) + 1] = index2;
            }

            return new ConstructedMesh16(vertices.ToArray(), indices, group.Material);
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMesh32"/> for the given OBJ <see cref="MeshGroup"/>.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <param name="reduce">Whether to simplify the mesh by sharing identical vertices.</param>
        /// <returns>A new <see cref="ConstructedMesh16"/>.</returns>
        public ConstructedMesh32 GetMesh32(MeshGroup group, bool reduce = true)
        {
            uint[] indices = new uint[group.Faces.Length * 3];
            Dictionary<FaceVertex, uint> vertexMap = new(group.Faces.Length * (reduce ? 2 : 0));
            List<VertexPositionNormalTexture> vertices = new(group.Faces.Length * (reduce ? 2 : 3));

            for (int i = 0; i < group.Faces.Length; i++)
            {
                Face face = group.Faces[i];
                uint index0;
                uint index1;
                uint index2;

                if (reduce)
                {
                    index0 = GetOrCreate32(vertexMap, vertices, face.Vertex0, face.Vertex1, face.Vertex2);
                    index1 = GetOrCreate32(vertexMap, vertices, face.Vertex1, face.Vertex2, face.Vertex0);
                    index2 = GetOrCreate32(vertexMap, vertices, face.Vertex2, face.Vertex0, face.Vertex1);
                }
                else
                {
                    index0 = checked((uint)(i * 3 + 0));
                    index1 = checked((uint)(i * 3 + 1));
                    index2 = checked((uint)(i * 3 + 2));
                    vertices.Add(ConstructVertex(face.Vertex0, face.Vertex1, face.Vertex2));
                    vertices.Add(ConstructVertex(face.Vertex1, face.Vertex2, face.Vertex0));
                    vertices.Add(ConstructVertex(face.Vertex2, face.Vertex0, face.Vertex1));
                }

                // Reverse winding order here.
                indices[(i * 3) + 0] = index0;
                indices[(i * 3) + 2] = index1;
                indices[(i * 3) + 1] = index2;
            }

            return new ConstructedMesh32(vertices.ToArray(), indices, group.Material);
        }

        private ushort GetOrCreate16(
            Dictionary<FaceVertex, ushort> vertexMap,
            List<VertexPositionNormalTexture> vertices,
            FaceVertex key,
            FaceVertex adjacent1,
            FaceVertex adjacent2)
        {
            if (!vertexMap.TryGetValue(key, out ushort index))
            {
                VertexPositionNormalTexture vertex = ConstructVertex(key, adjacent1, adjacent2);
                vertices.Add(vertex);
                index = checked((ushort)(vertices.Count - 1));
                vertexMap.Add(key, index);
            }

            return index;
        }

        private uint GetOrCreate32(
            Dictionary<FaceVertex, uint> vertexMap,
            List<VertexPositionNormalTexture> vertices,
            FaceVertex key,
            FaceVertex adjacent1,
            FaceVertex adjacent2)
        {
            if (!vertexMap.TryGetValue(key, out uint index))
            {
                VertexPositionNormalTexture vertex = ConstructVertex(key, adjacent1, adjacent2);
                vertices.Add(vertex);
                index = checked((uint)(vertices.Count - 1));
                vertexMap.Add(key, index);
            }

            return index;
        }

        private VertexPositionNormalTexture ConstructVertex(FaceVertex key, FaceVertex adjacent1, FaceVertex adjacent2)
        {
            Vector3 position = Positions[key.PositionIndex - 1];
            Vector3 normal;
            if (key.NormalIndex == -1)
            {
                normal = ComputeNormal(key, adjacent1, adjacent2);
            }
            else
            {
                normal = Normals[key.NormalIndex - 1];
            }


            Vector2 texCoord = key.TexCoordIndex == -1 ? Vector2.Zero : TexCoords[key.TexCoordIndex - 1];

            return new VertexPositionNormalTexture(position, normal, texCoord);
        }

        private Vector3 ComputeNormal(FaceVertex v1, FaceVertex v2, FaceVertex v3)
        {
            Vector3 pos1 = Positions[v1.PositionIndex - 1];
            Vector3 pos2 = Positions[v2.PositionIndex - 1];
            Vector3 pos3 = Positions[v3.PositionIndex - 1];

            return Vector3.Normalize(Vector3.Cross(pos1 - pos2, pos1 - pos3));
        }

        /// <summary>
        /// An OBJ file construct describing an individual mesh group.
        /// </summary>
        public struct MeshGroup
        {
            /// <summary>
            /// The name.
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The name of the associated <see cref="MaterialDefinition"/>.
            /// </summary>
            public readonly string? Material;

            /// <summary>
            /// The set of <see cref="Face"/>s comprising this mesh group.
            /// </summary>
            public readonly Face[] Faces;

            /// <summary>
            /// Constructs a new <see cref="MeshGroup"/>.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="material">The name of the associated <see cref="MaterialDefinition"/>.</param>
            /// <param name="faces">The faces.</param>
            public MeshGroup(string name, string? material, Face[] faces)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Material = material;
                Faces = faces;
            }
        }

        /// <summary>
        /// An OBJ file construct describing the indices of vertex components.
        /// </summary>
        public struct FaceVertex : IEquatable<FaceVertex>
        {
            /// <summary>
            /// The index of the position component.
            /// </summary>
            public int PositionIndex;

            /// <summary>
            /// The index of the normal component.
            /// </summary>
            public int NormalIndex;

            /// <summary>
            /// The index of the texture coordinate component.
            /// </summary>
            public int TexCoordIndex;

            public FaceVertex(int positionIndex, int normalIndex, int texCoordIndex)
            {
                PositionIndex = positionIndex;
                NormalIndex = normalIndex;
                TexCoordIndex = texCoordIndex;
            }

            public readonly bool Equals(FaceVertex other)
            {
                return PositionIndex == other.PositionIndex
                    && NormalIndex == other.NormalIndex
                    && TexCoordIndex == other.TexCoordIndex;
            }

            public override readonly bool Equals(object? obj)
            {
                return obj is FaceVertex value && Equals(value);
            }

#pragma warning disable IDE0070 // Use 'System.HashCode'
            public override readonly int GetHashCode()
#pragma warning restore IDE0070 // Use 'System.HashCode'
            {
                unchecked
                {
                    int code = 17;
                    code = code * 31 + PositionIndex;
                    code = code * 31 + NormalIndex;
                    code = code * 31 + TexCoordIndex;
                    return code;
                }
            }

            public override readonly string ToString()
            {
                return string.Format("Pos:{0}, Normal:{1}, TexCoord:{2}", PositionIndex, NormalIndex, TexCoordIndex);
            }
        }

        /// <summary>
        /// An OBJ file construct describing an individual mesh face.
        /// </summary>
        public struct Face
        {
            /// <summary>
            /// The first vertex.
            /// </summary>
            public FaceVertex Vertex0;

            /// <summary>
            /// The second vertex.
            /// </summary>
            public FaceVertex Vertex1;

            /// <summary>
            /// The third vertex.
            /// </summary>
            public FaceVertex Vertex2;

            /// <summary>
            /// The smoothing group. Describes which kind of vertex smoothing should be applied.
            /// </summary>
            public int SmoothingGroup;

            public Face(FaceVertex v0, FaceVertex v1, FaceVertex v2, int smoothingGroup = -1)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;
                SmoothingGroup = smoothingGroup;
            }
        }
    }
}
