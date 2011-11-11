#region File Description
//-----------------------------------------------------------------------------
// Quad.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace SnowGlobe
{
    public struct Distortion
    {
        public Vector3 Origin;
        public Vector3 UpperLeft;
        public Vector3 LowerLeft;
        public Vector3 UpperRight;
        public Vector3 LowerRight;
        public Vector3 Normal;
        public Vector3 Up;
        public Vector3 Left;

        public VertexPositionNormalTexture[] Vertices;
        //        public int[] Indexes;
        //public short[] Indexes;


        public Distortion(Vector3 origin, Vector3 normal, Vector3 up,
            float width, float height)
        {
            //Vertices = new VertexPositionNormalTexture[4];
            Vertices = new VertexPositionNormalTexture[401];
            //Indexes = new short[100];
            Origin = origin;
            Normal = normal;
            Up = up;

            // Calculate the quad corners
            Left = Vector3.Cross(normal, Up);
            Vector3 uppercenter = (Up * height / 2) + origin;
            UpperLeft = uppercenter + (Left * width / 2);
            UpperRight = uppercenter - (Left * width / 2);
            LowerLeft = UpperLeft - (Up * height);
            LowerRight = UpperRight - (Up * height);

            FillVertices();
        }

        private void FillVertices()
        {
            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Normal;
            }

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i].Position = new Vector3((float)Math.Cos(angle) / 1.2f, (float)Math.Sin(angle) / 1.2f, 0);
                Vertices[i].TextureCoordinate = new Vector2((float)i / 400, -0.05f);


                Vertices[i + 1].Position = new Vector3(((float)Math.Cos(angle) * .25f) / 1.2f, ((float)Math.Sin(angle) / 1.2f) * .25f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 1].TextureCoordinate = new Vector2(((float)i / 400), 0.95f);

            }
            Vertices[400] = Vertices[0];


            /*for (int i = 0; i < 199; i += 2)
            {
                float test = (float)i / 200;
                float angle = (float)(test * Math.PI);
                Vertices[i].Position = new Vector3((float)Math.Cos(angle) / 1.2f, (float)Math.Sin(angle) / 1.2f, 0);
                Vertices[i].TextureCoordinate = new Vector2((float)i / 200, 0);

                //figure out proper math for uv coordinate
                Vertices[i + 1].Position = new Vector3(((float)Math.Cos(angle) * .5f) / 1.2f, ((float)Math.Sin(angle) * .5f) / 1.2f, 0);
                Vertices[i + 1].TextureCoordinate = new Vector2(((float)i / 200), .75f);

            }
            Vertices[200] = Vertices[0];
            for (int j = 0; j < 199; j+= 2)
            {
                float test = (float)j / 200;
                float angle = (float)(test * Math.PI);

                Vertices[j + 200].Position = new Vector3(((float)Math.Cos(angle) * .5f) / 1.2f, ((float)Math.Sin(angle) * .5f) / 1.2f, 0);

                //figure out proper math for uv coordinate
                Vertices[j + 200].TextureCoordinate = new Vector2(((float)j / 200), .75f);

                Vertices[j + 201].Position = new Vector3(((float)Math.Cos(angle) * .25f) / 1.2f, ((float)Math.Sin(angle) / 1.2f) * .25f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[j + 201].TextureCoordinate = new Vector2(((float)j / 200), 1);
            }
            Vertices[400] = Vertices[200];*/



        }
    }
}
