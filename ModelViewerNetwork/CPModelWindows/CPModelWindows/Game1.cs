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

namespace SnowGlobe
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    struct MeshDetails
    {
        public bool visible;
        public bool selected;
        public float opacity;

        public MeshDetails(bool visible, bool selected, float opacity)
        {
            this.visible = visible;
            this.selected = selected;
            this.opacity = opacity;
        }
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Vector3 testPosition = new Vector3(0.0f, 0.0f, 0.0f);
        private bool pointing = true;
        private bool slicing = false;

        Runtime nui;

        KinectAudioSource source;
        SpeechRecognitionEngine sre;

        private Model car, lander, ship, flowers, eiffel, currModel;

        private Model anatomy;

        private Vector3 Position = Vector3.One;

        private Vector3 currHeadPos = new Vector3(0.0f, 0.0f, 100.0f);
        Vector3 headPos;

        public int frameCounter = 0;

        private float userProximity = 400.0f;

        //alpha values for each anatomical layer
        float skinAlpha = 1.0f;
        float muscleAlpha = 1.0f;
        float skeletalAlpha = 1.0f;
        float respiratoryAlpha = 1.0f;
        float digestiveAlpha = 1.0f;
        float urinaryAlpha = 1.0f;
        float circulatoryAlpha = 1.0f;
        float nervousAlpha = 1.0f;

        //active layer
        float[] layerOpacities = new float[8];
        int activeLayerIndex;

        //bool values for gestures
        bool peelingForward = false;
        bool peelingBackward = false;

        //limit for a gesture to be performed
        int frameLimit = 300;

        //duration before gestures are detected
        int gestureGapDuration = 50;

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
        Texture2D testingTexture, sliceTexture;

        Vector2 pos = new Vector2(0, 0);
        GraphicsDevice device;

        private Rectangle TitleSafe;
        SpriteBatch spriteBatch;

        GraphicsDeviceManager graphics;

        Distortion distortion;
        VertexDeclaration vertexDeclaration, vertexSliceDeclaration;
        Matrix View, Projection, camView, camProjection, kinectView, kinectProjection;
        BasicEffect distortionEffect, sliceEffect;

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

        private Vector3 screenPos = new Vector3(0, 0, 0);

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

            graphics.IsFullScreen = true;
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
            this.IsMouseVisible = true;

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

            layerOpacities[0] = 1.0f;
            layerOpacities[1] = 1.0f;
            layerOpacities[2] = 1.0f;
            layerOpacities[3] = 1.0f;
            layerOpacities[4] = 1.0f;
            layerOpacities[5] = 1.0f;
            layerOpacities[6] = 1.0f;
            layerOpacities[7] = 1.0f;

            activeLayerIndex = 0;

            //for cylinder
            //distortion = new Distortion(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1, distortionShift, 4);

            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //float aspectRatio = 0.9f;

            camProjection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, cutPlaneDistance, 10000.0f);

            camView = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, Zoom), Vector3.Zero, Vector3.Up);

            BoundingFrustum frustum = new BoundingFrustum(camView * camProjection);

            Vector3[] corners = new Vector3[4];

            corners = frustum.GetCorners();

            kinectProjection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(57.0f),
                4.0f / 3.0f, 1.0f, 50000.0f);

            kinectView =
                Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);

            View = Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero,
                Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 16.0f / 9.0f, 1, 500);

            //setupOSC();
            //setup speech recognition
            setupSpeech();

            prevState = Keyboard.GetState();
            base.Initialize();
        }

        private void setupSpeech()
        {
            //setup audio
            source = new KinectAudioSource();
            source.FeatureMode = true;
            source.AutomaticGainControl = false;
            source.SystemMode = SystemMode.OptibeamArrayOnly;

            RecognizerInfo ri = GetKinectRecognizer();

            if (ri == null)
            {
                Console.WriteLine("Could not find Kinect speech recognizer. Please refer to the sample requirements.");
                return;
            }

            Console.WriteLine("Using: {0}", ri.Name);

            sre = new SpeechRecognitionEngine(ri.Id);

            var options = new Choices();
            options.Add("Skin");
            options.Add("Circulatory");
            options.Add("Nervous");
            options.Add("Skeleton");

            var gb = new GrammarBuilder();
            //Specify the culture to match the recognizer in case we are running in a different culture.                                 
            gb.Culture = ri.Culture;
            gb.Append(options);


            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechDetected += SreSpeechDetected;
            sre.SpeechRecognized += SreSpeechRecognized;
            sre.SpeechHypothesized += SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;

            Stream s = source.Start();
            sre.SetInputToAudioStream(s,
                                      new SpeechAudioFormatInfo(
                                          EncodingFormat.Pcm, 16000, 16, 1,
                                          32000, 2, null));

            //Console.WriteLine("Recognizing. Say: 'red', 'green' or 'blue'. Press ENTER to stop");

            //sre.RecognizeAsync(RecognizeMode.Multiple);
            //Console.ReadLine();
            //Console.WriteLine("Stopping recognizer ...");
        }

        private RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
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
            sliceTexture = Content.Load<Texture2D>("Penguins");
            //gameShip2 = Content.Load<Model>("Ship");
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            //make sure the render target has a depth buffer
            viewport = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

            //setup initial opacities
            InitializeModelTags(currModel, 0.6f);

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

        public void InitializeModelTags(Model m, float alpha)
        {
            foreach (ModelMesh mesh in m.Meshes)
            {
                mesh.Tag = alpha;
            }
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
                    //Console.WriteLine(MathHelper.ToDegrees(angleX));

                    currHandYAngle = getAngleTrig(handRight.X, handRight.Z);
                    currHandXAngle = getAngleTrig(handRight.Y, handRight.Z);

                    //arbitrary thresholds
                    /*if (MathHelper.ToDegrees(angleY) > 140.0f)
                    {
                        if (MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f)
                        {
                            RotationYModel += (prevHandYAngle - currHandYAngle) * 5;
                            RotationXModel += (prevHandXAngle - currHandXAngle) * 5;
                        }
                    }*/

                    //setup slicing (currently arbitrary for testing)
                    cutPlaneDistance = -(currHeadPos.Z / 4.0f) + 270.0f;

                    //Console.WriteLine("Current Slice Depth: " + cutPlaneDistance + " Proximity: " + currHeadPos.Z);
                    if (MathHelper.ToDegrees(angleY) > 130.0f)
                    {
                       if (MathHelper.ToDegrees(angleX) > 100.0
                           && !(peelingForward || peelingBackward)
                           && (frameCounter > gestureGapDuration))
                       {
                           Console.WriteLine("Start Peel");
                           frameCounter = 0;
                           peelingForward = true;
                           
                       }

                       if (MathHelper.ToDegrees(angleX) < 60.0f && MathHelper.ToDegrees(angleX) > 40
                           && activeLayerIndex != 0.0f
                           && !(peelingForward || peelingBackward)
                           && (frameCounter > gestureGapDuration))
                       {
                           Console.WriteLine("Start Reverse Peel");
                           frameCounter = 0;
                           peelingBackward = true;
                           activeLayerIndex--;
                       }
                       
                       /*if (MathHelper.ToDegrees(angleX) > 100.0f && peeling)
                       {
                           Console.WriteLine("Finsh Reverse Peel");
                           peeling = false;
                       }
                       if (MathHelper.ToDegrees(angleX) < 60.0f && !peeling)
                       {
                           Console.WriteLine("Reverse Peel");
                           peeling = true;
                           activeLayerIndex--;
                       } */
                   }
                    
                 
                    if (peelingForward)
                    {
                        //Console.WriteLine("Frame: " + frameCounter);
                        if (MathHelper.ToDegrees(angleX) < 60.0f)
                        {
                            Console.WriteLine("Finish Peel");
                            peelingForward = false;
                            frameCounter = 0;
                            layerOpacities[activeLayerIndex] = 0.0f;
                            activeLayerIndex++;
                        }
                        else if (frameCounter < frameLimit)
                        {
                            layerOpacities[activeLayerIndex] = (MathHelper.ToDegrees(angleX) - 60.0f) / 70.0f;
                            //layerOpacities[activeLayerIndex] -= 0.01f;
                        }
                        else
                        {
                            peelingForward = false;
                            frameCounter = 0;
                            //layerOpacities[activeLayerIndex] = 1.0f;
                        }
                    }
                    if (peelingBackward)
                    {
                        if (MathHelper.ToDegrees(angleX) > 130.0f)
                        {
                            Console.WriteLine("Finish Reverse Peel");
                            peelingBackward = false;
                            frameCounter = 0;
                            layerOpacities[activeLayerIndex] = 1.0f;
                        }
                        else if (frameCounter < frameLimit)
                        {
                            layerOpacities[activeLayerIndex] = (MathHelper.ToDegrees(angleX) - 60.0f) / 70.0f;
                            //layerOpacities[activeLayerIndex] -= 0.01f;
                        }
                        else
                        {
                            peelingBackward = false;
                            frameCounter = 0;
                            layerOpacities[activeLayerIndex] = 1.0f;
                        }
                    }



                    prevHandYAngle = currHandYAngle;
                    prevHandXAngle = currHandXAngle;
                    //currHeadPos = Vector3.Transform(currHeadPos, Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation2)) * Matrix.CreateTranslation(cameraTranslation2));

                    //Console.WriteLine("Hand Y Angle: " + MathHelper.ToDegrees(angleY) + " X Angle: " + MathHelper.ToDegrees(angleX));
                    //Console.WriteLine("Head Pos: " + currHeadPos.X + " " + currHeadPos.Y + " " + currHeadPos.Z);

                    //if (currHeadPos.Z < 330)
                    //{
                        headPos = Vector3.Transform(currHeadPos, Matrix.CreateTranslation(cameraTranslation) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation)));
                    //}

                        Viewport cameraView = GraphicsDevice.Viewport;
                       

                    Vector3 handTemp = new Vector3(-handRight.X * 1000.0f, handRight.Y * 1000.0f, handRight.Z * 100.0f + 250.0f);
                    Vector3 elbowTemp = new Vector3(elbowRight.X * 100.0f, elbowRight.Y * 100.0f, elbowRight.Z * 100.0f + 250.0f);

                    //Console.WriteLine("HAND: " + handTemp.X + " " + handTemp.Y + " " + handTemp.Z);

                    //Vector3 line = new Vector3(handTemp.X - elbowTemp.X, handTemp.Y - elbowTemp.Y, handTemp.Z - elbowTemp.Z);
                    //line = cameraView.Project(line, kinectProjection, kinectView, Matrix.Identity);

                    //handTemp = Vector3.Transform(handTemp, (Matrix.CreateScale(2.94553348f) * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 120.0f))));
                    //elbowTemp = Vector3.Transform(elbowTemp, (Matrix.CreateScale(2.94553348f) * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 120.0f))));


                  
                    //testPosition.X = elbowTemp.X + ((handTemp.X - elbowTemp.Z) * t);
                    handTemp = cameraView.Project(handTemp, kinectProjection, kinectView, Matrix.Identity);
                    //elbowTemp = cameraView.Project(elbowTemp, kinectProjection, kinectView, Matrix.Identity);

                    screenPos = handTemp;

                    Vector3 testVector = new Vector3(screenPos.X, screenPos.Y, 0.0f);
                    //Console.WriteLine(screenPos.X + " " + screenPos.Y + " " + screenPos.Z);
                    //DrawSkeleton(data);

                    Ray ray = new Ray(testVector, Vector3.Normalize(handTemp - testVector));

                    checkIntersection(currModel, handTemp);
                    //Console.WriteLine(ray.Position.X + " " + ray.Position.Y + " " + ray.Position.Z);
                    //Console.WriteLine(ray.Direction.X + " " + ray.Direction.Y + " " + ray.Direction.Z);
                    //checkIntersection(currModel, ray);
                }
            }
        }


        private void SreSpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine("\nSpeech Detected");

        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("\nSpeech Rejected");

        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //This first release of the Kinect language pack doesn't have a reliable confidence model, so 
            //we don't use e.Result.Confidence here.
            Console.WriteLine("\nSpeech Recognized: \t{0}", e.Result.Text);
            if (e.Result.Text == "Skin")
            {
                //Array.Clear(layerOpacities, 0, layerOpacities.Length);
                Console.WriteLine("Set to skin");
                layerOpacities[0] = 1.0f;
            }
            else if (e.Result.Text == "Muscle")
            {
                Array.Clear(layerOpacities, 0, layerOpacities.Length);
                layerOpacities[1] = 1.0f;
            }
            else if (e.Result.Text == "Skeleton")
            {
                Console.WriteLine("Set to skeleton");
                Array.Clear(layerOpacities, 0, layerOpacities.Length);
                layerOpacities[2] = 1.0f;
            }
            else if (e.Result.Text == "Respiratory")
            {
                Array.Clear(layerOpacities, 0, layerOpacities.Length);
                layerOpacities[3] = 1.0f;
            }
            else if (e.Result.Text == "Respiratory")
            {
                Array.Clear(layerOpacities, 0, layerOpacities.Length);
                layerOpacities[3] = 1.0f;
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

            RotationYWorld = getAngleTrig(headPos.Z, headPos.X);
            RotationXWorld = -getAngleTrig(headPos.Z, headPos.Y);

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
                sre.RecognizeAsyncStop();
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
            
            m.CopyAbsoluteBoneTransformsTo(transforms);

            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            //float aspectRatio = 0.9f;

            if (slicing)
            {
                camProjection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, cutPlaneDistance, 10000.0f);
            }
            else
            {
                Matrix projection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, 1.0f, 10000.0f);
            }
            
            //just changed, was backwards
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            camPos = Vector3.Transform(camPos, pitch);
            camPos = Vector3.Transform(camPos, yaw);

            //Vector3 camPos = new Vector3(camX, camYAngleX, camZAngleX);
            camView = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);

            //Draw each layer bade on mesh indices
            //Opacity is determined by user position
            //the order of drawing is important (DON'T CHANGE!)
            

            //nervous
            drawLayer(m, 180, 184, layerOpacities[7], scale, transforms);
            //circulatory
            drawLayer(m, 177, 179, layerOpacities[6], scale, transforms);
            //urinary
            drawLayer(m, 176, 176, layerOpacities[5], scale, transforms);
            //digestive
            drawLayer(m, 4, 4, layerOpacities[4], scale, transforms);
            //respiratory
            drawLayer(m, 172, 175, layerOpacities[3], scale, transforms);
            //skeletal
            drawLayer(m, 146, 171, layerOpacities[2], scale, transforms);
            //muscle
            //drawLayer(m, 5, 145, layerOpacities[1], scale, transforms);
            drawLayer(m, 5, 145, 0.0f, scale, transforms);
            //skin
            //drawLayer(m, 0, 3, layerOpacities[0], scale, transforms);
            drawLayer(m, 0, 3, 0.0f, scale, transforms);

            //drawLayer(m, 177, 179, 1.0f, scale, view, projection, transforms);

        }

        private void drawLayer(Model modelIn, int startIndex, int endIndex, float alphaIn, float scale, Matrix[] transforms)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                foreach (BasicEffect effect in modelIn.Meshes[i].Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = camView;
                    effect.Projection = camProjection;
                    effect.World = gameWorldRotation *
                        transforms[modelIn.Meshes[i].ParentBone.Index] *
                        Matrix.CreateScale(scale);

                    if (alphaIn == 0.0f)
                    {
                        modelIn.Meshes[i].Tag = 0.0f;
                    }
                    if (pointing)
                    {
                        if (alphaIn != 0.0f)
                        {
                            effect.Alpha = (float)modelIn.Meshes[i].Tag;
                        }
                    }
                    else
                    {
                        if (alphaIn < 1.0f && alphaIn != 0.0f)
                        {
                            modelIn.Meshes[i].Tag = alphaIn;
                        }
                        else
                        {
                            modelIn.Meshes[i].Tag = 1.0f;
                        }
                        //debug with heart
                    }
                    if ((float)modelIn.Meshes[i].Tag > 0.0f && alphaIn != 0.0f)
                    {
                        modelIn.Meshes[i].Draw();
                    }
                }
            
            }
        }

        public void drawSlicingPlane(float slicePosition)
        {
            BasicEffect sliceEffect = new BasicEffect(graphics.GraphicsDevice);
            Vector3 sliceCamPos = new Vector3(0.0f, 0.0f, Zoom);

            Matrix sliceView = Matrix.CreateLookAt(sliceCamPos, Vector3.Zero, Vector3.Up);

            sliceEffect.World = gameWorldRotation * Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, slicePosition));
            sliceEffect.View = sliceView;
            sliceEffect.Alpha = 0.4f;
            sliceEffect.Projection = camProjection;
            sliceEffect.Texture = sliceTexture;
            sliceEffect.TextureEnabled = true;
           
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            short[] Indexes = new short[6];
            vertices[0] = new VertexPositionNormalTexture(new Vector3(-40, 80, 0), Vector3.Up, new Vector2(0.0f,0.0f));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(40, 80, 0), Vector3.Up, new Vector2(1.0f, 0.0f));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(-40, -80, 0), Vector3.Up, new Vector2(0.0f, 1.0f));
            vertices[3] = new VertexPositionNormalTexture(new Vector3(40, -80, 0), Vector3.Up, new Vector2(1.0f, 1.0f));

            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;

            foreach (EffectPass pass in sliceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives
                    <VertexPositionNormalTexture>(
                    PrimitiveType.TriangleList,
                    vertices, 0, 4,
                    Indexes, 0, 2);
            }
        }

        private void checkIntersection(Model model, Vector3 point)
        {

            BoundingSphere boundingSphere;
            Matrix[] transforms = new Matrix[model.Bones.Count];
            //MouseState ms = Mouse.GetState();

            Vector3 nearPoint = new Vector3(point.X, point.Y, 0);
            Vector3 farPoint = new Vector3(point.X, point.Y, 1);

            Viewport viewport = GraphicsDevice.Viewport;
            nearPoint = viewport.Unproject(nearPoint, camProjection, camView, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, camProjection, camView, Matrix.Identity);

            Ray ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

            //Console.WriteLine("Ray X: " + ray.Direction.X + " Y: " + ray.Direction.Y + " Z: " + ray.Direction.Z);
            //Console.WriteLine("Screen X: " + screenPos.X + " Y: " + screenPos.Y + " Z: " + screenPos.Z);
            //DrawTestModel(lander, 10.0f, );

            model.CopyAbsoluteBoneTransformsTo(transforms);

            bool intersect = false;

            foreach (ModelMesh mesh in model.Meshes)
            {
           
                boundingSphere = mesh.BoundingSphere.Transform(gameWorldRotation *
                        transforms[mesh.ParentBone.Index] *
                        Matrix.CreateScale(modelScale));

                //if (ray.Intersects(boundingSphere) != null && (bool)mesh.Tag)
                if (ray.Intersects(boundingSphere) != null && (float)mesh.Tag != 0.0f)
                {

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        intersect = CheckIntersectionTriangles(ray, part, gameWorldRotation *
                                                    transforms[mesh.ParentBone.Index] *
                                                    Matrix.CreateScale(modelScale));

                        if (intersect)
                        {
                            //Console.WriteLine("MESH: " + mesh.Name);
                            mesh.Tag = 0.6f;
                            break;
                        }
                        else
                        {
                            mesh.Tag = 0.05f;
                        }
                    }
                }
                else
                {
                    mesh.Tag = 0.05f;
                }
                if (intersect)
                    break;
            }
        }

        private bool CheckIntersectionTriangles(Ray ray, ModelMeshPart meshPart, Matrix transform)
        {
            // Read the format of the vertex buffer  
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            // Find the element that holds the position  
            VertexElement vertexPosition = new VertexElement();
            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position &&
                    vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    // There should only be one  
                    break;
                }
            }
            // Check the position element found is valid  
            if (vertexPosition == null ||
                vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
            {
                throw new Exception("Model uses unsupported vertex format!");
            }
            // This where we store the vertices until transformed  
            Vector3[] allVertex = new Vector3[meshPart.NumVertices];
            // Read the vertices from the buffer in to the array  
            meshPart.VertexBuffer.GetData<Vector3>(
                meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                allVertex,
                0,
                meshPart.NumVertices,
                declaration.VertexStride);

            //transform vertices
            for (int i = 0; i != allVertex.Length; ++i)
            {
                allVertex[i] = Vector3.Transform(allVertex[i], transform);
            }

            // Each primitive is a triangle  
            UInt32[] indexElements = new UInt32[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<UInt32>(meshPart.StartIndex * 4, indexElements, 0, meshPart.PrimitiveCount * 3);

            // Each TriangleVertexIndices holds the three indexes to each vertex that makes up a triangle  
            
            for (int i = 0; i != meshPart.PrimitiveCount; ++i)
            {
                UInt32 index1 = indexElements[i * 3 + 0];
                UInt32 index2 = indexElements[i * 3 + 1];
                UInt32 index3 = indexElements[i * 3 + 2];

                Vector3 vertex1 = allVertex[indexElements[i * 3 + 0]];
                Vector3 vertex2 = allVertex[indexElements[i * 3 + 1]];
                Vector3 vertex3 = allVertex[indexElements[i * 3 + 2]];

                if (RayIntersectTriangle(ray, vertex1, vertex2, vertex3))
                {
                    //Console.WriteLine("Triangle Intersection");
                    return true;
                }
            }
            return false;
            // Store our triangles   
        }

        private bool RayIntersectTriangle(Ray ray, Vector3 tri0, Vector3 tri1, Vector3 tri2)
        {
            float pickDistance, barycentricU, barycentricV;
            // Find vectors for two edges sharing vert0
            Vector3 edge1 = tri1 - tri0;
            Vector3 edge2 = tri2 - tri0;

            // Begin calculating determinant - also used to calculate barycentricU parameter
            Vector3 pvec = Vector3.Cross(ray.Direction, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f)
                return false;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = ray.Position - tri0;

            // Calculate barycentricU parameter and test bounds
            barycentricU = Vector3.Dot(tvec, pvec);
            if (barycentricU < 0.0f || barycentricU > det)
                return false;

            // Prepare to test barycentricV parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate barycentricV parameter and test bounds
            barycentricV = Vector3.Dot(ray.Direction, qvec);
            if (barycentricV < 0.0f || barycentricU + barycentricV > det)
                return false;

            // Calculate pickDistance, scale parameters, ray intersects triangle
            pickDistance = Vector3.Dot(edge2, qvec);
            float fInvDet = 1.0f / det;
            pickDistance *= fInvDet;
            barycentricU *= fInvDet;
            barycentricV *= fInvDet;

            return true;
        }
        
        //delete later
        private void DrawTestModel(Model m, float scale, Vector3 position)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);

            //for debugging
            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    
                    effect.EnableDefaultLighting();
                    effect.Alpha = 1.0f;
                    effect.View = camView;
                    effect.Projection = camProjection;
                    effect.World = gameWorldRotation *
                        transforms[mesh.ParentBone.Index]
                         * Matrix.CreateTranslation(position)
                        * Matrix.CreateScale(scale);
                }
                mesh.Draw();
            }
        }

        private void DrawSkeleton(SkeletonData skeleton)
        {
            BasicEffect skeletonEffect = new BasicEffect(graphics.GraphicsDevice);

            skeletonEffect.View = camView;
            skeletonEffect.Projection = camProjection;
            skeletonEffect.World = Matrix.Identity;
            skeletonEffect.VertexColorEnabled = true;

            VertexPositionColor[] vertices = new VertexPositionColor[5];

            Vector3 headPos = new Vector3(skeleton.Joints[JointID.Head].Position.X * 100.0f,
                skeleton.Joints[JointID.Head].Position.Y * 100.0f,
                skeleton.Joints[JointID.Head].Position.Z * 100.0f - 100.0f);

            Vector3 shoulderCenter = new Vector3(skeleton.Joints[JointID.ShoulderCenter].Position.X * 100.0f,
                skeleton.Joints[JointID.ShoulderCenter].Position.Y * 100.0f,
                skeleton.Joints[JointID.ShoulderCenter].Position.Z * 100.0f);

            Vector3 shoulderRight = new Vector3(skeleton.Joints[JointID.ShoulderRight].Position.X * 100.0f,
                skeleton.Joints[JointID.ShoulderRight].Position.Y * 100.0f,
                skeleton.Joints[JointID.ShoulderRight].Position.Z * 100.0f);

            Vector3 elbowRight = new Vector3(skeleton.Joints[JointID.ElbowRight].Position.X * 100.0f,
                skeleton.Joints[JointID.ElbowRight].Position.Y * 100.0f,
                skeleton.Joints[JointID.ElbowRight].Position.Z * 100.0f);

            Vector3 handRight = new Vector3(skeleton.Joints[JointID.HandRight].Position.X * 100.0f,
                skeleton.Joints[JointID.HandRight].Position.Y * 100.0f,
                skeleton.Joints[JointID.HandRight].Position.Z * 100.0f);

            vertices[0] = new VertexPositionColor(headPos, Color.Red);
            vertices[1] = new VertexPositionColor(shoulderCenter, Color.Red);
            vertices[2] = new VertexPositionColor(shoulderRight, Color.Red);
            vertices[3] = new VertexPositionColor(elbowRight, Color.Red);
            vertices[4] = new VertexPositionColor(handRight, Color.Red);

            //Console.WriteLine("Head X: " + headPos.X + " Y: " + headPos.Y + " Z: " + headPos.Z);

            foreach (EffectPass pass in skeletonEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, 4);
            }
        }

        private void DrawRay(Ray rayIn)
        {
            BasicEffect rayEffect = new BasicEffect(graphics.GraphicsDevice);

            Vector3 newCamPos = new Vector3(0.0f, 0.0f, 250.0f);
            Matrix newView = Matrix.CreateLookAt(newCamPos, Vector3.UnitZ, Vector3.Up);

            Matrix newProjection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                16.0f / 9.0f, 1.0f, 10000.0f);

            rayEffect.View = camView;
            rayEffect.Projection = camProjection;
            rayEffect.World = Matrix.Identity;
            rayEffect.VertexColorEnabled = true;

            VertexPositionColor[] vertices = new VertexPositionColor[4];

            vertices[0] = new VertexPositionColor(rayIn.Position * -1000.0f * rayIn.Direction, Color.Red);
            vertices[1] = new VertexPositionColor(rayIn.Position * 1000.0f * rayIn.Direction, Color.Red);

            foreach (EffectPass pass in rayEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
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
            //this.GraphicsDevice.SetRenderTarget(viewport);
            frameCounter++;
            this.GraphicsDevice.SetRenderTarget(null);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.GraphicsDevice.Clear(Color.White);

            this.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            DrawModel(currModel, modelScale);

            spriteBatch.Begin();
            //spriteBatch.Draw(colorFeed, new Rectangle(0, 0, 160, 120), Color.White);
            spriteBatch.Draw(testingTexture, new Rectangle((int)screenPos.X - 5, (int)screenPos.Y - 5, 10, 10), Color.White);
            spriteBatch.End();

            //drawSlicingPlane(Zoom - cutPlaneDistance - 1.0f);
            //DrawTestModel(lander, 10.0f, new Vector3(20.0f, 1.0f, 1.0f));

            //DrawRay(ray);
            //check for intersection
            //checkIntersection(currModel);
            //DrawForCylinder();

            //for testing projector alignment
            //DrawCalibrationPattern();

            base.Draw(gameTime);
        }
    }
}
