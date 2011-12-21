#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion



#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect.Nui;
using SnowGlobe;
using Bespoke.Common.Osc;

namespace SnowGlobe
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Runtime nui;

        FileStream stream;

        private Model car, lander, ship, flowers, eiffel, currModel;

        private Model anatomy;

        private Vector3 Position = Vector3.One;

        private Vector3 currHeadPos = new Vector3(0.0f, 0.0f, 100.0f);
        Vector3 headPos;

        private float userProximity = 400.0f;

        //vertical placement of view on cylinder
        private float distortionShift = 0.168f;

        private KeyboardState prevState;
        //Sphere position relative to kinect
        //Vector3 spherePos = new Vector3(0.0f, 1346.2f, 0.0f);
        Vector3 cameraTranslation = new Vector3(0.0f, 0.0f, 0.0f);
        private float cameraYRotation = 0.0f;

        //Control camera distance from anatomy
        private float Zoom = 250.0f;

        private float RotationYWorldTest = 0.0f;
        private float RotationYWorld = MathHelper.ToDegrees(-10.0f);
        private float RotationXWorld = 0.0f;

        //default
        private float RotationYModel = 0.0f;
        private float RotationXModel = 0.0f;

        private int handCount = 0;
        private bool rightTracking = false;
        private bool leftTracking = false;

        private bool enableRotation = false;

        //private Vector3 handPos = new Vector3(0.0f, 0.0f, 0.0f);

        private Vector3 lastPos = new Vector3(0.0f, 0.0f, 0.0f);

        private Vector3 handPosRight = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 handPosLeft = new Vector3(0.0f, 0.0f, 0.0f);

        private Vector3[] handPos = new Vector3[3];

        private float currDistance = 0.0f;
        private float prevDistance = 0.0f;

        //more hand stuff
        float lastTime = 0.0f;
        float startTime = 0.0f;
        Vector3 startPos;

        private Vector3 userPos = new Vector3(0.0f, 0.0f, 0.0f);

        private Vector3 userPosKinect1 = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 userPosKinect2 = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 userPosKinect3 = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 userPosKinect4 = new Vector3(0.0f, 0.0f, 0.0f);


        private float modelX = 0.0f;
        private float modelY = 0.0f;
        private float modelZ = 0.0f;

        //lander
        private float modelScaleLander = 1.0f;
        //ship
        private float modelScaleShip = 0.25f;
        //flowers
        private float modelScaleFlowers = 1.0f;

        private float modelScale;

        private int modelID;

        private float RotationX = 0.0f;

        //private float multiplier = 0.8f;

        private float curAngleX = 45.0f;
        private float curAngleY = 0.0f;

        private float origAngleX = 0.0f;
        private float origAngleY = 0.0f;

        private Matrix gameWorldRotation;
        private Matrix gameWorldRotation2;


        private int rotation;

        private int numUsers = 0;
        private int userID = -1;
        //private uint userID; 

        private string calibPose;

        //values for tracking hand positions
        private float prevHandYAngle;
        private float currHandYAngle;

        private float prevHandXAngle;
        private float currHandXAngle;

        //values for tracking user position
        private float prevUserAngleY;
        private float currUserAngleY;

        private float prevUserAngleX;
        private float currUserAngleX;

        RenderTarget2D viewport;
        //RenderTargetCube renderTarget;
        Texture2D testingTexture;

        Vector2 pos = new Vector2(0, 0);
        GraphicsDevice device;

        private Rectangle TitleSafe;
        SpriteBatch spriteBatch;

        GraphicsDeviceManager graphics;

        Distortion distortion;
        VertexDeclaration vertexDeclaration;
        Matrix View, Projection;
        BasicEffect distortionEffect;

        //Network variables
        private IPAddress kinectAddress1 = IPAddress.Parse("169.254.61.3");
        private IPAddress kinectAddress2 = IPAddress.Parse("192.168.1.122");

        private int kinectPort1 = 7000;
        private int kinectPort2 = 7005;

        private OscServer kinectServer1;
        private OscServer kinectServer2;

        bool trackingMain = false;
        bool trackingRemote1 = false;
        bool trackingRemote2 = false;

        float angleYLocal;
        float angleXLocal;

        float angleYRemote1;
        float angleXRemote1;

        float angleYRemote2;
        float angleXRemote2;

        //angles for shifting the rotations from each camera
        float shiftAngleLocal = 240.0f;
        float shiftAngleRemote1;
        float shiftAngleRemote2;

        private float cutPlaneDistance = 130.0f;


        //OSC Messages
        //OSC Methods

        float convert(float input)
        {
            byte[] buffer = System.BitConverter.GetBytes(input);

            byte[] temp = new byte[4];

            temp[0] = buffer[3];
            temp[1] = buffer[2];
            temp[2] = buffer[1];
            temp[3] = buffer[0];

            return System.BitConverter.ToSingle(temp, 0);
        }

        void kinect1Updated(object sender, OscBundleReceivedEventArgs e)
        {
            userPosKinect1.X = convert((float)e.Bundle.Messages[1].Data[0]);
            userPosKinect1.Y = convert((float)e.Bundle.Messages[1].Data[1]);
            userPosKinect1.Z = convert((float)e.Bundle.Messages[1].Data[2]);

            //Console.WriteLine("Kinect 1 Updated X: " + e.Bundle.Messages[1].Data.Length);

            if (userPosKinect1.X != 0.0f && userPosKinect1.Y != 0.0f && userPosKinect1.Z != 2700.0f)
            {
                trackingRemote1 = true;
                angleYRemote1 = getAngleTrig(userPosKinect1.Z, userPosKinect1.X) + MathHelper.ToRadians(shiftAngleRemote1);

                //Console.WriteLine("Remote 1 Kinect Angle: " + +MathHelper.ToDegrees(RotationYWorld));
            }
            else
            {
                trackingRemote1 = false;
            }
        }

        //OSC Methods
        void kinect2Updated(object sender, OscBundleReceivedEventArgs e)
        {

        }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            device = this.GraphicsDevice;

            //for 3d on projector need to be set at fullscreen 720p
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            

           // graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //test only
            
            // TODO: Add your initialization logic here
            //initialize Kinect SDK
            /*nui = new Runtime();

            nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

            nui.SkeletonEngine.TransformSmooth = true;

            TransformSmoothParameters param = new TransformSmoothParameters();

            param.Smoothing = 0.2f;
            param.Correction = 0.0f;
            param.Prediction = 0.0f;
            param.JitterRadius = 0.2f;
            param.MaxDeviationRadius = 0.3f;

            nui.SkeletonEngine.SmoothParameters = param;

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);*/

            handPos[0] = new Vector3(0.0f, 0.0f, 0.0f);
            handPos[1] = new Vector3(0.0f, 0.0f, 0.0f);

            //set up angles
            shiftAngleRemote1 = shiftAngleLocal + 115.3733f;
            shiftAngleRemote2 = shiftAngleLocal - 93.9733f;

            //initialize hand angles
            prevHandYAngle = 0.0f;
            prevHandXAngle = 0.0f;

            //for sphere
            //create new distortion object
            distortion = new Distortion(Vector3.Backward, 1, 1, distortionShift, 3, 1.33f);

            //for cylinder
            //distortion = new Distortion(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1, distortionShift, 4);

            View = Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero,
                Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 16.0f / 9.0f, 1, 500);

            //setupOSC();

            prevState = Keyboard.GetState();
            base.Initialize();
        }

        void setupOSC()
        {
            //OSC
            kinectServer1 = new OscServer(TransportType.Udp, IPAddress.Any, kinectPort1, kinectAddress1, Bespoke.Common.Net.TransmissionType.Unicast);
            kinectServer2 = new OscServer(TransportType.Udp, IPAddress.Any, kinectPort2, kinectAddress2, Bespoke.Common.Net.TransmissionType.Unicast);
            kinectServer1.FilterRegisteredMethods = true;
            kinectServer1.BundleReceived += new OscBundleReceivedHandler(kinect1Updated);
            kinectServer2.BundleReceived += new OscBundleReceivedHandler(kinect2Updated);

            //start listening
            kinectServer1.Start();
            kinectServer2.Start();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //load ship
            lander = Content.Load<Model>("Models/Lander/Lander");
            car = Content.Load<Model>("Models/kit_car");
            flowers = Content.Load<Model>("Models/Flower/Flower");
            ship = Content.Load<Model>("Models/Ship/Ship");

            anatomy = Content.Load<Model>("Anatomy/Full_NoReproductive");

            currModel = anatomy;
            modelScale = modelScaleFlowers;
            enableRotation = true;
            modelID = 1;

            Console.WriteLine("Num Meshes: " + anatomy.Meshes.Count);

            int i = 0;
            foreach (ModelMesh m in anatomy.Meshes)
            {
                Console.WriteLine(i + ": " + m.Name);
                i++;
            }

            testingTexture = Content.Load<Texture2D>("Grid");
            //gameShip2 = Content.Load<Model>("Ship");
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            //make sure the render target has a depth buffer
            viewport = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

            distortionEffect = new BasicEffect(graphics.GraphicsDevice);

            distortionEffect.World = Matrix.Identity;
            distortionEffect.View = View;
            distortionEffect.Projection = Projection;
            distortionEffect.TextureEnabled = true;

            vertexDeclaration = new VertexDeclaration(new VertexElement[]
                {
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    //new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Color, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                }
            );
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    Vector headPosTemp = data.Joints[JointID.Head].Position;

                    //Console.WriteLine(skeletonFrame.FloorClipPlane.X + " " + skeletonFrame.FloorClipPlane.Y + " " + skeletonFrame.FloorClipPlane.Z + " " + skeletonFrame.FloorClipPlane.W);
                    //float floorY = (((skeletonFrame.FloorClipPlane.X * headPos.X) + (skeletonFrame.FloorClipPlane.Z * headPos.Z) + skeletonFrame.FloorClipPlane.W) / (-skeletonFrame.FloorClipPlane.Y));

                    /*float a = Math.Abs(headPos.Y * (float)Math.Tan(MathHelper.ToRadians(90 - 25.0f)));

                    float b = Math.Abs(headPos.Z - a);

                    float c = Math.Abs(b * (float)Math.Sin(MathHelper.ToRadians(90 - 25.0f)));

                    float d = Math.Abs(headPos.Y / (float)Math.Cos(MathHelper.ToRadians(90 - 25.0f)));*/

                    //convert to cm and translate to ensure correct coordinate system
                    currHeadPos.X = headPosTemp.X * 100.0f;
                    currHeadPos.Y = headPosTemp.Y * 100.0f;
                    currHeadPos.Z = headPosTemp.Z * 100.0f;


                    //set proximity distance, needs to be changed to proper proxemics
                    userProximity = currHeadPos.Z * 2.0f;

                    Vector handRight = data.Joints[JointID.HandRight].Position;
                    Vector elbowRight = data.Joints[JointID.ElbowRight].Position;
                    Vector shoulderRight = data.Joints[JointID.ShoulderRight].Position;
                    Vector hipRight = data.Joints[JointID.HipRight].Position;

                    //cosine law
                    float angleY = (float)Math.Acos((Math.Pow(Vector2.Distance(new Vector2(handRight.X, handRight.Z), new Vector2(elbowRight.X, elbowRight.Z)), 2)
                        + Math.Pow(Vector2.Distance(new Vector2(elbowRight.X, elbowRight.Z), new Vector2(shoulderRight.X, shoulderRight.Z)), 2)
                        - Math.Pow(Vector2.Distance(new Vector2(handRight.X, handRight.Z), new Vector2(shoulderRight.X, shoulderRight.Z)), 2))
                        / (2 * Vector2.Distance(new Vector2(handRight.X, handRight.Z), new Vector2(elbowRight.X, elbowRight.Z))
                        * Vector2.Distance(new Vector2(elbowRight.X, elbowRight.Z), new Vector2(shoulderRight.X, shoulderRight.Z))));

                    //cosine law
                    float angleX = (float)Math.Acos((Math.Pow(Vector2.Distance(new Vector2(handRight.Y, handRight.Z), new Vector2(shoulderRight.Y, shoulderRight.Z)), 2)
                        + Math.Pow(Vector2.Distance(new Vector2(shoulderRight.Y, shoulderRight.Z), new Vector2(hipRight.Y, hipRight.Z)), 2)
                        - Math.Pow(Vector2.Distance(new Vector2(handRight.Y, handRight.Z), new Vector2(hipRight.Y, hipRight.Z)), 2))
                        / (2 * Vector2.Distance(new Vector2(handRight.Y, handRight.Z), new Vector2(shoulderRight.Y, shoulderRight.Z))
                        * Vector2.Distance(new Vector2(shoulderRight.Y, shoulderRight.Z), new Vector2(hipRight.Y, hipRight.Z))));
                    Console.WriteLine(MathHelper.ToDegrees(angleX));

                    currHandYAngle = getAngleTrig(handRight.X, handRight.Z);
                    currHandXAngle = getAngleTrig(handRight.Y, handRight.Z);

                    //arbitrary thresholds
                    if (MathHelper.ToDegrees(angleY) > 140.0f)
                    {
                        if (MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f)
                        {
                            RotationYModel += (prevHandYAngle - currHandYAngle) * 5;
                            RotationXModel += (prevHandXAngle - currHandXAngle) * 5;
                        }
                    }

                    prevHandYAngle = currHandYAngle;
                    prevHandXAngle = currHandXAngle;
                    //currHeadPos = Vector3.Transform(currHeadPos, Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation2)) * Matrix.CreateTranslation(cameraTranslation2));

                    //Console.WriteLine("Head Pos: " + currHeadPos.X + " " + currHeadPos.Y + " " + currHeadPos.Z);

                    //if (currHeadPos.Z < 330)
                    //{
                        headPos = Vector3.Transform(currHeadPos, Matrix.CreateTranslation(cameraTranslation) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation)));
                    //}
                }
            }
        }

        private void SwapModel()
        {
            resetRotations();
        }

        private void resetRotations()
        {
            RotationYModel = 0.0f;
            RotationXModel = 0.0f;
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private float getAngleTrig(float x, float y)
        {
            if (x == 0.0f)
            {
                if (y == 0.0f)
                    return (float)((3 * Math.PI) / 2.0f);
                else
                    return (float)(Math.PI / 2.0f);
            }
            else if (y == 0.0f)
            {
                if (x < 0)
                    return (float)Math.PI;
                else
                    return 0;
            }
            if (y > 0.0f)
            {
                if (x > 0.0f)
                    return (float)Math.Atan(y / x);
                else
                    return (float)(Math.PI - Math.Atan(y / -x));
            }
            else
            {
                if (x > 0.0f)
                    return (float)(2.0f * Math.PI - Math.Atan(-y / x));
                else
                {
                    return (float)(Math.PI + (float)Math.Atan(-y / -x));
                }
            }


        }

        /// <summary>
        /// Updates the position of the 3D model based on inputs received from a
        /// connected game pad.
        /// </summary>
        private void UpdateRotations()
        {

            /*RotationYWorld = getAngleTrig(headPos.Z, headPos.X);
            RotationXWorld = -getAngleTrig(headPos.Z, headPos.Y);*/

            gameWorldRotation =
                    Matrix.CreateRotationX(0) *
                    Matrix.CreateRotationY(0);

            gameWorldRotation2 =
               Matrix.CreateRotationZ(RotationYWorld);



        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            // Allows the game to exit
            if (state.IsKeyDown(Keys.Escape))
            {
                stream.Close();
                this.Exit();
            }
            /*if (state.IsKeyDown(Keys.A))
            {
                resetRotations();
            }
            if (state.IsKeyDown(Keys.K))
            {
                shiftAngleLocal -= 1.0f;
                Console.WriteLine("Shift: " + shiftAngleLocal);
            }
            if (state.IsKeyDown(Keys.L))
            {
                shiftAngleLocal += 1.0f;
                Console.WriteLine("Shift: " + shiftAngleLocal);
            }*/

            //View controls
            if (state.IsKeyDown(Keys.Left))
            {
                RotationYWorld += MathHelper.ToRadians(1.0f);
            }
            if (state.IsKeyDown(Keys.Right))
            {
                RotationYWorld -= MathHelper.ToRadians(1.0f);
            }
            if (state.IsKeyDown(Keys.Up))
            {
                RotationXWorld += MathHelper.ToRadians(1.0f);
            }
            if (state.IsKeyDown(Keys.Down))
            {
                RotationXWorld -= MathHelper.ToRadians(1.0f);
            }

            //slice controls
            if (state.IsKeyDown(Keys.A) && cutPlaneDistance > 1.0f)
            {
                cutPlaneDistance -= 0.1f;
                //Zoom += 0.5f;
                Console.WriteLine(cutPlaneDistance);
            }
            if (state.IsKeyDown(Keys.S))
            {
                cutPlaneDistance += 0.1f;
                //Zoom -= 0.5f;
                Console.WriteLine(cutPlaneDistance);
            }

            if (state.IsKeyDown(Keys.Z))
            {
                userProximity -= 0.5f;
                //Zoom -= 0.5f;
                Console.WriteLine(userProximity);
            }
            if (state.IsKeyDown(Keys.X))
            {
                userProximity += 0.5f;
                //Zoom += 0.5f;
                Console.WriteLine(userProximity);
            }

            if (state.IsKeyDown(Keys.N))
            {
                Zoom -= 0.5f;
            }
            if (state.IsKeyDown(Keys.M))
            {
                Zoom += 0.5f;
            }

            prevState = state;

            UpdateRotations();

            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the 3D model
        /// </summary>
        private void DrawModel(Model m, float scale)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //float aspectRatio = 0.9f;
            m.CopyAbsoluteBoneTransformsTo(transforms);

            /*Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 1.0f, 10000.0f);*/

            //for slicing
            Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, cutPlaneDistance, 10000.0f);

            //create camera position

            /*float camX = (float)(Zoom * Math.Sin(RotationYWorld) * Math.Cos(2.5*(RotationXWorld + MathHelper.ToRadians(10.0f))));
            float camZ = (float)(Zoom * Math.Cos(RotationYWorld));
            float camY = (float)(Zoom * Math.Sin(RotationYWorld) * Math.Sin(2.5*(RotationXWorld + MathHelper.ToRadians(10.0f))));*/


            //just changed, was backwards
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            camPos = Vector3.Transform(camPos, pitch);
            camPos = Vector3.Transform(camPos, yaw);

            //Vector3 camPos = new Vector3(camX, camYAngleX, camZAngleX);
            Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            //Draw each layer bade on mesh indices
            //Opacity is determined by user position
            //the order of drawing is important (DON'T CHANGE!)

            //nervous
            drawLayer(m, 180, 184, (userProximity - 0.0f) / 50.0f, scale, view, projection, transforms);
            //circulatory
            drawLayer(m, 177, 179, (userProximity - 50.0f) / 50.0f, scale, view, projection, transforms);
            //urinary
            drawLayer(m, 176, 176, (userProximity - 100.0f) / 50.0f, scale, view, projection, transforms);
            //digestive
            drawLayer(m, 4, 4, (userProximity - 150.0f) / 50.0f, scale, view, projection, transforms);
            //respiratory
            drawLayer(m, 172, 175, (userProximity - 200.0f) / 50.0f, scale, view, projection, transforms);
            //skeletal
            drawLayer(m, 146, 171, (userProximity - 250.0f) / 50.0f, scale, view, projection, transforms);
            //muscle
            drawLayer(m, 5, 145, (userProximity - 300.0f) / 50.0f, scale, view, projection, transforms);
            //skin
            drawLayer(m, 0, 3, (userProximity - 350.0f) / 50.0f, scale, view, projection, transforms);

            //drawLayer(m, 177, 179, 1.0f, scale, view, projection, transforms);

        }

        private void drawLayer(Model modelIn, int startIndex, int endIndex, float alphaIn, float scale, Matrix view, Matrix projection, Matrix[] transforms)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                foreach (BasicEffect effect in modelIn.Meshes[i].Effects)
                {
                    if (alphaIn < 1.0f)
                        effect.Alpha = alphaIn;
                    else
                        effect.Alpha = 1.0f;

                    effect.EnableDefaultLighting();
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = gameWorldRotation *
                        transforms[modelIn.Meshes[i].ParentBone.Index] *
                        Matrix.CreateScale(scale);
                }
                modelIn.Meshes[i].Draw();
            }
        }

        private void DrawForCylinder()
        {
            //draw distortion
            this.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(Color.Black);
            //this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            distortionEffect.Texture = viewport;

            foreach (EffectPass pass in distortionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                distortionEffect.World = gameWorldRotation;
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, distortion.Vertices, 0, 3999);
            }
        }


        private void DrawCalibrationPattern()
        {
            this.GraphicsDevice.SetRenderTarget(null);
            this.GraphicsDevice.Clear(Color.Black);
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            distortionEffect.Texture = testingTexture;

            Matrix gameWorldRotation2 = Matrix.CreateRotationZ(MathHelper.ToRadians(0));
            foreach (EffectPass pass in distortionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                distortionEffect.World = gameWorldRotation2;
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, distortion.Vertices, 0, 3999);
            }

            gameWorldRotation2 = Matrix.CreateRotationZ(MathHelper.ToRadians(120));
            foreach (EffectPass pass in distortionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                distortionEffect.World = gameWorldRotation2;
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, distortion.Vertices, 0, 3999);
            }

            gameWorldRotation2 = Matrix.CreateRotationZ(MathHelper.ToRadians(240));
            foreach (EffectPass pass in distortionEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                distortionEffect.World = gameWorldRotation2;
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, distortion.Vertices, 0, 3999);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            //set to null to draw to screen
            this.GraphicsDevice.SetRenderTarget(viewport);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.GraphicsDevice.Clear(Color.White);

            this.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //for lander, draw(x,y,z,scale)
            DrawModel(currModel, modelScale);

            this.GraphicsDevice.SetRenderTarget(null);

            viewport.SaveAsJpeg(stream, 1280, 720);
            DrawForCylinder();

            //for testing projector alignment
            //DrawCalibrationPattern();

            base.Draw(gameTime);
        }
    }
}
