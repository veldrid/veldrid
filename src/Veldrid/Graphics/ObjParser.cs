using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using static Veldrid.SpanHelpers;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A parser for Wavefront OBJ files.
    /// </summary>
    public class ObjParser
    {
        private static readonly string[] s_newline = new string[] { Environment.NewLine };
        private static readonly char[] s_whitespaceChars = new char[] { ' ' };
        private static readonly char[] s_slashChar = new char[] { '/' };

        private readonly ParseContext _pc = new ParseContext();

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="text">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(string text)
        {
            return Parse(text.AsSpan());
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="text">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(ReadOnlySpan<char> text)
        {
            ReadOnlySpan<char> currentChunk = text;
            int lineEnd = -1;
            while ((lineEnd = currentChunk.IndexOf('\n')) != -1)
            {
                ReadOnlySpan<char> lineChunk;
                if (lineEnd != 0 && currentChunk[lineEnd - 1] == '\r')
                {
                    lineChunk = currentChunk.Slice(0, lineEnd - 1);
                }
                else
                {
                    lineChunk = currentChunk.Slice(0, lineEnd);
                }
                _pc.Process(lineChunk);
                currentChunk = currentChunk.Slice(lineEnd + 1);
            }

            _pc.EndOfFileReached();

            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                string allText = sr.ReadToEnd();
                return Parse(allText.AsSpan());
            }
        }

        private class ParseContext
        {
            private readonly ReadOnlySpan<char>[] _pieces = new ReadOnlySpan<char>[10];
            private readonly ReadOnlySpan<char>[] _slashSplit = new ReadOnlySpan<char>[10];

            private List<Vector3> _positions = new List<Vector3>();
            private List<Vector3> _normals = new List<Vector3>();
            private List<Vector2> _texCoords = new List<Vector2>();

            private List<ObjFile.MeshGroup> _groups = new List<ObjFile.MeshGroup>();

            private ReadOnlySpan<char> _currentGroupName;
            private ReadOnlySpan<char> _currentMaterial;
            private int _currentSmoothingGroup;
            private List<ObjFile.Face> _currentGroupFaces = new List<ObjFile.Face>();

            private int _currentLine;
            private ReadOnlySpan<char> _currentLineText;

            private string _materialLibName;

            private static readonly ReadOnlySpan<char> _span_globalFileGroup = new ReadOnlySpan<char>("GlobalFileGroup".ToCharArray());
            private static readonly ReadOnlySpan<char> _span_off = new ReadOnlySpan<char>(new char[] { 'o', 'f', 'f' });
            private static readonly ReadOnlySpan<char> _span_v = new ReadOnlySpan<char>(new char[] { 'v' });
            private static readonly ReadOnlySpan<char> _span_vn = new ReadOnlySpan<char>(new char[] { 'v', 'n' });
            private static readonly ReadOnlySpan<char> _span_vt = new ReadOnlySpan<char>(new char[] { 'v', 't' });
            private static readonly ReadOnlySpan<char> _span_g = new ReadOnlySpan<char>(new char[] { 'g' });
            private static readonly ReadOnlySpan<char> _span_usemtl = new ReadOnlySpan<char>(new char[] { 'u', 's', 'e', 'm', 't', 'l' });
            private static readonly ReadOnlySpan<char> _span_s = new ReadOnlySpan<char>(new char[] { 's' });
            private static readonly ReadOnlySpan<char> _span_f = new ReadOnlySpan<char>(new char[] { 'f' });
            private static readonly ReadOnlySpan<char> _span_mtllib = new ReadOnlySpan<char>(new char[] { 'm', 't', 'l', 'l', 'i', 'b' });

            public void Process(ReadOnlySpan<char> line)
            {
                _currentLine++;
                _currentLineText = line;

                SplitBy(_pieces, line, ' ', out int numItemsSplit);

                if (numItemsSplit == 0 || _pieces[0].IsEmpty || _pieces[0][0] == '#')
                {
                    return;
                }

                if (_pieces[0].SequenceEqual(_span_v))
                {
                    ExpectExactly(numItemsSplit, 3, "v");
                    DiscoverPosition(ParseVector3(_pieces[1], _pieces[2], _pieces[3], "position data"));
                }
                else if (_pieces[0].SequenceEqual(_span_vn))
                {
                    ExpectExactly(numItemsSplit, 3, "vn");
                    DiscoverNormal(ParseVector3(_pieces[1], _pieces[2], _pieces[3], "normal data"));
                }
                else if (_pieces[0].SequenceEqual(_span_vt))
                {
                    ExpectAtLeast(numItemsSplit, 1, "vt");
                    Vector2 texCoord = ParseVector2(_pieces[1], _pieces[2], "texture coordinate data");
                    // Flip v coordinate
                    texCoord.Y = 1f - texCoord.Y;
                    DiscoverTexCoord(texCoord);
                }
                else if (_pieces[0].SequenceEqual(_span_g))
                {
                    ExpectAtLeast(numItemsSplit, 1, "g");
                    FinalizeGroup();
                    _currentGroupName = line.Slice(1, line.Length - 1);
                }
                else if (_pieces[0].SequenceEqual(_span_usemtl))
                {
                    ExpectExactly(numItemsSplit, 1, "usematl");
                    if (!_currentMaterial.IsEmpty)
                    {
                        string nextGroupName = _currentGroupName + "_Next";
                        FinalizeGroup();
                        _currentGroupName = nextGroupName.AsSpan();
                    }
                    _currentMaterial = _pieces[1];
                }
                else if (_pieces[0].SequenceEqual(_span_s))
                {
                    ExpectExactly(numItemsSplit, 1, "s");
                    if (_pieces[1].SequenceEqual(_span_off))
                    {
                        _currentSmoothingGroup = 0;
                    }
                    else
                    {
                        _currentSmoothingGroup = ParseInt(_pieces[1], "smoothing group");
                    }
                }
                else if (_pieces[0].SequenceEqual(_span_f))
                {
                    ExpectAtLeast(numItemsSplit, 3, "f");
                    ProcessFaceLine(_pieces, numItemsSplit);
                }
                else if (_pieces[0].SequenceEqual(_span_mtllib))
                {
                    ExpectExactly(numItemsSplit, 1, "mtllib");
                    DiscoverMaterialLib(_pieces[1]);
                }
                else
                {
                    throw new ObjParseException(
                        string.Format("An unsupported line-type specifier, '{0}', was used on line {1}, \"{2}\"",
                        GetString(_pieces[0]),
                        _currentLine,
                        GetString(_currentLineText)));
                }
            }

            private void SplitBy(ReadOnlySpan<char>[] destination, ReadOnlySpan<char> span, char value, out int numItemsSplit)
            {
                Array.Clear(destination, 0, destination.Length);
                numItemsSplit = 0;
                int destIndex = 0;
                int currentChunkEnd = -1;
                int nextChunkStart = int.MaxValue;
                bool endReached = false;
                while ((currentChunkEnd = span.IndexOf(value)) != -1 && !endReached)
                {
                    nextChunkStart = currentChunkEnd;
                    endReached = (nextChunkStart + 1 == span.Length);
                    // Keep seeking forward until a different char is ecountered.
                    while (nextChunkStart + 1 < span.Length)
                    {
                        if (span[nextChunkStart + 1] == value)
                        {
                            nextChunkStart += 1;
                            if (nextChunkStart == span.Length)
                            {
                                endReached = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    numItemsSplit += 1;
                    Debug.Assert(destIndex < destination.Length);
                    destination[destIndex] = span.Slice(0, currentChunkEnd);
                    destIndex += 1;
                    span = span.Slice(nextChunkStart + 1);
                }

                if (!endReached)
                {
                    // End of file -- no separator encountered since last chunk.
                    numItemsSplit += 1;
                    destination[destIndex] = span;
                }
            }

            private void DiscoverMaterialLib(ReadOnlySpan<char> libName)
            {
                if (_materialLibName != null)
                {
                    throw new ObjParseException(
                        string.Format("mtllib appeared again in the file. It should only appear once. Line {0}, \"{1}\"", _currentLine, GetString(_currentLineText)));
                }

                _materialLibName = GetString(libName);
            }

            private void ProcessFaceLine(ReadOnlySpan<char>[] pieces, int count)
            {
                ReadOnlySpan<char> first = pieces[1];
                ObjFile.FaceVertex faceVertex0 = ParseFaceVertex(first);

                for (int i = 0; i < count - 3; i++)
                {
                    ReadOnlySpan<char> second = pieces[i + 2];
                    ObjFile.FaceVertex faceVertex1 = ParseFaceVertex(second);
                    ReadOnlySpan<char> third = pieces[i + 3];
                    ObjFile.FaceVertex faceVertex2 = ParseFaceVertex(third);

                    DiscoverFace(new ObjFile.Face(faceVertex0, faceVertex1, faceVertex2, _currentSmoothingGroup));
                }
            }

            private ObjFile.FaceVertex ParseFaceVertex(ReadOnlySpan<char> faceComponents)
            {
                SplitBy(_slashSplit, faceComponents, '/', out int numItems);

                if (numItems != 1 && numItems != 2 && numItems != 3)
                {
                    throw CreateExceptionForWrongFaceCount(numItems);
                }

                int pos = ParseInt(_slashSplit[0], "the first face position index");

                int texCoord = -1;
                if (numItems >= 2 && !_slashSplit[1].IsEmpty)
                {
                    texCoord = ParseInt(_slashSplit[1], "the first face texture coordinate index");
                }

                int normal = -1;
                if (_slashSplit.Length == 3)
                {
                    normal = ParseInt(_slashSplit[2], "the first face normal index");
                }

                return new ObjFile.FaceVertex() { PositionIndex = pos, NormalIndex = normal, TexCoordIndex = texCoord };
            }

            private ObjParseException CreateExceptionForWrongFaceCount(int count)
            {
                return new ObjParseException(
                    string.Format("Expected 1, 2, or 3 face components, but got {0}, on line {1}, \"{2}\"",
                    count,
                    _currentLine,
                    GetString(_currentLineText)));
            }

            public void DiscoverPosition(Vector3 position)
            {
                _positions.Add(position);
            }

            public void DiscoverNormal(Vector3 normal)
            {
                _normals.Add(normal);
            }

            public void DiscoverTexCoord(Vector2 texCoord)
            {
                _texCoords.Add(texCoord);
            }

            public void DiscoverFace(ObjFile.Face face)
            {
                _currentGroupFaces.Add(face);
            }

            public void FinalizeGroup()
            {
                if (!_currentGroupName.IsEmpty)
                {
                    ObjFile.Face[] faces = _currentGroupFaces.ToArray();
                    _groups.Add(new ObjFile.MeshGroup(GetString(_currentGroupName), GetString(_currentMaterial), faces));

                    _currentGroupName = ReadOnlySpan<char>.Empty;
                    _currentMaterial = ReadOnlySpan<char>.Empty;
                    _currentSmoothingGroup = -1;
                    _currentGroupFaces.Clear();
                }
            }

            public void EndOfFileReached()
            {
                _currentGroupName = !_currentGroupName.IsEmpty ? _currentGroupName : _span_globalFileGroup;
                _groups.Add(new ObjFile.MeshGroup(GetString(_currentGroupName), GetString(_currentMaterial), _currentGroupFaces.ToArray()));
            }

            public ObjFile FinalizeFile()
            {
                return new ObjFile(_positions.ToArray(), _normals.ToArray(), _texCoords.ToArray(), _groups.ToArray(), _materialLibName);
            }

            private Vector3 ParseVector3(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, ReadOnlySpan<char> zStr, string location)
            {
                try
                {
                    float x = ParseFloat(xStr);
                    float y = ParseFloat(yStr);
                    float z = ParseFloat(zStr);

                    return new Vector3(x, y, z);
                }
                catch (FormatException fe) when (!Debugger.IsAttached)
                {
                    throw CreateParseException(location, fe);
                }
            }

            private Vector2 ParseVector2(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, string location)
            {
                try
                {
                    float x = ParseFloat(xStr);
                    float y = ParseFloat(yStr);

                    return new Vector2(x, y);
                }
                catch (FormatException fe)
                {
                    throw CreateParseException(location, fe);
                }
            }

            private int ParseInt(ReadOnlySpan<char> intStr, string location)
            {
                if (!PrimitiveParser.InvariantUtf16.TryParseInt32(intStr, out int value))
                {
                    throw new ObjParseException($"Encountered an invalid int at {location}.");
                }

                return value;
            }

            // TODO: Remove this when System.Text.Primitives supports this.
            private float ParseFloat(ReadOnlySpan<char> text)
            {
                return float.Parse(GetString(text));
            }

            private uint ParseUInt32(ReadOnlySpan<char> text)
            {
                if (!PrimitiveParser.InvariantUtf16.TryParseUInt32(text, out uint value, out int _))
                {
                    throw new ObjParseException($"Invalid float value on line {_currentLine}, text: {_currentLineText}");
                }

                return value;
            }

            private void ExpectExactly(int pieces, int expectedCount, string name)
            {
                if (pieces != expectedCount + 1)
                {
                    string message = string.Format(
                        "Expected exactly {0} components to a line starting with {1}, on line {2}, \"{3}\".",
                        expectedCount,
                        name,
                        _currentLine,
                        GetString(_currentLineText));
                    throw new ObjParseException(message);
                }
            }

            private void ExpectAtLeast(int pieces, int expectedCount, string name)
            {
                if (pieces < expectedCount + 1)
                {
                    string message = string.Format(
                        "Expected at least {0} components to a line starting with {1}, on line {2}, \"{3}\".",
                        expectedCount,
                        name,
                        _currentLine,
                        GetString(_currentLineText));
                    throw new ObjParseException(message);
                }
            }

            private ObjParseException CreateParseException(string location, FormatException fe)
            {
                string message = string.Format("An error ocurred while parsing {0} on line {1}, \"{2}\"", location, _currentLine, GetString(_currentLineText));
                return new ObjParseException(message, fe);
            }
        }
    }

    /// <summary>
    /// An parsing error for Wavefront OBJ files.
    /// </summary>
    public class ObjParseException : Exception
    {
        public ObjParseException(string message) : base(message)
        {
        }

        public ObjParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents a parset Wavefront OBJ file.
    /// </summary>
    public class ObjFile
    {
        public Vector3[] Positions { get; }
        public Vector3[] Normals { get; }
        public Vector2[] TexCoords { get; }
        public MeshGroup[] MeshGroups { get; }
        public string MaterialLibName { get; }

        public ObjFile(Vector3[] positions, Vector3[] normals, Vector2[] texCoords, MeshGroup[] meshGroups, string materialLibName)
        {
            Positions = positions;
            Normals = normals;
            TexCoords = texCoords;
            MeshGroups = meshGroups;
            MaterialLibName = materialLibName;
        }

        /// <summary>
        /// Gets a <see cref="ConstructedMeshInfo"/> for the given OBJ <see cref="MeshGroup"/>.
        /// </summary>
        /// <param name="group">The OBJ <see cref="MeshGroup"/> to construct.</param>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetMesh(MeshGroup group)
        {
            Dictionary<FaceVertex, ushort> vertexMap = new Dictionary<FaceVertex, ushort>();
            ushort[] indices = new ushort[group.Faces.Length * 3];
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < group.Faces.Length; i++)
            {
                Face face = group.Faces[i];
                ushort index0 = GetOrCreate(vertexMap, vertices, face.Vertex0, face.Vertex1, face.Vertex2);
                ushort index1 = GetOrCreate(vertexMap, vertices, face.Vertex1, face.Vertex2, face.Vertex0);
                ushort index2 = GetOrCreate(vertexMap, vertices, face.Vertex2, face.Vertex0, face.Vertex1);

                // Reverse winding order here.
                indices[(i * 3)] = index0;
                indices[(i * 3) + 2] = index1;
                indices[(i * 3) + 1] = index2;
            }

            return new ConstructedMeshInfo(vertices.ToArray(), indices, group.Material);
        }

        /// <summary>
        /// Constructs the first <see cref="MeshGroup"/> in this file.
        /// </summary>
        /// <returns>A new <see cref="ConstructedMeshInfo"/>.</returns>
        public ConstructedMeshInfo GetFirstMesh()
        {
            return GetMesh(MeshGroups[0]);
        }

        private ushort GetOrCreate(
            Dictionary<FaceVertex, ushort> vertexMap,
            List<VertexPositionNormalTexture> vertices,
            FaceVertex key,
            FaceVertex adjacent1,
            FaceVertex adjacent2)
        {
            ushort index;
            if (!vertexMap.TryGetValue(key, out index))
            {
                VertexPositionNormalTexture vertex = ConstructVertex(key, adjacent1, adjacent2);
                vertices.Add(vertex);
                index = checked((ushort)(vertices.Count - 1));
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
            public readonly string Material;

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
            public MeshGroup(string name, string material, Face[] faces)
            {
                Name = name;
                Material = material;
                Faces = faces;
            }
        }

        /// <summary>
        /// An OBJ file construct describing the indices of vertex components.
        /// </summary>
        public struct FaceVertex
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

            public override string ToString()
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
            public readonly FaceVertex Vertex0;

            /// <summary>
            /// The second vertex.
            /// </summary>
            public readonly FaceVertex Vertex1;

            /// <summary>
            /// The third vertex.
            /// </summary>
            public readonly FaceVertex Vertex2;

            /// <summary>
            /// The smoothing group. Describes which kind of vertex smoothing should be applied.
            /// </summary>
            public readonly int SmoothingGroup;

            public Face(FaceVertex v0, FaceVertex v1, FaceVertex v2, int smoothingGroup = -1)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;

                SmoothingGroup = smoothingGroup;
            }
        }
    }

    /// <summary>
    /// A standalone <see cref="MeshData"/> created from information from an <see cref="ObjFile"/>.
    /// </summary>
    public class ConstructedMeshInfo : MeshData
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public VertexPositionNormalTexture[] Vertices { get; }

        /// <summary>
        /// The indices of the mesh.
        /// </summary>
        public ushort[] Indices { get; }

        /// <summary>
        /// The name of the <see cref="MaterialDefinition"/> associated with this mesh.
        /// </summary>
        public string MaterialName { get; }

        /// <summary>
        /// Constructs a new <see cref="ConstructedMeshInfo"/>.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="indices">The indices.</param>
        /// <param name="materialName">The name of the associated MTL <see cref="MaterialDefinition"/>.</param>
        public ConstructedMeshInfo(VertexPositionNormalTexture[] vertices, ushort[] indices, string materialName)
        {
            Vertices = vertices;
            Indices = indices;
            MaterialName = materialName;
        }

        public VertexBuffer CreateVertexBuffer(ResourceFactory factory)
        {
            var vb = factory.CreateVertexBuffer(Vertices.Length * VertexPositionNormalTexture.SizeInBytes, false);
            vb.SetVertexData(Vertices, new VertexDescriptor(VertexPositionNormalTexture.SizeInBytes, 3, 0, IntPtr.Zero));
            return vb;
        }

        public IndexBuffer CreateIndexBuffer(ResourceFactory factory, out int indexCount)
        {
            IndexBuffer ib = factory.CreateIndexBuffer(Indices.Length * sizeof(int), false);
            ib.SetIndices(Indices);
            indexCount = Indices.Length;
            return ib;
        }

        public BoundingSphere GetBoundingSphere()
        {
            return BoundingSphere.CreateFromPoints(Vertices);
        }

        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.CreateFromVertices(Vertices);
        }

        public bool RayCast(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            bool result = false;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                float newDistance;
                if (ray.Intersects(ref v0, ref v1, ref v2, out newDistance))
                {
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                    }

                    result = true;
                }
            }

            return result;
        }

        public int RayCast(Ray ray, List<float> distances)
        {
            int hits = 0;
            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                Vector3 v0 = Vertices[Indices[i + 0]].Position;
                Vector3 v1 = Vertices[Indices[i + 1]].Position;
                Vector3 v2 = Vertices[Indices[i + 2]].Position;

                float newDistance;
                if (ray.Intersects(ref v0, ref v1, ref v2, out newDistance))
                {
                    hits++;
                    distances.Add(newDistance);
                }
            }

            return hits;
        }

        public Vector3[] GetVertexPositions()
        {
            return Vertices.Select(vpnt => vpnt.Position).ToArray();
        }

        public ushort[] GetIndices()
        {
            return Indices;
        }
    }
}
