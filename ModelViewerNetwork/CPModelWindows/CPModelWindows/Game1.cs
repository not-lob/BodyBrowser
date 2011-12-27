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
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using SnowGlobe;
using Bespoke.Common.Osc;
using System.Threading;

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

        private SpriteFont labelFont;

        private Vector3 Position = Vector3.One;

        private Vector3 currHeadPos = new Vector3(0.0f, 0.0f, 100.0f);
        Vector3 headPos;

        private float userProximity = 500.0f;

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
        private float RotationYWorld = 0.0f;//MathHelper.ToDegrees(-10.0f);
        private float RotationXWorld = 0.0f;

        //Pick object
        private bool bInPickMode = false;
        private int iPickIndex = 0;
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

        private float prevLeftHandYAngle;
        private float currLeftHandYAngle;

        private float prevLeftHandXAngle;
        private float currLeftHandXAngle;

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

        //Speech recognition
        private const string RecognizerId = "SR_MS_en-US_Kinect_10.0"; 

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
            nui = new Runtime();

            nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

            nui.SkeletonEngine.TransformSmooth = true;

            TransformSmoothParameters param = new TransformSmoothParameters();

            param.Smoothing = 0.2f;
            param.Correction = 0.0f;
            param.Prediction = 0.0f;
            param.JitterRadius = 0.2f;
            param.MaxDeviationRadius = 0.3f;

            nui.SkeletonEngine.SmoothParameters = param;

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);

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

            //Start a new thread for speech recognition
            Thread threadSpeechRecog = new Thread(SpeechRecognization);
            threadSpeechRecog.Start();
            
            // The mouse is not visible by default.
            IsMouseVisible = true;

            base.Initialize();
        }

        public void SpeechRecognization()
        {
            using (var source = new KinectAudioSource())
            {
                source.FeatureMode = true;
                source.AutomaticGainControl = false;
                source.SystemMode = SystemMode.OptibeamArrayOnly;

                RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();
                if (ri == null)
                {
                    Console.WriteLine("Could not open speech recognizer");
                    return;
                }

                using (var src = new SpeechRecognitionEngine(ri.Id))
                {
                    var commands = new Choices();
                    commands.Add("show me the skeleton");
                    commands.Add("show me the circulatory");
                    commands.Add("show me the body");

                    commands.Add("show me the brain");
                    commands.Add("show me the heart");

                    commands.Add("reset");


                    var gb = new GrammarBuilder();
                    //Specify the culuture to match the recogizer
                    gb.Culture = ri.Culture;
                    gb.Append(commands);

                    //Create the actual Grammer instance
                    var g = new Grammar(gb);

                    src.LoadGrammar(g);
                    src.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SrcSpeechRecognized);
                    //src.SpeechRecognitionRejected += SrcSpeechRecognizedRejected;

                    using (Stream s = source.Start())
                    {
                        src.SetInputToAudioStream(s,
                            new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1,
                                32000, 2, null));
                        src.RecognizeAsync(RecognizeMode.Multiple);
                        Console.ReadLine();

                        src.RecognizeAsyncStop();
                        Environment.Exit(0);


                    }
                }

            }
        }

        void SrcSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.9)
            {
                if (e.Result.Text == "show me the skeleton")
                {
                    //
                }
                else if (e.Result.Text == "show me the circulatory")
                {
                    //
                }
                else if (e.Result.Text == "show me the body")
                {
                    //
                }

                else if (e.Result.Text == "show me the brain")
                {
                    //

                }
                else if (e.Result.Text == "show me the heart")
                {
                    //
                }

                else if (e.Result.Text == "reset")
                {
                    resetRotations();
                }
            }
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

            //Font
            labelFont = Content.Load<SpriteFont>("Fonts/LabelFont");

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

            //testingTexture = Content.Load<Texture2D>("Grid");
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


                    //Left hand - zoom
                    Vector handLeft = data.Joints[JointID.HandLeft].Position;
                    Vector elbowLeft = data.Joints[JointID.ElbowLeft].Position;
                    Vector shoulderLeft = data.Joints[JointID.ShoulderLeft].Position;
                    Vector hipLeft = data.Joints[JointID.HipLeft].Position;

                    //cosine law
                    float angleLeftY = (float)Math.Acos((Math.Pow(Vector2.Distance(new Vector2(handLeft.X, handLeft.Z), new Vector2(elbowLeft.X, elbowLeft.Z)), 2)
                        + Math.Pow(Vector2.Distance(new Vector2(elbowLeft.X, elbowLeft.Z), new Vector2(shoulderLeft.X, shoulderLeft.Z)), 2)
                        - Math.Pow(Vector2.Distance(new Vector2(handLeft.X, handLeft.Z), new Vector2(shoulderLeft.X, shoulderLeft.Z)), 2))
                        / (2 * Vector2.Distance(new Vector2(handLeft.X, handLeft.Z), new Vector2(elbowLeft.X, elbowLeft.Z))
                        * Vector2.Distance(new Vector2(elbowLeft.X, elbowLeft.Z), new Vector2(shoulderLeft.X, shoulderLeft.Z))));

                    //cosine law
                    float angleLeftX = (float)Math.Acos((Math.Pow(Vector2.Distance(new Vector2(handLeft.Y, handLeft.Z), new Vector2(shoulderLeft.Y, shoulderLeft.Z)), 2)
                        + Math.Pow(Vector2.Distance(new Vector2(shoulderLeft.Y, shoulderLeft.Z), new Vector2(hipLeft.Y, hipLeft.Z)), 2)
                        - Math.Pow(Vector2.Distance(new Vector2(handLeft.Y, handLeft.Z), new Vector2(hipLeft.Y, hipLeft.Z)), 2))
                        / (2 * Vector2.Distance(new Vector2(handLeft.Y, handLeft.Z), new Vector2(shoulderLeft.Y, shoulderLeft.Z))
                        * Vector2.Distance(new Vector2(shoulderLeft.Y, shoulderLeft.Z), new Vector2(hipLeft.Y, hipLeft.Z))));
                    //Console.WriteLine(MathHelper.ToDegrees(angleX));

                    currLeftHandXAngle = getAngleTrig(handLeft.X, handLeft.Z);
                    currLeftHandYAngle = getAngleTrig(handLeft.Y, handLeft.Z);

                    //arbitrary thresholds
                    if (MathHelper.ToDegrees(angleY) > 140.0f)
                    {
                        if (MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f)
                        {
                            RotationYModel -= (prevHandYAngle - currHandYAngle) * 5;
                            //Peng after talk we suppress x rotation 
                            //RotationXModel += (prevHandXAngle - currHandXAngle) * 5;
                        }
                    }
                    if (MathHelper.ToDegrees(angleLeftY) > 140.0f)
                    {
                        if (MathHelper.ToDegrees(angleLeftX) > 30.0f && MathHelper.ToDegrees(angleLeftX) < 150.0f)
                        {
                            Zoom -= (prevLeftHandYAngle - currLeftHandYAngle) * 300;
                        }
                    }

                    prevHandYAngle = currHandYAngle;
                    prevHandXAngle = currHandXAngle;

                    prevLeftHandXAngle = currLeftHandXAngle;
                    prevLeftHandYAngle = currLeftHandYAngle;
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

            //CheckMouseClick();
            base.Update(gameTime);
        }

        void CheckMouseClick()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Ray pickRay = GetPickRay();

                float selectedDistance = float.MaxValue;
                int i = 1;
                iPickIndex = 0;
                foreach (ModelMesh m in anatomy.Meshes)
                {
                    if (i < 146)
                    {
                        ++i;
                        continue;
                        
                    }
                    BoundingSphere sphere = m.BoundingSphere;
                    Nullable<float> result = pickRay.Intersects(sphere);
                    if (result.HasValue == true)
                    {
                        if (result.Value < selectedDistance)
                        {
                            bInPickMode = true;
                            iPickIndex = i;
                            break;
                        }

                    }
                    ++i;
                }
                if (iPickIndex == 0)
                    bInPickMode = false;

            }
        }

        Ray GetPickRay()
        {
            MouseState mouseState = Mouse.GetState();

            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            camPos = Vector3.Transform(camPos, pitch);
            camPos = Vector3.Transform(camPos, yaw);

            Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            //Projection Matrix
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 0.1f, 10000.0f);
            Matrix[] transforms = new Matrix[currModel.Bones.Count];
            Matrix world = gameWorldRotation *
                            transforms[currModel.Meshes[2].ParentBone.Index] *
                            Matrix.CreateScale(modelScale);//Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource,
                projection, view, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource,
                projection, view, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            return pickRay;
        }

        /// <summary>
        /// Renders the label around 3D model
        /// </summary>
        private void DrawLabel()
        {
            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            camPos = Vector3.Transform(camPos, pitch);
            camPos = Vector3.Transform(camPos, yaw);

            Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            //Projection Matrix
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 0.1f, 10000.0f);

            //Create effect for text sprite
            BasicEffect effect = new BasicEffect(GraphicsDevice);
            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.View = view;
            effect.Projection = projection;
            effect.World = Matrix.CreateScale(1, -1, 1);

            DrawSkinLayerLabel(effect);
            DrawMuscularLayerLabel(effect);
            DrawOrganicLayerLabel(effect);

            //Restore device status
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            ////just changed, was backwards
            //Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            //Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            //Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            //camPos = Vector3.Transform(camPos, pitch);
            //camPos = Vector3.Transform(camPos, yaw);

            ////Vector3 camPos = new Vector3(camX, camYAngleX, camZAngleX);
            //Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            //float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //Matrix projection =
            //    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
            //    aspectRatio, 0.1f, 10000.0f);


            //BasicEffect effect = new BasicEffect(GraphicsDevice);
            //effect.TextureEnabled = true;
            //effect.VertexColorEnabled = true;
            //effect.View = view;
            //effect.Projection = projection;
            //Vector3 textPosition = new Vector3(15, 68, 0);
            //effect.World = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(textPosition);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.Default, RasterizerState.CullCounterClockwise, effect);
            ////public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
            //spriteBatch.DrawString(labelFont, "Brain", Vector2.Zero, Color.Gold /* ((userProximity - 350.0f) / 50.0f)*/, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
            //spriteBatch.End();
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        }

        private void DrawSkinLayerLabel(BasicEffect effect)
        {
            Color skinLayerColor = Color.Red;

            //Label text
            String[] skinFrontLabelText = { "Head", "Neck", "Shoulder", "Elbow", "Hand", "Abdomen", "Knee", "Ankle" };
            String[] skinBackLabelText = { "Head", "Spine", "Hip" };

            //Label position
            Vector3[] skinFrontLabelPosition = { new Vector3(13, 68, 0),  //head
                                              new Vector3(-30, 50, 0),  //neck
                                              new Vector3(18, 47, 0),  //shoulder
                                              //new Vector3(18, 10, 0),  //spine
                                              new Vector3(-43, 20, 0),  //elbow
                                              new Vector3(92, 26, 0), //hand
                                              new Vector3(20, 0, 0), //abdomen
                                              //new Vector3(22, -16, 0), //hip
                                              new Vector3(-39, -63, 0), //knee
                                              new Vector3(23, -106, 0) //ankle
                                          };
            Vector3[] skinBackLabelPosition = { new Vector3(13, 68, 0),  //head
                                               new Vector3(-40, 10, 0),  //spine
                                               new Vector3(22, -16, 0)  //hip
                                              };

            float alphaIn = ((userProximity - 400.0f) / 50.0f);
            if (alphaIn > 1.0f)
                alphaIn = 1.0f;
            else if (alphaIn >= 0.0f && alphaIn <= 1.0f)
                alphaIn = alphaIn;
            else
                alphaIn = 0.0f;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 7; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelText[i], new Vector2(skinFrontLabelPosition[i].X, -skinFrontLabelPosition[i].Y), skinLayerColor * alphaIn, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 3; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinBackLabelText[i], new Vector2(skinBackLabelPosition[i].X, -skinBackLabelPosition[i].Y), skinLayerColor * alphaIn, 0, Vector2.Zero, 0.25f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
        }

        private void DrawMuscularLayerLabel(BasicEffect effect)
        {
            Color muscularLayerColor = Color.Blue;

            //Label text
            String[] muscularFrontLabelText = { "Frontalis", "Zygomaticus", "Trapezius", "Deltoid", "Brachioradialis", 
                                                  "Pectoralis major", "Rectus abdominus", "Rectus femoris", "Fibularis longus", "Extensor digitorum longus" };
            String[] muscularBackLabelText = { "Sternocleidomastoid", "Deltoid", "Latissimus dorsi", 
                                                 "Extensor digitorum", "Gluteus maximus", "Biceps femoris", "Gastrocnemius" };

            //Label position
            Vector3[] muscularFrontLabelPosition = { new Vector3(12, 65, 0),  //Frontalis
                                              new Vector3(10, 55, 0),  //Zygomaticus
                                              new Vector3(-45, 48, 0),  //Trapezius
                                              new Vector3(26, 43, 0),  //Deltoid
                                              new Vector3(55, 40, 0),  //Brachioradialis
                                              new Vector3(20, 21, 0), //Pectoralis major
                                              new Vector3(-83, 5, 0), //Rectus abdominus
                                              new Vector3(-75, -36, 0), //Rectus femoris
                                              new Vector3(21, -70, 0), //Fibularis longus
                                              new Vector3(-53, -107, 0) //Extensor digitorum longus
                                          };
            Vector3[] muscularBackLabelPosition = { new Vector3(-93, 48, 0),  //Sternocleidomastoid
                                               //new Vector3(25, 50, 0),  //Trapezius
                                               new Vector3(-85, 40, 0),  //Deltoid
                                               //new Vector3(70, 38, 0),  //Triceps brachii
                                               new Vector3(20, 13, 0),  //Latissimus dorsi
                                               new Vector3(-150, 34, 0),  //Extensor digitorum
                                               new Vector3(23, -15, 0), //Gluteus maximus
                                               new Vector3(-66, -38, 0),  //Biceps femoris
                                               new Vector3(23, -76, 0)  //Gastrocnemius
                                              };

            ////View Matrix
            //Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            //Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            //Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            //camPos = Vector3.Transform(camPos, pitch);
            //camPos = Vector3.Transform(camPos, yaw);

            //Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            ////Projection Matrix
            //float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //Matrix projection =
            //    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
            //    aspectRatio, 0.1f, 10000.0f);

            ////Create effect for text sprite
            //BasicEffect effect = new BasicEffect(GraphicsDevice);
            //effect.TextureEnabled = true;
            //effect.VertexColorEnabled = true;
            //effect.View = view;
            //effect.Projection = projection;

            float alphaIn = ((userProximity - 350.0f) / 50.0f);
            if (alphaIn > 1.0f && alphaIn <= 2.0f)
                alphaIn = 2.0f - alphaIn;
            else if (alphaIn >= 0.0f && alphaIn <= 1.0f)
                alphaIn = alphaIn;
            else
                alphaIn = 0.0f;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 10; ++i)
            {
                //effect.World = Matrix.Identity;
                //effect.World = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(muscularFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, muscularFrontLabelText[i], new Vector2(muscularFrontLabelPosition[i].X, -muscularFrontLabelPosition[i].Y), muscularLayerColor * alphaIn, 0, Vector2.Zero, 0.23f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 7; ++i)
            {
                //effect.World = Matrix.Identity;
                //effect.World = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(muscularBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, muscularBackLabelText[i], new Vector2(muscularBackLabelPosition[i].X, -muscularBackLabelPosition[i].Y), muscularLayerColor * alphaIn, 0, Vector2.Zero, 0.25f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }

        private void DrawOrganicLayerLabel(BasicEffect effect)
        {
            Color organicLayerColor = Color.Orange;

            //Label text
            String[] organicFrontLabelText = { "Larynx", "Lung", "Liver", "Stomach", "Ascending colon", "Descending colon", "Rectum & anus"};
            String[] organicBackLabelText = { "Kidney", "Bladder" };
            //Label position
            Vector3[] organicFrontLabelPosition = { new Vector3(13, 48, 0),  //Larynx
                                              new Vector3(17, 21, 0),  //Lung
                                              new Vector3(-38, 10, 0),  //Liver
                                              new Vector3(18, 8, 0),  //Stomach
                                              new Vector3(-78, 0, 0),  //Ascending colon
                                              new Vector3(17, -4, 0), //Descending colon
                                              new Vector3(22, -20, 0), //Rectum & anus
                                          };
            Vector3[] organicBackLabelPosition = { new Vector3(18, 3, 0),  //Kidney
                                               new Vector3(-42, -10, 0),  //Bladder
                                              };

            float alphaIn = ((userProximity - 200.0f) / 50.0f);
            if (alphaIn > 2.0f && alphaIn <= 3.0f)
                alphaIn = 3.0f - alphaIn;
            else if (alphaIn > 1.0f && alphaIn <= 2.0f)
                alphaIn = 1.0f;
            else if (alphaIn >= 0.0f && alphaIn <= 1.0f)
                alphaIn = alphaIn;
            else
                alphaIn = 0.0f;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 7; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, organicFrontLabelText[i], new Vector2(organicFrontLabelPosition[i].X, -organicFrontLabelPosition[i].Y), organicLayerColor * alphaIn, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 2; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, organicBackLabelText[i], new Vector2(organicBackLabelPosition[i].X, -organicBackLabelPosition[i].Y), organicLayerColor * alphaIn, 0, Vector2.Zero, 0.25f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
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

            if (bInPickMode)
            {
                for (int i = 146; i < m.Meshes.Count; i++)
                {
                    foreach (BasicEffect effect in m.Meshes[i].Effects)
                    {
                        if (i != iPickIndex)
                            effect.Alpha = 0.3f;
                        else
                            effect.Alpha = 1.0f;

                        effect.EnableDefaultLighting();
                        effect.View = view;
                        effect.Projection = projection;
                        effect.World = gameWorldRotation *
                            transforms[m.Meshes[i].ParentBone.Index] *
                            Matrix.CreateScale(scale);
                    }
                    m.Meshes[i].Draw();
                }
            }
            else
            {
                //Draw each layer bade on mesh indices
                //Opacity is determined by user position
                //the order of drawing is important (DON'T CHANGE!)

                //nervous
                drawLayer(m, 180, 184, (userProximity - 100.0f) / 50.0f, scale, view, projection, transforms);
                //circulatory
                drawLayer(m, 177, 179, (userProximity - 100.0f) / 50.0f, scale, view, projection, transforms);

                //urinary
                drawLayer(m, 176, 176, (userProximity - 200.0f) / 50.0f, scale, view, projection, transforms);
                //digestive
                drawLayer(m, 4, 4, (userProximity - 200.0f) / 50.0f, scale, view, projection, transforms);
                //respiratory
                drawLayer(m, 172, 175, (userProximity - 200.0f) / 50.0f, scale, view, projection, transforms);

                //skeletal
                drawLayer(m, 146, 171, (userProximity - 300.0f) / 50.0f, scale, view, projection, transforms);
                //muscle
                drawLayer(m, 5, 145, (userProximity - 350.0f) / 50.0f, scale, view, projection, transforms);
                //skin
                drawLayer(m, 0, 3, (userProximity - 400.0f) / 50.0f, scale, view, projection, transforms);

                //drawLayer(m, 177, 179, 1.0f, scale, view, projection, transforms);
            }
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
            this.GraphicsDevice.SetRenderTarget(null);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.GraphicsDevice.Clear(Color.White);

            this.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //for lander, draw(x,y,z,scale)
            DrawModel(currModel, modelScale);
            DrawLabel();

            //this.GraphicsDevice.SetRenderTarget(null);

            //viewport.SaveAsJpeg(stream, 1280, 720);
            //DrawForCylinder();

            //for testing projector alignment
            //DrawCalibrationPattern();

            base.Draw(gameTime);
        }
    }
}
