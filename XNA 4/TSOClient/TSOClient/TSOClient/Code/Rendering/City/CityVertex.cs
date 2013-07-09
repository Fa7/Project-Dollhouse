﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace TSOClient.Code.Rendering.City
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CityVertex
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;
        public Vector4 TextureWeight1;
        public Vector4 TextureWeight2;

        public static int SizeInBytes = (sizeof(float) * (3 + 2 + 4 + 4)) + 4;
        public static VertexElement[] VertexElements = new VertexElement[]
        {
             new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement( 0, VertexElementFormat.Color, VertexElementUsage.Color, 0 ),
             new VertexElement( 0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 ),
             new VertexElement( 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2 )
        };

        public CityVertex(Vector3 position, Vector2 textureCoords, Color color, TerrainType terrain)
        {
            this.Position = position;
            this.Color = color;
            this.TextureCoordinate = textureCoords;
            this.TextureWeight1 = new Vector4(terrain == TerrainType.Grass ? 1 : 0,
                                              terrain == TerrainType.Snow ? 1 : 0,
                                              terrain == TerrainType.Sand ? 1 : 0,
                                              terrain == TerrainType.Rock ? 1 : 0);

            this.TextureWeight2 = new Vector4(terrain == TerrainType.Water ? 1 : 0, 0, 0, 0);
        }
    }
}
