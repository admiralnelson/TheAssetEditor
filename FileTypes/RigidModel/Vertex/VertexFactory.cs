﻿using Filetypes.RigidModel.Vertex;
using Filetypes.RigidModel.Vertex.Formats;
using FileTypes.RigidModel.Vertex.Formats;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Filetypes.RigidModel.Vertex
{
    public interface IVertexCreator
    {
        VertexFormat Type { get; }
        CommonVertex Create(byte[] buffer, int offset, int vertexSize);
        byte[] ToBytes(CommonVertex vertex);
        uint VertexSize { get; }
    }

    public class CommonVertex
    {
        public Vector4 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 BiNormal { get; set; }
        public Vector3 Tangent { get; set; }
        public Vector2 Uv { get; set; }
        public Vector4 Colour { get; set; }

        public byte[] BoneIndex;
        public float[] BoneWeight;
        public int WeightCount { get; set; } = 0;

        public Vector3 GetPosistionAsVec3() => new Vector3(Position.X, Position.Y, Position.Z);
    }

    public class VertexFactory
    {
        Dictionary<VertexFormat, IVertexCreator> _vertexCreators = new Dictionary<VertexFormat, IVertexCreator>();
        public VertexFactory()
        {
            _vertexCreators[VertexFormat.Static] = new StaticVertexCreator();
            _vertexCreators[VertexFormat.Position16_bit] = new Position16_bitVertexCreator();

            _vertexCreators[VertexFormat.Weighted] = new WeightedVertexCreator();
            _vertexCreators[VertexFormat.Weighted_withTint] = new WeightedVertexCreator() { AddTintColour = true };

            _vertexCreators[VertexFormat.Cinematic] = new CinematicVertexCreator();
            _vertexCreators[VertexFormat.Cinematic_withTint] = new CinematicVertexCreator() { AddTintColour = true};
        }

        public static VertexFactory Create() => new VertexFactory();

        public CommonVertex[] CreateVertexFromBytes(VertexFormat format, byte[] buffer, int vertexCount, int vertexStart, int vertexSize)
        {
            var creator = _vertexCreators[format];

            var vertexList = new CommonVertex[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                vertexList[i] =  creator.Create(buffer, vertexStart + (i * vertexSize), vertexSize);
            return vertexList;
        }

        public uint GetVertexSize(VertexFormat format)
        {
            return _vertexCreators[format].VertexSize;
        }

        public byte[] Save(VertexFormat vertexType, CommonVertex vertex)
        {
            return _vertexCreators[vertexType].ToBytes(vertex);
        }
    }
}

