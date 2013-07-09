﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TSOClient.Code.Rendering.City
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TerrainVertex
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;
        public Vector2 BlendCoordinate;
        public Vector2 BackTextureCoordinate;


        public static int SizeInBytes = (sizeof(float) * (3 + 2 + 2 + 2)) + 4;
        public static VertexElement[] VertexElements = new VertexElement[]
        {
             new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement( 0, VertexElementFormat.Color, VertexElementUsage.Color, 0 ),
             new VertexElement( 0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( 0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement( 0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2 )
        };


        public TerrainVertex(Vector3 position, Vector2 textureCoords, Color color, Vector2 blendCoords, Vector2 backTextureCoords)
        {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinate = textureCoords;
            this.BlendCoordinate = blendCoords;
            this.BackTextureCoordinate = backTextureCoords;
        }
    }
}
