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
        public float Shift;
        public float Exponent;
        public float Radius;

        public Vector3 Normal;

        public VertexPositionNormalTexture[] Vertices;


        public Distortion(Vector3 normal,
            float width, float height, float shift, int exponent, float radius)
        {
            Vertices = new VertexPositionNormalTexture[16001];
            Shift = shift;
            Exponent = exponent;
            Radius = radius;
            Normal = normal;

            FillVertices();
        }

        private void FillVertices()
        {
            // Provide a normal for each vertex
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = Normal;
            }

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i].Position = new Vector3((float)Math.Cos(angle) / Radius, (float)Math.Sin(angle) / Radius, 0);
                Vertices[i].TextureCoordinate = new Vector2((float)i / 400, 0.0f + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 1].Position = new Vector3(((float)Math.Cos(angle) * .9f) / Radius, ((float)Math.Sin(angle) / Radius) * .9f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 1].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.1, Exponent) + Shift);

            }
            Vertices[400] = Vertices[0];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 400].Position = new Vector3(((float)Math.Cos(angle) * 0.9f) / Radius, ((float)Math.Sin(angle) * 0.9f) / Radius, 0);
                Vertices[i + 400].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.1, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 401].Position = new Vector3(((float)Math.Cos(angle) * 0.8f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.8f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 401].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.2, Exponent) + Shift);

            }

            Vertices[800] = Vertices[400];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 800].Position = new Vector3(((float)Math.Cos(angle) * 0.8f) / Radius, ((float)Math.Sin(angle) * 0.8f) / Radius, 0);
                Vertices[i + 800].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.2, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 801].Position = new Vector3(((float)Math.Cos(angle) * 0.7f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.7f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 801].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.3, Exponent) + Shift);

            }

            Vertices[1200] = Vertices[800];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 1200].Position = new Vector3(((float)Math.Cos(angle) * 0.7f) / Radius, ((float)Math.Sin(angle) * 0.7f) / Radius, 0);
                Vertices[i + 1200].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.3, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 1201].Position = new Vector3(((float)Math.Cos(angle) * 0.6f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.6f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 1201].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.4, Exponent) + Shift);

            }

            Vertices[1600] = Vertices[1200];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 1600].Position = new Vector3(((float)Math.Cos(angle) * 0.6f) / Radius, ((float)Math.Sin(angle) * 0.6f) / Radius, 0);
                Vertices[i + 1600].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.4, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 1601].Position = new Vector3(((float)Math.Cos(angle) * 0.5f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.5f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 1601].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.5, Exponent) + Shift);

            }

            Vertices[2000] = Vertices[1600];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 2000].Position = new Vector3(((float)Math.Cos(angle) * 0.5f) / Radius, ((float)Math.Sin(angle) * 0.5f) / Radius, 0);
                Vertices[i + 2000].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.5, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 2001].Position = new Vector3(((float)Math.Cos(angle) * 0.4f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.4f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 2001].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.6, Exponent) + Shift);

            }

            Vertices[2400] = Vertices[2000];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 2400].Position = new Vector3(((float)Math.Cos(angle) * 0.4f) / Radius, ((float)Math.Sin(angle) * 0.4f) / Radius, 0);
                Vertices[i + 2400].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.6, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 2401].Position = new Vector3(((float)Math.Cos(angle) * 0.3f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.3f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 2401].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.7, Exponent) + Shift);

            }

            Vertices[2800] = Vertices[2400];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 2800].Position = new Vector3(((float)Math.Cos(angle) * 0.3f) / Radius, ((float)Math.Sin(angle) * 0.3f) / Radius, 0);
                Vertices[i + 2800].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.7, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 2801].Position = new Vector3(((float)Math.Cos(angle) * 0.2f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.2f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 2801].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.8, Exponent) + Shift);

            }

            Vertices[3200] = Vertices[2800];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 3200].Position = new Vector3(((float)Math.Cos(angle) * 0.2f) / Radius, ((float)Math.Sin(angle) * 0.2f) / Radius, 0);
                Vertices[i + 3200].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.8, Exponent) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 3201].Position = new Vector3(((float)Math.Cos(angle) * 0.1f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.1f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 3201].TextureCoordinate = new Vector2(((float)i / 400), (float)Math.Pow(0.9, Exponent) + Shift);

            }

            Vertices[3600] = Vertices[3200];

            for (int i = 0; i < 399; i += 2)
            {
                float test = (float)i / 400;
                float angle = (float)(test * Math.PI);
                Vertices[i + 3600].Position = new Vector3(((float)Math.Cos(angle) * 0.1f) / Radius, ((float)Math.Sin(angle) * 0.1f) / Radius, 0);
                Vertices[i + 3600].TextureCoordinate = new Vector2((float)i / 400, (float)Math.Pow(0.9, Exponent - 1) + Shift);

                //Vertices[i].Position = new Vector3(0.0f, 0.0f, 0.0f);
                Vertices[i + 3601].Position = new Vector3(((float)Math.Cos(angle) * 0.01f) / Radius, ((float)Math.Sin(angle) / Radius) * 0.01f, 0);
                //Vertices[i + 1].TextureCoordinate = new Vector2((float)Math.Cos(angle) * .25f, ((float)Math.Sin(angle) / 1.333f) * .25f);
                Vertices[i + 3601].TextureCoordinate = new Vector2(((float)i / 400), 1.00f + Shift);

            }

            Vertices[4000] = Vertices[3600];

        }
    }
}
