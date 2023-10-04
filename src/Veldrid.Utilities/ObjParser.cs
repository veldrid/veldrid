using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.Utilities
{
    /// <summary>
    /// A parser for Wavefront OBJ files.
    /// </summary>
    public class ObjParser
    {
        private const int InitialReadBufferSize = 1024 * 8;

        private static readonly char[] _whitespaceChar = new[] { ' ' };
        private static readonly char _slashChar = '/';

        private readonly ParseContext _pc = new();
        private char[]? _readBuffer;

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(IEnumerable<string> lines)
        {
            foreach (string line in lines)
                _pc.Process(line);

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given raw text lines.
        /// </summary>
        /// <param name="lines">The text lines of the OBJ file.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(IEnumerable<ReadOnlyMemory<char>> lines)
        {
            foreach (ReadOnlyMemory<char> line in lines)
                _pc.Process(line.Span);

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="reader">The <see cref="Stream"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(Stream stream)
        {
            using StreamReader reader = new(stream);
            return Parse(reader);
        }

        /// <summary>
        /// Parses an <see cref="ObjFile"/> from the given text stream.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read from.</param>
        /// <returns>A new <see cref="ObjFile"/>.</returns>
        public ObjFile Parse(TextReader reader)
        {
            if (_readBuffer == null)
                _readBuffer = new char[InitialReadBufferSize];

            int readIndex = 0;

            // Tries to process one or more lines inside the read buffer.
            void TryProcessLines()
            {
                Span<char> text = _readBuffer.AsSpan(0, readIndex);
                int lineEnd;
                while ((lineEnd = text.IndexOf('\n')) != -1)
                {
                    Span<char> line = text[..lineEnd];
                    if (line.Length > 0 && line[^1] == '\r')
                    {
                        line = line[..^1];
                    }

                    _pc.Process(line);
                    text = text[(lineEnd + 1)..];
                }

                // Shift back remaining data.
                int consumed = readIndex - text.Length;
                readIndex -= consumed;
                Array.Copy(_readBuffer, consumed, _readBuffer, 0, readIndex);
            }

            TryRead:
            int read;
            while ((read = reader.ReadBlock(_readBuffer, readIndex, _readBuffer.Length - readIndex)) > 0)
            {
                readIndex += read;
                TryProcessLines();
            }

            if (readIndex > 0)
            {
                if (readIndex == _readBuffer.Length)
                {
                    // The buffer couldn't contain a whole line so resize it.
                    Array.Resize(ref _readBuffer, _readBuffer.Length * 2);
                    goto TryRead;
                }

                TryProcessLines();

                // Try to parse the rest that doesn't have a line ending.
                _pc.Process(_readBuffer.AsSpan(0, readIndex));
            }

            _pc.EndOfFileReached();
            return _pc.FinalizeFile();
        }

        private class ParseContext
        {
            private List<Vector3> _positions = new();
            private List<Vector3> _normals = new();
            private List<Vector2> _texCoords = new();

            private List<ObjFile.MeshGroup> _groups = new();

            private string? _currentGroupName;
            private string? _currentMaterial;
            private int _currentSmoothingGroup;
            private List<ObjFile.Face> _currentGroupFaces = new();

            private int _currentLine;
            private string? _materialLibName;

            public void Process(ReadOnlySpan<char> line)
            {
                _currentLine++;

                ReadOnlySpanSplitter<char> splitter = new(line, _whitespaceChar, StringSplitOptions.RemoveEmptyEntries);
                if (!splitter.MoveNext())
                    return;

                ReadOnlySpan<char> piece0 = splitter.Current;
                if (piece0.StartsWith("#"))
                    return;

                if (piece0.SequenceEqual("v"))
                {
                    ExpectPieces(ref splitter, "v", true, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2, out ReadOnlySpan<char> piece3);
                    DiscoverPosition(ParseVector3(piece1, piece2, piece3, "position data"));
                }
                else if (piece0.SequenceEqual("vn"))
                {
                    ExpectPieces(ref splitter, "vn", true, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2, out ReadOnlySpan<char> piece3);
                    DiscoverNormal(ParseVector3(piece1, piece2, piece3, "normal data"));
                }
                else if (piece0.SequenceEqual("vt"))
                {
                    const string pieceName = "texture coordinate data";

                    ReadOnlySpan<char> x;
                    ReadOnlySpan<char> y = "0";

                    if (!splitter.MoveNext())
                        ThrowExpectPiecesException("one", pieceName, false);
                    x = splitter.Current;

                    if (splitter.MoveNext())
                        y = splitter.Current;

                    Vector2 texCoord = ParseVector2(x, y, pieceName);
                    // Flip v coordinate
                    texCoord.Y = 1f - texCoord.Y;
                    DiscoverTexCoord(texCoord);
                }
                else if (piece0.SequenceEqual("g"))
                {
                    ExpectPieces(ref splitter, "g", false, out ReadOnlySpan<char> piece1);
                    FinalizeGroup();
                    _currentGroupName = piece1.ToString();
                }
                else if (piece0.SequenceEqual("usemtl"))
                {
                    ExpectPieces(ref splitter, "usematl", true, out ReadOnlySpan<char> piece1);
                    if (_currentMaterial != null)
                    {
                        string nextGroupName = _currentGroupName + "_Next";
                        FinalizeGroup();
                        _currentGroupName = nextGroupName;
                    }
                    _currentMaterial = piece1.ToString();
                }
                else if (piece0.SequenceEqual("s"))
                {
                    ExpectPieces(ref splitter, "s", true, out ReadOnlySpan<char> piece1);
                    if (piece1.SequenceEqual("off"))
                        _currentSmoothingGroup = 0;
                    else
                        _currentSmoothingGroup = ParseInt(piece1, "smoothing group");
                }
                else if (piece0.SequenceEqual("f"))
                {
                    ExpectPieces(ref splitter, "f", false, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2);
                    ProcessFaceLine(ref splitter, piece1, piece2);
                }
                else if (piece0.SequenceEqual("mtllib"))
                {
                    ExpectPieces(ref splitter, "mtllib", true, out ReadOnlySpan<char> piece1);
                    DiscoverMaterialLib(piece1);
                }
                else
                {
                    throw new ObjParseException(string.Format(
                        "An unsupported line-type specifier, '{0}', was used on line {1}.",
                        piece0.ToString(),
                        _currentLine));
                }
            }

            private void DiscoverMaterialLib(ReadOnlySpan<char> libName)
            {
                if (_materialLibName != null)
                {
                    throw new ObjParseException(
                        $"mtllib appeared again in the file. It should only appear once. Line {_currentLine}.");
                }

                _materialLibName = libName.ToString();
            }

            private void ProcessFaceLine(
                ref ReadOnlySpanSplitter<char> splitter,
                ReadOnlySpan<char> piece1, ReadOnlySpan<char> piece2)
            {
                ObjFile.FaceVertex faceVertex0 = ParseFaceVertex(piece1);

                while (splitter.MoveNext())
                {
                    ReadOnlySpan<char> piece3 = splitter.Current;
                    ObjFile.FaceVertex faceVertex1 = ParseFaceVertex(piece2);
                    ObjFile.FaceVertex faceVertex2 = ParseFaceVertex(piece3);

                    DiscoverFace(new ObjFile.Face(faceVertex0, faceVertex1, faceVertex2, _currentSmoothingGroup));
                    piece2 = piece3;
                }
            }

            private ObjFile.FaceVertex ParseFaceVertex(ReadOnlySpan<char> faceComponents)
            {
                if (faceComponents.IsEmpty)
                    ThrowExceptionForWrongFaceCount("There must be at least one face component");
                
                int firstSlash = faceComponents.IndexOf(_slashChar);
                ReadOnlySpan<char> firstSlice = firstSlash == -1
                    ? faceComponents
                    : faceComponents[..firstSlash];

                ReadOnlySpan<char> afterFirstSlash = faceComponents[(firstSlash + 1)..];
                int secondSlash = afterFirstSlash.IndexOf(_slashChar);
                ReadOnlySpan<char> secondSlice = secondSlash == -1
                    ? afterFirstSlash
                    : afterFirstSlash[..secondSlash];

                ReadOnlySpan<char> afterSecondSlash = afterFirstSlash[(secondSlash + 1)..];
                int thirdSlash = afterSecondSlash.IndexOf(_slashChar);
                if (thirdSlash != -1)
                    ThrowExceptionForWrongFaceCount("No more than three face components are allowed");
                ReadOnlySpan<char> thirdSlice = afterSecondSlash;

                int position = ParseInt(firstSlice, "the first face position index");
                int texCoord = firstSlash == -1 ? -1 : ParseInt(secondSlice, "the first face texture coordinate index");
                int normal = secondSlash == -1 ? -1 : ParseInt(thirdSlice, "the first face normal index");

                return new ObjFile.FaceVertex(position, normal, texCoord);
            }

            [DoesNotReturn]
            private void ThrowExceptionForWrongFaceCount(string message)
            {
                throw new ObjParseException($"{message}, on line {_currentLine}.");
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void FinalizeFaceVertex(
                int positionOffset, int normalOffset, int texCoordOffset,
                ref ObjFile.FaceVertex vertex)
            {
                if (vertex.PositionIndex < 0)
                    vertex.PositionIndex += positionOffset;

                if (vertex.NormalIndex < 0)
                    vertex.NormalIndex += normalOffset;

                if (vertex.TexCoordIndex < 0)
                    vertex.TexCoordIndex += texCoordOffset;
            }

            public void FinalizeGroup()
            {
                if (_currentGroupName != null)
                {
                    int positionOffset = _positions.Count + 1;
                    int normalOffset = _normals.Count + 1;
                    int texCoordOffset = _texCoords.Count + 1;

                    ObjFile.Face[] faces = _currentGroupFaces.ToArray();
                    for (int i = 0; i < faces.Length; i++)
                    {
                        ref ObjFile.Face face = ref faces[i];
                        FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, ref face.Vertex0);
                        FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, ref face.Vertex1);
                        FinalizeFaceVertex(positionOffset, normalOffset, texCoordOffset, ref face.Vertex2);
                    }

                    _groups.Add(new ObjFile.MeshGroup(_currentGroupName, _currentMaterial, faces));

                    _currentGroupName = null;
                    _currentMaterial = null;
                    _currentSmoothingGroup = -1;
                    _currentGroupFaces.Clear();
                }
            }

            public void EndOfFileReached()
            {
                _currentGroupName ??= "GlobalFileGroup";
                FinalizeGroup();
            }

            public ObjFile FinalizeFile()
            {
                return new ObjFile(_positions.ToArray(), _normals.ToArray(), _texCoords.ToArray(), _groups.ToArray(), _materialLibName);
            }

            private Vector3 ParseVector3(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, ReadOnlySpan<char> zStr, string location)
            {
                if (FastParse.TryParseDouble(xStr, out double x) &&
                    FastParse.TryParseDouble(yStr, out double y) &&
                    FastParse.TryParseDouble(zStr, out double z))
                {
                    return new Vector3((float)x, (float)y, (float)z);
                }
                ThrowParseException(location);
                return default;
            }

            private Vector2 ParseVector2(ReadOnlySpan<char> xStr, ReadOnlySpan<char> yStr, string location)
            {
                if (FastParse.TryParseDouble(xStr, out double x) &&
                    FastParse.TryParseDouble(yStr, out double y))
                {
                    return new Vector2((float)x, (float)y);
                }
                ThrowParseException(location);
                return default;
            }

            private int ParseInt(ReadOnlySpan<char> intStr, string location)
            {
                if (FastParse.TryParseInt(intStr, out int result))
                {
                    return result;
                }
                ThrowParseException(location);
                return default;
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece0, out ReadOnlySpan<char> piece1, out ReadOnlySpan<char> piece2)
            {
                if (!pieces.MoveNext())
                    goto Fail;
                piece0 = pieces.Current;

                if (!pieces.MoveNext())
                    goto Fail;
                piece1 = pieces.Current;

                if (!pieces.MoveNext())
                    goto Fail;
                piece2 = pieces.Current;

                if (!exact || !pieces.MoveNext())
                    return;

                Fail:
                ThrowExpectPiecesException("three", name, exact);
                piece0 = default;
                piece1 = default;
                piece2 = default;
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece0, out ReadOnlySpan<char> piece1)
            {
                if (!pieces.MoveNext())
                    goto Fail;
                piece0 = pieces.Current;

                if (!pieces.MoveNext())
                    goto Fail;
                piece1 = pieces.Current;

                if (!exact || !pieces.MoveNext())
                    return;

                Fail:
                ThrowExpectPiecesException("three", name, exact);
                piece0 = default;
                piece1 = default;
            }

            private void ExpectPieces(
                ref ReadOnlySpanSplitter<char> pieces, string name, bool exact,
                out ReadOnlySpan<char> piece)
            {
                if (!pieces.MoveNext())
                    goto Fail;
                piece = pieces.Current;

                if (!exact || !pieces.MoveNext())
                    return;

                Fail:
                ThrowExpectPiecesException("one", name, exact);
                piece = default;
            }

            [DoesNotReturn]
            private void ThrowExpectPiecesException(string amount, string name, bool exact)
            {
                string message = string.Format(
                    "Expected {0} {1} components to a line starting with {2}, on line {3}.",
                    exact ? "exact" : "at least",
                    amount,
                    name,
                    _currentLine);
                throw new ObjParseException(message);
            }

            [DoesNotReturn]
            private void ThrowParseException(string location)
            {
                string message = string.Format("An error ocurred while parsing {0} on line {1}.", location, _currentLine);
                throw new ObjParseException(message, new FormatException());
            }
        }
    }
}
