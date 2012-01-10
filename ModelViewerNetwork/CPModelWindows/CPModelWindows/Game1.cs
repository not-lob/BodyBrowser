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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Vector3 rayPosition = new Vector3(1280.0f, 720.0f, 0.0f);

        Vector3 floorPlane = new Vector3(-0.02085731f, 0.8743463f, -0.4848542f);

        private bool pointing = false;
        private bool slicing = false;
        private bool gestures = false;
        private bool labels = false;
        private bool focused = false;
        private bool unfocusing = false;

        private bool wasPointing = false;

        private float pointingOpacity = 0.2f;
        private ModelMesh intersected = null;
        private float newZoom;
        private Vector3 cameraPos = new Vector3(0.0f, 0.0f, 0.0f);

        private float initialZoom = 200.0f;
        private float Zoom;
        private float prevZoom = 200.0f;

        private float cameraY = 0.0f;
        private float cameraNewY = 0.0f;

        private Vector3 cameraLookAt = Vector3.Zero;

        private Vector2 pointingLabelPos = new Vector2(17, -68);

        private float rotationYWorldOffset = 0.0f;

        //change the brightnes to improve display on the cylinder
        private float brightness = 0.29f;

        Runtime nui;

        KinectAudioSource source;
        SpeechRecognitionEngine sre;

        private Model anatomy;

        private SpriteFont labelFont;

        private Vector3 Position = Vector3.One;

        private Vector2 cursorPosition;

        private Vector3 currHeadPos = new Vector3(0.0f, 0.0f, 100.0f);
        Vector3 headPos;

        private float userProximity = 200.0f;

        //active layer
        float[] layerOpacities = new float[8];
        //int activeLayerIndex;

        //bool values for gestures
        bool peelingForward = false;
        bool peelingBackward = false;


        //vertical placement of view on cylinder
        private float distortionShift = 0.0260000024f;


        private KeyboardState prevState;
        //Sphere position relative to kinect
        //Vector3 spherePos = new Vector3(0.0f, 1346.2f, 0.0f);
        Vector3 cameraTranslation = new Vector3(0.0f, 0.0f, 0.0f);
        private float cameraYRotation = 0.0f;

        //Control camera distance from anatomy
        

        //aspect ratio 0.9 for cylinder, screen otherwise
        private float aspectRatio = 0.85f;
        //private float aspectRatio = 16.0f / 9.0f;

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
        private Matrix gameWorldRotationViewport;


        private int rotation;

        private int numUsers = 0;
        private int userID = -1;
        //private uint userID; 

        private string calibPose;

        #region Hand and User Positions
        private float prevHandYAngle;
        private float currHandYAngle;

        private float prevHandXAngle;
        private float currHandXAngle;

        private float prevLeftHandYAngle;
        private float currLeftHandYAngle;

        private float prevLeftHandXAngle;
        private float currLeftHandXAngle;

        private Vector3 prevHandPos;
        private Vector3 currHandPos;

        private Vector3 prevLeftHandPos;
        private Vector3 currLeftHandPos;

        //values for tracking user position
        private float prevUserAngleY;
        private float currUserAngleY;

        private float prevUserAngleX;
        private float currUserAngleX;
    #endregion Hand and User Positions

        RenderTarget2D viewport;
        //RenderTargetCube renderTarget;
        Texture2D calibrationTexture, cursorTexture;

        Vector2 pos = new Vector2(0, 0);
        GraphicsDevice device;

        SpriteBatch spriteBatch;

        GraphicsDeviceManager graphics;

        Distortion distortion;
        VertexDeclaration vertexDeclaration, vertexSliceDeclaration;
        Matrix View, Projection, camView, camProjection, kinectView, kinectProjection;
        BasicEffect distortionEffect, sliceEffect;

        #region Peel Animation
        //For peel animation
        enum PeelMode
        {
            None = 0,
            PeelingForward,
            PeelingBackward
        }
        PeelMode peelMode = PeelMode.None;

        //threshold for peel gesture
        private float peelMoveThreshold = 0;
        //this lock is to confirm that change one layer in one time guesture.
        private bool peelLock = false;

        //timer used for peeling
        public int frameCounter = 0;
        //limit for a gesture to be performed
        int frameLimit = 300;

        //duration before gestures are detected
        int gestureGapDuration = 50;

        bool bPeelValid = false;

        #endregion Peel Animation

        #region Network Variables
        private IPAddress kinectAddress1 = IPAddress.Parse("130.15.5.213");
        private IPAddress kinectAddress2 = IPAddress.Parse("130.15.5.207");

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
        float shiftAngleLocal = 40.5f;
        float shiftAngleRemote1;
        float shiftAngleRemote2;
        #endregion Network Variables

        private float cutPlaneDistance = 1.0f;

        private Vector3 screenPos = new Vector3(0, 0, 0);

        #region Label Variables
        //For label indication line
        BasicEffect labelIndicationLineEffect;
        //skin far distance label indication line
        VertexDeclaration skinLabelFarIndicationLineVertexDeclaration;
        VertexPositionColor[] skinLabelFarIndicationLinePointList;
        VertexBuffer skinLabelFarIndicationLineVertexDeclarationVertexBuffer;
        //skin med distance label indication line
        VertexDeclaration skinLabelMedIndicationLineVertexDeclaration;
        VertexPositionColor[] skinLabelMedIndicationLinePointList;
        VertexBuffer skinLabelMedIndicationLineVertexDeclarationVertexBuffer;
        //skin near distance label indication line
        VertexDeclaration skinLabelNearIndicationLineVertexDeclaration;
        VertexPositionColor[] skinLabelNearIndicationLinePointList;
        VertexBuffer skinLabelNearIndicationLineVertexDeclarationVertexBuffer;

        //muscular far distance label indication line
        VertexDeclaration muscularLabelFarIndicationLineVertexDeclaration;
        VertexPositionColor[] muscularLabelFarIndicationLinePointList;
        VertexBuffer muscularLabelFarIndicationLineVertexDeclarationVertexBuffer;

        //organic far distance label indication line
        VertexDeclaration organicLabelFarIndicationLineVertexDeclaration;
        VertexPositionColor[] organicLabelFarIndicationLinePointList;
        VertexBuffer organicLabelFarIndicationLineVertexDeclarationVertexBuffer;

        //skeleton far distance label indication line
        VertexDeclaration skeletonLabelFarIndicationLineVertexDeclaration;
        VertexPositionColor[] skeletonLabelFarIndicationLinePointList;
        VertexBuffer skeletonLabelFarIndicationLineVertexDeclarationVertexBuffer;
        //skeleton med distance label indication line
        VertexDeclaration skeletonLabelMedIndicationLineVertexDeclaration;
        VertexPositionColor[] skeletonLabelMedIndicationLinePointList;
        VertexBuffer skeletonLabelMedIndicationLineVertexDeclarationVertexBuffer;
        //skeleton near distance label indication line
        VertexDeclaration skeletonLabelNearIndicationLineVertexDeclaration;
        VertexPositionColor[] skeletonLabelNearIndicationLinePointList;
        VertexBuffer skeletonLabelNearIndicationLineVertexDeclarationVertexBuffer;

        //Create effect for text sprite
        BasicEffect labelEffect;
        
        enum ModelLayer
        {
            Skin,
            Muscle,
            Organic,
            Skeleton,
            Circulatory,
            Nervous
        }
        ModelLayer activeLayerIndex; //0-skin, 1-muscle, 2-organic, 3-skeleton, 4-circulatory, 5-nervous

        static Color Red = new Color(204, 0, 51);
        static Color Yellow = new Color(255, 204, 0);
        static Color Purple = new Color(153, 0, 102);
        //Skin label color
        Color skinLabelColorFar = Yellow;//Color.Red;
        Color skinLabelColorMed = Red;//Color.Blue;
        Color skinLabelColorNear = Purple;//Color.Green;

        //Skin label text
        String[] skinFrontLabelTextFar = { "Head", "Neck", "Elbow", "Hand", "Thorax", "Abdomen", "Knee", "Foot" };
        String[] skinBackLabelTextFar = { "Head", "Spine", "Hip" };
        String[] skinFrontLabelTextMed = { "Temple", "Ear", "Eye", "Forehead", "Shoulder", "Breast", "Nipple", "Navel", "Groin", "Pubis" };
        String[] skinFrontLabelTextNear = { "Eyelid", "Pupil", "Iris", "Sclera", "Ankle", "Instep", "Toe" };

        //Muscular and organic label color
        Color muscularLayerColor = new Color(153, 204, 0);
        Color organicLayerColor = new Color(255, 153, 51);

        //Skeleton label color
        Color skeletonLayerColorFar = new Color(153, 204, 204);
        Color skeletonLayerColorMed = new Color(255, 153, 51);
        Color skeletonLayerColorNear = new Color(255, 255, 0);
        //Skeleton label text
        String[] skeletonFrontLabelTextFar = { "Skull", "Clavicle", "Rib", "Humerus", "Vertebrae", "Os coxa", "Femur", "Tibia" };
        String[] skeletonBackLabelTextFar = { "Scapula", "Vertebrae", "Fibula", "Tarsals" };
        String[] skeletonFrontLabelTextMed = { "Sternum", "Floating rib", "Spinal column" };

        Vector3 glNearPoint;
        #endregion Label Variables

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
            userPosKinect1.X = (float)e.Bundle.Messages[0].Data[0];
            userPosKinect1.Y = (float)e.Bundle.Messages[0].Data[1];
            userPosKinect1.Z = (float)e.Bundle.Messages[0].Data[2];

            //Console.WriteLine("Kinect 1 Updated X: " + userPosKinect1.X + " " + userPosKinect1.Y + " " + userPosKinect1.Z);

                trackingRemote1 = true;
                angleYRemote1 = getAngleTrig(userPosKinect1.Z, userPosKinect1.X) + MathHelper.ToRadians(shiftAngleRemote1);
                //angleXRemote1 = -getAngleTrig(userPosKinect1.Z, userPosKinect1.Y);
        }

        //OSC Methods
        void kinect2Updated(object sender, OscBundleReceivedEventArgs e)
        {
            userPosKinect2.X = (float)e.Bundle.Messages[0].Data[0];
            userPosKinect2.Y = (float)e.Bundle.Messages[0].Data[1];
            userPosKinect2.Z = (float)e.Bundle.Messages[0].Data[2];

            //Console.WriteLine("Kinect 2 Updated X: " + userPosKinect2.X + " " + userPosKinect2.Y + " " + userPosKinect2.Z);

            //if (userPosKinect2.X != 0.0f && userPosKinect2.Y != 0.0f && userPosKinect2.Z != 2700.0f)
            //{
                trackingRemote1 = true;
                angleYRemote2 = getAngleTrig(userPosKinect2.Z, userPosKinect2.X) + MathHelper.ToRadians(shiftAngleRemote2);
                //angleXRemote2 = -getAngleTrig(userPosKinect2.Z, userPosKinect2.Y);
            //}
            //else
            //{
               // trackingRemote1 = false;
            //}
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
            //this.IsMouseVisible = true;

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

            Console.WriteLine("Camera Angle " + nui.NuiCamera.ElevationAngle);

            //set zoom
            Zoom = initialZoom;
            newZoom = Zoom;
            cameraPos.Z = Zoom;

            handPos[0] = new Vector3(0.0f, 0.0f, 0.0f);
            handPos[1] = new Vector3(0.0f, 0.0f, 0.0f);

            //set up angles
            shiftAngleRemote1 = -36.0f;
            shiftAngleRemote2 = -92.0f;

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

            activeLayerIndex = 0;

            //for cylinder
            //distortion = new Distortion(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1, distortionShift, 4);

            if (slicing)
            {
                camProjection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, cutPlaneDistance, 10000.0f);
            }
            else
            {
                camProjection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, 1.0f, 10000.0f);
            }

            camView = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, Zoom), Vector3.Zero, Vector3.Up);

            BoundingFrustum frustum = new BoundingFrustum(camView * camProjection);

            kinectProjection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(57.0f),
                4.0f / 3.0f, 1.0f, 50000.0f);

            kinectView =
                Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up);

            View = Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero,
                Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 16.0f / 9.0f, 1, 500);

            //Set Layer and enable Labels
            labelEffect = new BasicEffect(GraphicsDevice);

            activeLayerIndex = ModelLayer.Skin;

            InitializeLabelIndicationLine();

            setupOSC();
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
            options.Add("Show me the skin");
            options.Add("Show me the muscles");
            options.Add("Show me the circulatory system");
            options.Add("Show me the organs");
            options.Add("Show me the nervous system");
            options.Add("Show me the skeleton");
            options.Add("Start Pointing");
            options.Add("Start Slicing");
            options.Add("Stop Pointing");
            options.Add("Stop Slicing");
            options.Add("Show labels");
            options.Add("Stop showing labels");
            options.Add("Focus");
            options.Add("Go Back");
            options.Add("Show me the heart");
            options.Add("Show me the brain");
            options.Add("Show me the skull");
            options.Add("Show me the lungs");

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

            sre.RecognizeAsync(RecognizeMode.Multiple);
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

        private void InitializeLabelIndicationLine()
        {
            labelIndicationLineEffect = new BasicEffect(this.GraphicsDevice);
            labelIndicationLineEffect.VertexColorEnabled = true;

            //Initialize skin far distance label indication line
            skinLabelFarIndicationLinePointList = new VertexPositionColor[22];
            //Head
            skinLabelFarIndicationLinePointList[0] = new VertexPositionColor(
                new Vector3(0, -60, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[1] = new VertexPositionColor(
                new Vector3(17, -63, 0), skinLabelColorFar);
            //Neck
            skinLabelFarIndicationLinePointList[2] = new VertexPositionColor(
                new Vector3(-8, -42, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[3] = new VertexPositionColor(
                new Vector3(-25, -48, 0), skinLabelColorFar);
            //Elbow
            skinLabelFarIndicationLinePointList[4] = new VertexPositionColor(
                new Vector3(-30, -10, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[5] = new VertexPositionColor(
                new Vector3(-60, -1, 0), skinLabelColorFar);
            //Hand
            skinLabelFarIndicationLinePointList[6] = new VertexPositionColor(
                new Vector3(-40, 30, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[7] = new VertexPositionColor(
                new Vector3(-50, 35, 0), skinLabelColorFar);
            //Thorax
            skinLabelFarIndicationLinePointList[8] = new VertexPositionColor(
                new Vector3(0.8f, -13, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[9] = new VertexPositionColor(
                new Vector3(40, -7, 0), skinLabelColorFar);
            //Abdomen
            skinLabelFarIndicationLinePointList[10] = new VertexPositionColor(
                new Vector3(0.5f, -0.6f, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[11] = new VertexPositionColor(
                new Vector3(37, 14, 0), skinLabelColorFar);
            //Knee
            skinLabelFarIndicationLinePointList[12] = new VertexPositionColor(
                new Vector3(-11, 63, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[13] = new VertexPositionColor(
                new Vector3(-35, 68, 0), skinLabelColorFar);
            //Foot
            skinLabelFarIndicationLinePointList[14] = new VertexPositionColor(
                new Vector3(15, 110, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[15] = new VertexPositionColor(
                new Vector3(30, 110, 0), skinLabelColorFar);

            //Back head
            skinLabelFarIndicationLinePointList[16] = new VertexPositionColor(
                new Vector3(0, -60, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[17] = new VertexPositionColor(
                new Vector3(17, -63, 0), skinLabelColorFar);
            //Back spine
            skinLabelFarIndicationLinePointList[18] = new VertexPositionColor(
                new Vector3(0, -8, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[19] = new VertexPositionColor(
                new Vector3(-40, -5, 0), skinLabelColorFar);
            //Back hip
            skinLabelFarIndicationLinePointList[20] = new VertexPositionColor(
                new Vector3(15, 18, 0), skinLabelColorFar);
            skinLabelFarIndicationLinePointList[21] = new VertexPositionColor(
                new Vector3(35, 21, 0), skinLabelColorFar);
            skinLabelFarIndicationLineVertexDeclaration = VertexPositionColorTexture.VertexDeclaration;
            // Initialize the vertex buffer, allocating memory for each vertex.
            skinLabelFarIndicationLineVertexDeclarationVertexBuffer = new VertexBuffer(graphics.GraphicsDevice, skinLabelFarIndicationLineVertexDeclaration,
                22, BufferUsage.None);
            // Set the vertex buffer data to the array of vertices.
            skinLabelFarIndicationLineVertexDeclarationVertexBuffer.SetData<VertexPositionColor>(skinLabelFarIndicationLinePointList);


            //Initialize skin med distance label indication line
            skinLabelMedIndicationLinePointList = new VertexPositionColor[18];
            //Temple
            skinLabelMedIndicationLinePointList[0] = new VertexPositionColor(
                new Vector3(6, -58, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[1] = new VertexPositionColor(
                new Vector3(14, -63, 0), skinLabelColorMed);
            //Ear
            skinLabelMedIndicationLinePointList[2] = new VertexPositionColor(
                new Vector3(-8, -53, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[3] = new VertexPositionColor(
                new Vector3(-17, -57, 0), skinLabelColorMed);
            //Eye
            skinLabelMedIndicationLinePointList[4] = new VertexPositionColor(
                new Vector3(4, -54, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[5] = new VertexPositionColor(
                new Vector3(14, -53, 0), skinLabelColorMed);
            //Forehead
            skinLabelMedIndicationLinePointList[6] = new VertexPositionColor(
                new Vector3(0, -61, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[7] = new VertexPositionColor(
                new Vector3(14, -73, 0), skinLabelColorMed);
            //Shoulder
            skinLabelMedIndicationLinePointList[8] = new VertexPositionColor(
                new Vector3(19, -36, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[9] = new VertexPositionColor(
                new Vector3(25, -42, 0), skinLabelColorMed);
            //Breast
            skinLabelMedIndicationLinePointList[10] = new VertexPositionColor(
                new Vector3(10, -27, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[11] = new VertexPositionColor(
                new Vector3(40, -16, 0), skinLabelColorMed);
            //Nipple
            skinLabelMedIndicationLinePointList[12] = new VertexPositionColor(
                new Vector3(11, -21.6f, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[13] = new VertexPositionColor(
                new Vector3(40, -5, 0), skinLabelColorMed);
            //Navel
            skinLabelMedIndicationLinePointList[14] = new VertexPositionColor(
                new Vector3(0.17f, 4.3f, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[15] = new VertexPositionColor(
                new Vector3(40, 21, 0), skinLabelColorMed);
            //Groin
            skinLabelMedIndicationLinePointList[16] = new VertexPositionColor(
                new Vector3(6.7f, 18.5f, 0), skinLabelColorMed);
            skinLabelMedIndicationLinePointList[17] = new VertexPositionColor(
                new Vector3(-40, 31, 0), skinLabelColorMed);
            skinLabelMedIndicationLineVertexDeclaration = VertexPositionColorTexture.VertexDeclaration;
            // Initialize the vertex buffer, allocating memory for each vertex.
            skinLabelMedIndicationLineVertexDeclarationVertexBuffer = new VertexBuffer(graphics.GraphicsDevice, skinLabelMedIndicationLineVertexDeclaration,
                18, BufferUsage.None);
            // Set the vertex buffer data to the array of vertices.
            skinLabelMedIndicationLineVertexDeclarationVertexBuffer.SetData<VertexPositionColor>(skinLabelMedIndicationLinePointList);

            //Initialize skin near distance label indication line
            skinLabelNearIndicationLinePointList = new VertexPositionColor[18];
            //Eyelid
            skinLabelNearIndicationLinePointList[0] = new VertexPositionColor(
                new Vector3(4, -55, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[1] = new VertexPositionColor(
                new Vector3(14, -63, 0), skinLabelColorNear);
            //Pupil
            skinLabelNearIndicationLinePointList[2] = new VertexPositionColor(
                new Vector3(4, -53, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[3] = new VertexPositionColor(
                new Vector3(14, -49, 0), skinLabelColorNear);
            //Iris
            skinLabelNearIndicationLinePointList[4] = new VertexPositionColor(
                new Vector3(4, -53, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[5] = new VertexPositionColor(
                new Vector3(14, -56, 0), skinLabelColorNear);
            //Sclera
            skinLabelNearIndicationLinePointList[6] = new VertexPositionColor(
                new Vector3(4, -53, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[7] = new VertexPositionColor(
                new Vector3(14, -70, 0), skinLabelColorNear);
            //Ankle
            skinLabelNearIndicationLinePointList[8] = new VertexPositionColor(
                new Vector3(13, 102, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[9] = new VertexPositionColor(
                new Vector3(30, 97, 0), skinLabelColorNear);
            //Instep
            skinLabelNearIndicationLinePointList[10] = new VertexPositionColor(
                new Vector3(14, 107, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[11] = new VertexPositionColor(
                new Vector3(30, 104, 0), skinLabelColorNear);
            //Toe
            skinLabelNearIndicationLinePointList[12] = new VertexPositionColor(
                new Vector3(15, 110, 0), skinLabelColorNear);
            skinLabelNearIndicationLinePointList[13] = new VertexPositionColor(
                new Vector3(30, 110, 0), skinLabelColorNear);
            skinLabelNearIndicationLineVertexDeclaration = VertexPositionColorTexture.VertexDeclaration;
            // Initialize the vertex buffer, allocating memory for each vertex.
            skinLabelNearIndicationLineVertexDeclarationVertexBuffer = new VertexBuffer(graphics.GraphicsDevice, skinLabelNearIndicationLineVertexDeclaration,
                18, BufferUsage.None);
            // Set the vertex buffer data to the array of vertices.
            skinLabelNearIndicationLineVertexDeclarationVertexBuffer.SetData<VertexPositionColor>(skinLabelNearIndicationLinePointList);

            //Initialize muscular far distance label indication line
            muscularLabelFarIndicationLinePointList = new VertexPositionColor[28];
            //Frontalis
            muscularLabelFarIndicationLinePointList[0] = new VertexPositionColor(
                new Vector3(0, -62, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[1] = new VertexPositionColor(
                new Vector3(12, -70, 0), muscularLayerColor);
            //Zygomaticus
            muscularLabelFarIndicationLinePointList[2] = new VertexPositionColor(
                new Vector3(5, -50, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[3] = new VertexPositionColor(
                new Vector3(10, -50, 0), muscularLayerColor);
            //Trapezius
            muscularLabelFarIndicationLinePointList[4] = new VertexPositionColor(
                new Vector3(-14, -37, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[5] = new VertexPositionColor(
                new Vector3(-30, -38, 0), muscularLayerColor);
            //Deltoid
            muscularLabelFarIndicationLinePointList[6] = new VertexPositionColor(
                new Vector3(-22, -35, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[7] = new VertexPositionColor(
                new Vector3(26, -38, 0), muscularLayerColor);
            //Brachioradialis
            muscularLabelFarIndicationLinePointList[8] = new VertexPositionColor(
                new Vector3(30, 2, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[9] = new VertexPositionColor(
                new Vector3(50, 2, 0), muscularLayerColor);
            //Pectoralis major
            muscularLabelFarIndicationLinePointList[10] = new VertexPositionColor(
                new Vector3(9, -26, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[11] = new VertexPositionColor(
                new Vector3(55, -20, 0), muscularLayerColor);
            //Rectus abdominus
            muscularLabelFarIndicationLinePointList[12] = new VertexPositionColor(
                new Vector3(0, -3, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[13] = new VertexPositionColor(
                new Vector3(-60, 0, 0), muscularLayerColor);
            //Rectus femoris
            muscularLabelFarIndicationLinePointList[14] = new VertexPositionColor(
                new Vector3(-14, 33, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[15] = new VertexPositionColor(
                new Vector3(-60, 36, 0), muscularLayerColor);
            //Fibularis longus
            muscularLabelFarIndicationLinePointList[16] = new VertexPositionColor(
                new Vector3(16, 76, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[17] = new VertexPositionColor(
                new Vector3(31, 75, 0), muscularLayerColor);
            //Extensor digitorum longus
            muscularLabelFarIndicationLinePointList[18] = new VertexPositionColor(
                new Vector3(-14, 98, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[19] = new VertexPositionColor(
                new Vector3(-40, 112, 0), muscularLayerColor);

            //Back Sternocleidomastoid
            muscularLabelFarIndicationLinePointList[20] = new VertexPositionColor(
                new Vector3(-4, -43, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[21] = new VertexPositionColor(
                new Vector3(-18, -43, 0), muscularLayerColor);
            //Back Latissimus dorsi
            muscularLabelFarIndicationLinePointList[22] = new VertexPositionColor(
                new Vector3(11, -17, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[23] = new VertexPositionColor(
                new Vector3(40, -8, 0), muscularLayerColor);
            //Back Gluteus maximus
            muscularLabelFarIndicationLinePointList[24] = new VertexPositionColor(
                new Vector3(11, 21, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[25] = new VertexPositionColor(
                new Vector3(40, 20, 0), muscularLayerColor);
            //Back Gastrocnemius
            muscularLabelFarIndicationLinePointList[26] = new VertexPositionColor(
                new Vector3(17, 85, 0), muscularLayerColor);
            muscularLabelFarIndicationLinePointList[27] = new VertexPositionColor(
                new Vector3(33, 81, 0), muscularLayerColor);
            muscularLabelFarIndicationLineVertexDeclaration = VertexPositionColorTexture.VertexDeclaration;
            // Initialize the vertex buffer, allocating memory for each vertex.
            muscularLabelFarIndicationLineVertexDeclarationVertexBuffer = new VertexBuffer(graphics.GraphicsDevice, muscularLabelFarIndicationLineVertexDeclaration,
                28, BufferUsage.None);
            // Set the vertex buffer data to the array of vertices.
            muscularLabelFarIndicationLineVertexDeclarationVertexBuffer.SetData<VertexPositionColor>(muscularLabelFarIndicationLinePointList);

            //Initialize organic far distance label indication line
            organicLabelFarIndicationLinePointList = new VertexPositionColor[18];
            //Larynx
            organicLabelFarIndicationLinePointList[0] = new VertexPositionColor(
                new Vector3(0, -40, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[1] = new VertexPositionColor(
                new Vector3(23, -43, 0), organicLayerColor);
            //Lung
            organicLabelFarIndicationLinePointList[2] = new VertexPositionColor(
                new Vector3(8, -18, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[3] = new VertexPositionColor(
                new Vector3(30, -16, 0), organicLayerColor);
            //Liver
            organicLabelFarIndicationLinePointList[4] = new VertexPositionColor(
                new Vector3(-6, -9, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[5] = new VertexPositionColor(
                new Vector3(-30, -5, 0), organicLayerColor);
            //Stomach
            organicLabelFarIndicationLinePointList[6] = new VertexPositionColor(
                new Vector3(-6, -3, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[7] = new VertexPositionColor(
                new Vector3(25, -3, 0), organicLayerColor);
            //Ascending colon
            organicLabelFarIndicationLinePointList[8] = new VertexPositionColor(
                new Vector3(-10, -2, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[9] = new VertexPositionColor(
                new Vector3(-48, 5, 0), organicLayerColor);
            //Descending colon
            organicLabelFarIndicationLinePointList[10] = new VertexPositionColor(
                new Vector3(11, -4, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[11] = new VertexPositionColor(
                new Vector3(30, 9, 0), organicLayerColor);
            //Rectum & anus
            organicLabelFarIndicationLinePointList[12] = new VertexPositionColor(
                new Vector3(0, 23, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[13] = new VertexPositionColor(
                new Vector3(40, 25, 0), organicLayerColor);
            //Back Kidney
            organicLabelFarIndicationLinePointList[14] = new VertexPositionColor(
                new Vector3(5, -2, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[15] = new VertexPositionColor(
                new Vector3(40, 2, 0), organicLayerColor);
            //Back Bladder
            organicLabelFarIndicationLinePointList[16] = new VertexPositionColor(
                new Vector3(0, 16, 0), organicLayerColor);
            organicLabelFarIndicationLinePointList[17] = new VertexPositionColor(
                new Vector3(-32, 15, 0), organicLayerColor);
            organicLabelFarIndicationLineVertexDeclaration = VertexPositionColorTexture.VertexDeclaration;
            // Initialize the vertex buffer, allocating memory for each vertex.
            organicLabelFarIndicationLineVertexDeclarationVertexBuffer = new VertexBuffer(graphics.GraphicsDevice, organicLabelFarIndicationLineVertexDeclaration,
                18, BufferUsage.None);
            // Set the vertex buffer data to the array of vertices.
            organicLabelFarIndicationLineVertexDeclarationVertexBuffer.SetData<VertexPositionColor>(organicLabelFarIndicationLinePointList);
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

            //load anatomy model
            anatomy = Content.Load<Model>("Anatomy/Full_NoReproductive");

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

            calibrationTexture = Content.Load<Texture2D>("Grid");
            cursorTexture = Content.Load<Texture2D>("CursorAlpha");

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            //make sure the render target has a depth buffer
            viewport = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

            //setup initial opacities
            if (!pointing)
            {
                SetModelTags(1.0f);
            }
            else
            {
                SetModelTags(pointingOpacity);
            }

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

        public void SetModelTags(float alpha)
        {
            foreach (ModelMesh mesh in anatomy.Meshes)
            {
                mesh.Tag = alpha;
            }
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;

            //Console.WriteLine("Got a skeleton");
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    Vector headPosTemp = data.Joints[JointID.Head].Position;
                    //Console.WriteLine(skeletonFrame.FloorClipPlane.W);
                    //Console.WriteLine("X: " + skeletonFrame.FloorClipPlane.X + " Y: " + skeletonFrame.FloorClipPlane.Y + " Z: " + skeletonFrame.FloorClipPlane.Z + " W: " + skeletonFrame.FloorClipPlane.W);
                    float floorY = (((skeletonFrame.FloorClipPlane.X * headPosTemp.X) + (skeletonFrame.FloorClipPlane.Z * headPosTemp.Z) + skeletonFrame.FloorClipPlane.W) / (-skeletonFrame.FloorClipPlane.Y));

                    float headToFloor = (floorPlane.X * headPosTemp.X) + (floorPlane.Y * headPosTemp.Y) + (floorPlane.Z * headPosTemp.Z);

                    


                    /*float a = Math.Abs(headPosTemp.Y * (float)Math.Tan(MathHelper.ToRadians(90.0f - (float)nui.NuiCamera.ElevationAngle)));

                    float b = Math.Abs(headPosTemp.Z - a);

                    float c = Math.Abs(b * (float)Math.Sin(MathHelper.ToRadians(90 - (float)nui.NuiCamera.ElevationAngle)));

                    float d = Math.Abs(headPos.Y / (float)Math.Cos(MathHelper.ToRadians(90.0f - (float)nui.NuiCamera.ElevationAngle)));*/

                    //Console.WriteLine("TEST " + headToFloor);

                    //convert to cm and translate to ensure correct coordinate system
                    currHeadPos.X = headPosTemp.X * 100.0f;
                    currHeadPos.Y = headPosTemp.Y * 100.0f;
                    currHeadPos.Z = headPosTemp.Z * 100.0f;


                    //set proximity distance, needs to be changed to proper proxemics
                    //userProximity = currHeadPos.Z * 2.0f;

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

                    currHandPos.X = handRight.X * 100f;
                    currHandPos.Y = handRight.Y * 100f;
                    currHandPos.Z = handRight.Z * 100f;

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

                    currLeftHandPos.X = handLeft.X * 100f;
                    currLeftHandPos.Y = handLeft.Y * 100f;
                    currLeftHandPos.Z = handLeft.Z * 100f;

                    currLeftHandXAngle = getAngleTrig(handLeft.X, handLeft.Z);
                    currLeftHandYAngle = getAngleTrig(handLeft.Y, handLeft.Z);

                    //setup slicing (currently arbitrary for testing)

                    if (slicing)
                        cutPlaneDistance = (-(currHeadPos.Z / 6.0f) + 210.0f) * (Zoom / initialZoom);

                    if (gestures)
                    {
                        if (MathHelper.ToDegrees(angleY) > 140.0f && MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f &&
                            MathHelper.ToDegrees(angleLeftY) > 140.0f && MathHelper.ToDegrees(angleLeftX) > 30.0f && MathHelper.ToDegrees(angleLeftX) < 150.0f)
                        {
                            float currHandDistance = Vector2.Distance(new Vector2(currHandPos.X, currHandPos.Y), new Vector2(currLeftHandPos.X, currLeftHandPos.Y));
                            float prevHandDistance = Vector2.Distance(new Vector2(prevHandPos.X, prevHandPos.Y), new Vector2(prevLeftHandPos.X, prevLeftHandPos.Y));
                            if (currHandDistance > prevHandDistance)
                            {
                                Zoom -= (currHandDistance - prevHandDistance) * 2;
                            }
                            else if (currHandDistance < prevHandDistance)
                            {
                                Zoom -= (currHandDistance - prevHandDistance) * 2;
                            }
                        }
                        else
                        {
                            //Rotate
                            if (MathHelper.ToDegrees(angleY) > 140.0f)
                            {
                                if (MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f)
                                {
                                    RotationYModel -= (prevHandYAngle - currHandYAngle) * 5;
                                    //Peng after talk we suppress x rotation 
                                    //RotationXModel += (prevHandXAngle - currHandXAngle) * 5;
                                }
                                peelMoveThreshold = 0;
                            }

                            //Peel
                            if (MathHelper.ToDegrees(angleLeftY) > 130.0f)
                            {
                                if (MathHelper.ToDegrees(angleLeftX) > 130.0
                               && peelMode == PeelMode.None
                               && activeLayerIndex < ModelLayer.Nervous
                               && (frameCounter > gestureGapDuration))
                                {
                                    Console.WriteLine("Start Peel");
                                    frameCounter = 0;
                                    peelMode = PeelMode.PeelingForward;
                                }

                                if (MathHelper.ToDegrees(angleLeftX) < 60.0f && MathHelper.ToDegrees(angleLeftX) > 40
                                    && peelMode == PeelMode.None
                                    && activeLayerIndex > ModelLayer.Skin
                                    && (frameCounter > gestureGapDuration))
                                {
                                    Console.WriteLine("Start Reverse Peel");
                                    frameCounter = 0;
                                    peelMode = PeelMode.PeelingBackward;
                                    activeLayerIndex--;
                                }


                                if (peelMode == PeelMode.PeelingForward)
                                {
                                    Console.WriteLine("Frame: " + frameCounter);
                                    if (!bPeelValid && MathHelper.ToDegrees(angleLeftX) < 130.0f)
                                    {
                                        bPeelValid = true;
                                        //peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                        layerOpacities[(int)activeLayerIndex] = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                    }
                                    else if (MathHelper.ToDegrees(angleLeftX) < 60.0f)
                                    {
                                        Console.WriteLine("Finish Peel");
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;

                                        activeLayerIndex++;
                                    }
                                    else if (bPeelValid && MathHelper.ToDegrees(angleLeftX) > 130.0) //user roll back the peel operation
                                    {
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;
                                    }
                                    else if (frameCounter < frameLimit)
                                    {
                                        //peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                        layerOpacities[(int)activeLayerIndex] = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                        //layerOpacities[activeLayerIndex] -= 0.01f;
                                        //Console.WriteLine("Opacity " + activeLayerIndex + ": " + layerOpacities[activeLayerIndex]);
                                    }
                                    else
                                    {
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;
                                    }
                                }
                                if (peelMode == PeelMode.PeelingBackward)
                                {
                                    if (!bPeelValid && MathHelper.ToDegrees(angleLeftX) > 60.0f)
                                    {
                                        bPeelValid = true;
                                        //peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                        layerOpacities[(int)activeLayerIndex] = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                    }
                                    else if (MathHelper.ToDegrees(angleLeftX) > 130.0f)
                                    {
                                        Console.WriteLine("Finish Reverse Peel");
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;

                                        //activeLayerIndex++;
                                        layerOpacities[(int)activeLayerIndex] = 1.0f;
                                    }
                                    else if (bPeelValid && MathHelper.ToDegrees(angleLeftX) < 60.0f) //user roll back the peel operation
                                    {
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;

                                        activeLayerIndex++;
                                    }
                                    else if (frameCounter < frameLimit)
                                    {
                                        //peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                        layerOpacities[(int)activeLayerIndex] = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                    }
                                    else
                                    {
                                        peelMode = PeelMode.None;
                                        frameCounter = 0;
                                        bPeelValid = false;
                                        //layerOpacities[activeLayerIndex] = 1.0f;
                                    }
                                }
                            }

                        }
                    }


                    prevHandPos = currHandPos;
                    prevHandYAngle = currHandYAngle;
                    prevHandXAngle = currHandXAngle;

                    prevLeftHandPos = currLeftHandPos;
                    prevLeftHandXAngle = currLeftHandXAngle;
                    prevLeftHandYAngle = currLeftHandYAngle;

                    //currHeadPos = Vector3.Transform(currHeadPos, Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation2)) * Matrix.CreateTranslation(cameraTranslation2));

                    //Console.WriteLine("Hand Y Angle: " + MathHelper.ToDegrees(angleY) + " X Angle: " + MathHelper.ToDegrees(angleX));
                    //Console.WriteLine("Head Pos: " + currHeadPos.X + " " + currHeadPos.Y + " " + currHeadPos.Z);

                    //if (currHeadPos.Z < 330)
                    //{
                        headPos = Vector3.Transform(currHeadPos, Matrix.CreateTranslation(cameraTranslation) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraYRotation)));
                    //}
                    //headPos.Y = headToFloor + 0.38f;

                    headPos.Y = headToFloor * 100.0f;

                    Viewport cameraView = GraphicsDevice.Viewport;
                       
                    Vector3 handTemp = new Vector3(-handRight.X * 1000.0f, handRight.Y * 1000.0f - 150.0f, handRight.Z * 100.0f + initialZoom);
 
                    handTemp = cameraView.Project(handTemp, kinectProjection, kinectView, Matrix.Identity);

                    screenPos = handTemp;

                    Console.WriteLine(screenPos.Y);

                    rayPosition = handTemp;
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
            Console.WriteLine("\nSpeech Recognized: \t{0} {1}", e.Result.Text, e.Result.Confidence);

            if (e.Result.Confidence > 0.90f)
            {
                if (e.Result.Text == "Show me the skin")
                {
                    //Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    Console.WriteLine("Set to skin");
                    layerOpacities[0] = 1.0f;
                    layerOpacities[1] = 1.0f;
                    layerOpacities[2] = 1.0f;
                    layerOpacities[3] = 1.0f;
                    layerOpacities[4] = 1.0f;
                    layerOpacities[5] = 1.0f;
                }
                else if (e.Result.Text == "Show me the muscles")
                {
                    Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    layerOpacities[1] = 1.0f;
                }
                else if (e.Result.Text == "Show me the skeleton")
                {
                    Console.WriteLine("Set to skeleton");
                    Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    layerOpacities[2] = 1.0f;
                }
                else if (e.Result.Text == "Show me the organs")
                {
                    Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    layerOpacities[3] = 1.0f;
                }
                else if (e.Result.Text == "Show me the circulatory system")
                {
                    Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    layerOpacities[4] = 1.0f;
                }
                else if (e.Result.Text == "Show me the nervous system")
                {
                    Array.Clear(layerOpacities, 0, layerOpacities.Length);
                    layerOpacities[5] = 1.0f;
                }
                else if (e.Result.Text == "Start Pointing")
                {
                    pointing = true;
                    gestures = false;
                    slicing = false;
                    labels = false;
                    layerOpacities[0] = 0.0f;
                    layerOpacities[1] = 0.0f;
                }
                else if (e.Result.Text == "Stop Pointing")
                {
                    pointing = false;
                    gestures = true;
                    frameCounter = 0;
                    layerOpacities[0] = 1.0f;
                    layerOpacities[1] = 1.0f;
                }
                else if (e.Result.Text == "Start Slicing")
                {
                    slicing = true;
                }
                else if (e.Result.Text == "Stop Slicing")
                {
                    slicing = false;
                    cutPlaneDistance = 1.0f;
                }
                else if ((e.Result.Text == "Show labels"))
                {
                    Console.WriteLine("Show labels");
                    labels = true;
                }
                else if (e.Result.Text == "Stop showing labels")
                    {
                    labels = false;
                }
                else if ((e.Result.Text == "Focus") && pointing)
                {
                    focusMesh();
                }
                else if ((e.Result.Text == "Go Back") && focused)
                {
                    unFocusMesh();
                }
                else if ((e.Result.Text == "Show me the heart"))
                {
                    intersected = anatomy.Meshes[177];
                    focusMesh();
                }
                else if ((e.Result.Text == "Show me the brain"))
                {
                    intersected = anatomy.Meshes[182];
                    focusMesh();
                }
                else if ((e.Result.Text == "Show me the skull"))
                {
                    intersected = anatomy.Meshes[150];
                    focusMesh();
                }
                else if ((e.Result.Text == "Show me the lungs"))
                {
                    intersected = anatomy.Meshes[174];
                    focusMesh();
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

           
            /*float RotationYWorldLocal = getAngleTrig(headPos.Z, headPos.X) - MathHelper.ToRadians(rotationYWorldOffset);
            float RotationXWorldLocal = -getAngleTrig(headPos.Z, headPos.Y) - MathHelper.ToRadians(14.0f);

            float RotationYWorldRemote1 = angleYRemote1 - MathHelper.ToRadians(rotationYWorldOffset);

            float test = MathHelper.ToDegrees(RotationYWorldRemote1);

            //Console.WriteLine("Local " + MathHelper.ToDegrees(RotationYWorldLocal));
            Console.WriteLine("Remote " + MathHelper.ToDegrees(RotationYWorldRemote1));
            if (MathHelper.ToDegrees(RotationYWorldLocal) > 336 && MathHelper.ToDegrees(RotationYWorldLocal) < 20)
            {
                RotationYWorld = RotationYWorldLocal;
                //RotationXWorld = RotationXWorldLocal;
            }

            
            else if (RotationYWorldRemote1 > 305)
            {
                //RotationYWorld = angleYRemote1 - MathHelper.ToRadians(rotationYWorldOffset);
                RotationYWorld = angleYRemote1 - MathHelper.ToRadians(rotationYWorldOffset);
                //RotationXWorld = angleXRemote1 - MathHelper.ToRadians(14.0f);
            }

            else
            {
                Console.WriteLine("Got Here");
                RotationYWorld = angleYRemote2 - MathHelper.ToRadians(rotationYWorldOffset);
            }*/
            

            gameWorldRotation =
                    Matrix.CreateRotationX(0) *
                    Matrix.CreateRotationY(0);

            gameWorldRotationViewport =
               Matrix.CreateRotationZ(RotationYWorld + MathHelper.ToRadians(shiftAngleLocal));



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
            /*if (state.IsKeyDown(Keys.Left))
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
            }*/
            if (state.IsKeyDown(Keys.A))
            {
                cutPlaneDistance -= 0.5f;
            }
            if (state.IsKeyDown(Keys.S))
            {
                cutPlaneDistance += 0.5f;
            }

            if (state.IsKeyDown(Keys.J))
            {
                pointingLabelPos.X -= 0.05f;
            }
            if (state.IsKeyDown(Keys.L))
            {
                pointingLabelPos.X += 0.05f;
            }
            if (state.IsKeyDown(Keys.I))
            {
                pointingLabelPos.Y += 0.05f;
            }
            if (state.IsKeyDown(Keys.K))
            {
                pointingLabelPos.Y -= 0.05f;
            }

            if (state.IsKeyDown(Keys.Left))
            {
                //shiftAngleLocal -= 0.5f;
                //shiftAngleRemote1 -= 0.5f;
                //shiftAngleRemote2 -= 0.5f;
                RotationYWorld -= MathHelper.ToRadians(0.075f);
            }
            if (state.IsKeyDown(Keys.Right))
            {
                //shiftAngleLocal += 0.5f;
                //shiftAngleRemote1 += 0.5f;
                //shiftAngleRemote2 += 0.5f;
                RotationYWorld += MathHelper.ToRadians(0.075f);
            }
            if (state.IsKeyDown(Keys.Down))
            {
                //rotationYWorldOffset -= 0.5f;
                RotationXWorld -= MathHelper.ToRadians(0.075f);
            }
            if (state.IsKeyDown(Keys.Up))
            {
                //rotationYWorldOffset += 0.5f;
                RotationXWorld += MathHelper.ToRadians(0.075f);
            }


            /*if (state.IsKeyDown(Keys.A) && prevState.IsKeyDown(Keys.A))
            {
                slicing = true;
                gestures = false;
                pointing = false;
                SetModelTags(1.0f);
                cutPlaneDistance = 1.0f;
            }

            if (state.IsKeyDown(Keys.S) && prevState.IsKeyDown(Keys.S))
            {
                slicing = false;
                gestures = true;
                pointing = false;

                cutPlaneDistance = 1.0f;
            }

            if (state.IsKeyDown(Keys.D) && prevState.IsKeyDown(Keys.D))
            {
                slicing = false;
                gestures = false;
                pointing = true;

                cutPlaneDistance = 1.0f;
                SetModelTags(pointingOpacity);
                layerOpacities[0] = 0.0f;
                layerOpacities[1] = 0.0f;

            }*/
            if (state.IsKeyDown(Keys.F5))
            {
                brightness -= 0.01f;
            }
            if (state.IsKeyDown(Keys.F6))
            {
                brightness += 0.01f;
            }
            if (state.IsKeyDown(Keys.Z))
            {
                distortionShift -= 0.001f;
            }
            if (state.IsKeyDown(Keys.X))
            {
                distortionShift += 0.001f;
            }

            if (state.IsKeyDown(Keys.N))
            {
                Zoom -= 0.5f;
                Console.WriteLine("Zoom " + Zoom);
            }
            if (state.IsKeyDown(Keys.M))
            {
                Zoom += 0.5f;
                Console.WriteLine("Zoom " + Zoom);
            }

            if (state.IsKeyDown(Keys.Q))
            {
                focusMesh();

                focused = true;
                pointing = false;
            }

            if (state.IsKeyDown(Keys.W))
            {
                unFocusMesh();
                focused = false;
                pointing = true;
            }

            if (focused)
            {
                if (Zoom > newZoom)
                {
                    Zoom -= 4.5f;
                }
                else
                {
                    Zoom = newZoom;
                }
                if (cameraY < cameraNewY && (cameraY - cameraNewY) > 1.0f)
                {
                    cameraY += 1.5f;
                    cameraLookAt.Y += 1.5f;
                }
                else if ((cameraY > cameraNewY) && (cameraY - cameraNewY) > 1.0f)
                {
                    cameraY -= 1.5f;
                    cameraLookAt.Y -= 1.5f;
                }
                else
                {
                    cameraY = cameraNewY;
                    cameraLookAt.Y = cameraNewY;
                }

            }

            if (unfocusing)
            {
                if (Zoom < initialZoom)
                {
                    Zoom += 4.5f;
                }
                else
                {
                    Zoom = initialZoom;
                }
                if (cameraY < cameraNewY && (cameraY - cameraNewY) > 1.0f)
                {
                    cameraY += 1.5f;
                    cameraLookAt.Y += 1.5f;
                }
                else if ((cameraY > cameraNewY) && (cameraY - cameraNewY) > 1.0f)
                {
                    cameraY -= 1.5f;
                    cameraLookAt.Y -= 1.5f;
                }
                else if ((cameraY - cameraNewY) < 1.0f && (Zoom == initialZoom))
                {
                    unfocusing = false;
                    cameraY = cameraNewY;
                    cameraLookAt.Y = cameraNewY;
                }

            }
            
               
            prevState = state;

            distortion = new Distortion(Vector3.Backward, 1, 1, distortionShift, 3, 1.33f);

            cameraPos = new Vector3(0.0f, cameraY, Zoom);

            UpdateRotations();

            base.Update(gameTime);
        }

        private void focusMesh()
        {
            Matrix[] transforms = new Matrix[anatomy.Bones.Count];

            anatomy.CopyAbsoluteBoneTransformsTo(transforms);

            BoundingSphere bounds = intersected.BoundingSphere.Transform(gameWorldRotation *
                        transforms[intersected.ParentBone.Index] *
                        Matrix.CreateScale(modelScale));

            SetModelTags(0.0f);

            Console.WriteLine("Bounding Sphere " + bounds.Center.Y);
            //anatomy.Meshes[164].Tag = 1.0f;
            intersected.Tag = 1.0f;

            cameraNewY = bounds.Center.Y - 5.0f;
            //newZoom = 2.67f * bounds.Radius;
            newZoom = 5.67f * bounds.Radius;

            focused = true;
            wasPointing = pointing;
            pointing = false;
            //gestures = false;
        }

        private void unFocusMesh()
        {
            focused = false;
            unfocusing = true;
            pointing = wasPointing;

            cameraNewY = 0.0f;
        }

        #region LabelInformation

        /// <summary>
        /// Renders the label around 3D model
        /// </summary>
        private void DrawLabel()
        {
            //View Matrix
            /*Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            camPos = Vector3.Transform(camPos, pitch);
            camPos = Vector3.Transform(camPos, yaw);

            Matrix view = Matrix.CreateLookAt(camPos, Vector3.Zero, Vector3.Up);*/

            //Projection Matrix
            //float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            Matrix projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 0.1f, 10000.0f);

            labelEffect.TextureEnabled = true;
            labelEffect.VertexColorEnabled = true;
            labelEffect.View = camView;
            labelEffect.Projection = camProjection;
            labelEffect.World = Matrix.CreateScale(1, -1, 1);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, labelEffect);
            //spriteBatch.DrawString(labelFont, "" + glNearPoint/*peelMoveThreshold*/, new Vector2(-52, -70), Color.Red, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            //spriteBatch.End();

            switch (activeLayerIndex)
            {
                case ModelLayer.Skin:
                    DrawSkinLayerLabel(labelEffect);
                    break;
                case ModelLayer.Muscle:
                    DrawMuscularLayerLabel(labelEffect);
                    break;
                case ModelLayer.Organic:
                    DrawOrganicLayerLabel(labelEffect);
                    break;
                case ModelLayer.Skeleton:
                    DrawSkeletonLayerLabel(labelEffect);
                    break;
            }

            //Restore device status
            //GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

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
            if (userProximity >= 375.0f)
                DrawSkinLayerLabelFar(effect, 1.0f);
            else if (userProximity >= 325.0f && userProximity < 375.0f)
            {
                DrawSkinLayerLabelFar(effect, (userProximity - 325.0f) / 50.0f);
                DrawSkinLayerLabelMed(effect, (375.0f - userProximity) / 50.0f);
            }
            else if (userProximity >= 285.0f && userProximity < 325.0f)
            {
                DrawSkinLayerLabelMed(effect, 1.0f);
            }
            else if (userProximity >= 235.0f && userProximity < 285.0f)
            {
                DrawSkinLayerLabelMed(effect, (userProximity - 235.0f) / 50.0f);
                DrawSkinLayerLabelNear(effect, (285.0f - userProximity) / 50.0f);
            }
            else
            {
                DrawSkinLayerLabelNear(effect, 1.0f);
            }
            //        {
            //            labelIndicationLineEffect.View = effect.View;
            //            labelIndicationLineEffect.Projection = effect.Projection;
            //            labelIndicationLineEffect.World = effect.World;
            //            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();
            //            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
            //    PrimitiveType.LineStrip,
            //    labelPointList,
            //                //0,   // vertex buffer offset to add to each element of the index buffer
            //                //2,   // number of vertices to draw
            //                //lineStripeIndices,
            //    0,   // first index element to read
            //    1    // number of primitives to draw
            //);
            //        }
        }

        private void DrawSkinLayerLabelFar(BasicEffect effect, float alpha)
        {
            //Label position
            Vector3[] skinFrontLabelPosition = { new Vector3(17, 68, 0),  //head
                                              new Vector3(-40, 60, 0),  //neck
                                              new Vector3(-75, -1, 0),  //elbow
                                              new Vector3(-75, -30, 0), //hand
                                              new Vector3(40, 12, 0), //Thorax
                                              new Vector3(37, -8, 0), //Abdomen
                                              new Vector3(-55, -63, 0), //knee
                                              new Vector3(30, -106, 0) //Foot
                                          };
            Vector3[] skinBackLabelPosition = { new Vector3(17, 68, 0),  //head
                                               new Vector3(-65, 10, 0),  //spine
                                               new Vector3(40, -16, 0)  //hip
                                              };

            float alphaIn = alpha;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 8; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextFar[i], new Vector2(skinFrontLabelPosition[i].X, -skinFrontLabelPosition[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 3; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinBackLabelTextFar[i], new Vector2(skinBackLabelPosition[i].X, -skinBackLabelPosition[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();


            labelIndicationLineEffect.View = effect.View;
            labelIndicationLineEffect.Projection = effect.Projection;
            labelIndicationLineEffect.World = effect.World;
            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();

            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);
            camPos = Vector3.Transform(camPos, yaw);
            if (camPos.Z >= 0) //Show front label indication line
            {
                for (int i = 0; i < 16; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
    PrimitiveType.LineStrip,
    skinLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
    i,   // first index element to read
    1    // number of primitives to draw
    );
                }
            }
            else if (camPos.Z < 0) //Show back label indication line
            {
                for (int i = 16; i < 22; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
PrimitiveType.LineStrip,
skinLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
i,   // first index element to read
1    // number of primitives to draw
);
                }
            }


        }

        private void DrawSkinLayerLabelMed(BasicEffect effect, float alpha)
        {
            //Draw far distance label(some may offset a bit to make place to new added label)

            //Label position
            Vector3[] skinFrontLabelPositionFar = { new Vector3(45, 68, 0),  //head
                                              new Vector3(-40, 60, 0),  //neck
                                              new Vector3(-75, -1, 0),  //elbow
                                              new Vector3(-75, -30, 0), //hand
                                              new Vector3(65, 12, 0), //Thorax
                                              new Vector3(62, -8, 0), //Abdomen
                                              new Vector3(-55, -63, 0), //knee
                                              new Vector3(30, -106, 0) //Foot
                                          };
            Vector3[] skinBackLabelPositionFar = { new Vector3(17, 68, 0),  //head
                                               new Vector3(-65, 10, 0),  //spine
                                               new Vector3(40, -16, 0)  //hip
                                              };

            float alphaIn = alpha;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 8; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextFar[i], new Vector2(skinFrontLabelPositionFar[i].X, -skinFrontLabelPositionFar[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 3; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinBackLabelTextFar[i], new Vector2(skinBackLabelPositionFar[i].X, -skinBackLabelPositionFar[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();


            //Draw medium distance label
            //Label text

            //Label position
            Vector3[] skinFrontLabelPositionMed = { new Vector3(14, 68, 0),  //Temple
                                              new Vector3(-27, 67, 0),  //Ear
                                              new Vector3(14, 60, 0), //Eye
                                              new Vector3(14, 78, 0), //Forehead
                                              new Vector3(25, 47, 0), //Shoulder
                                              new Vector3(40, 16, 0), //Breast
                                              new Vector3(40, 6, 0), //Nipple
                                              new Vector3(42, -16, 0), //Navel
                                              new Vector3(-55, -35, 0) //Groin
                                          };
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 9; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextMed[i], new Vector2(skinFrontLabelPositionMed[i].X, -skinFrontLabelPositionMed[i].Y), skinLabelColorMed * alphaIn, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }
            spriteBatch.End();


            labelIndicationLineEffect.View = effect.View;
            labelIndicationLineEffect.Projection = effect.Projection;
            labelIndicationLineEffect.World = effect.World;
            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();
            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);
            camPos = Vector3.Transform(camPos, yaw);
            if (camPos.Z >= 0) //Show front label indication line
            {
                for (int i = 0; i < 16; i += 2)
                {
                    if (i != 0 && i != 8 && i != 10)
                    {
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.LineStrip,
                            skinLabelFarIndicationLinePointList,
                            //0,   // vertex buffer offset to add to each element of the index buffer
                            //2,   // number of vertices to draw
                            //lineStripeIndices,
                            i,   // first index element to read
                            1    // number of primitives to draw
                            );
                    }
                }

                for (int i = 0; i < 18; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        skinLabelMedIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
            else if (camPos.Z < 0) //Show back label indication line
            {
                for (int i = 16; i < 22; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        skinLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
        }

        private void DrawSkinLayerLabelNear(BasicEffect effect, float alpha)
        {
            //Draw far distance label(some may offset a bit to make place to new added label)

            //Label position
            Vector3[] skinFrontLabelPositionFar = { new Vector3(55, 68, 0),  //head
                                              new Vector3(-40, 60, 0),  //neck
                                              new Vector3(-75, -1, 0),  //elbow
                                              new Vector3(-75, -30, 0), //hand
                                              new Vector3(70, 12, 0), //Thorax
                                              new Vector3(67, -8, 0), //Abdomen
                                              new Vector3(-55, -63, 0), //knee
                                              new Vector3(45, -106, 0) //Foot
                                          };
            Vector3[] skinBackLabelPositionFar = { new Vector3(17, 68, 0),  //head
                                               new Vector3(-65, 10, 0),  //spine
                                               new Vector3(40, -16, 0)  //hip
                                              };

            float alphaIn = alpha;
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 8; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextFar[i], new Vector2(skinFrontLabelPositionFar[i].X, -skinFrontLabelPositionFar[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 3; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinBackLabelTextFar[i], new Vector2(skinBackLabelPositionFar[i].X, -skinBackLabelPositionFar[i].Y), skinLabelColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();


            //Draw medium distance label
            //Label text

            //Label position
            Vector3[] skinFrontLabelPositionMed = { new Vector3(32, 73, 0),  //Temple
                                              new Vector3(-27, 67, 0),  //Ear
                                              new Vector3(32, 63, 0), //Eye
                                              new Vector3(32, 83, 0), //Forehead
                                              new Vector3(25, 47, 0), //Shoulder
                                              new Vector3(40, 16, 0), //Breast
                                              new Vector3(40, 6, 0), //Nipple
                                              new Vector3(42, -16, 0), //Navel
                                              new Vector3(-55, -35, 0) //Groin
                                          };
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 9; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextMed[i], new Vector2(skinFrontLabelPositionMed[i].X, -skinFrontLabelPositionMed[i].Y), skinLabelColorMed * alphaIn, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }
            spriteBatch.End();

            //Draw near distance label
            //Label text

            //Label position
            Vector3[] skinFrontLabelPositionNear = { new Vector3(14, 68, 0),  //Eyelid
                                              new Vector3(14, 54, 0),  //Pupil
                                              new Vector3(14, 61, 0), //Iris
                                              new Vector3(14, 75, 0), //Sclera
                                              new Vector3(30, -92, 0), //Ankle
                                              new Vector3(30, -99, 0), //Instep
                                              new Vector3(30, -116, 0) //Toe
                                          };
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 7; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skinFrontLabelTextNear[i], new Vector2(skinFrontLabelPositionNear[i].X, -skinFrontLabelPositionNear[i].Y), skinLabelColorNear * alphaIn, 0, Vector2.Zero, 0.18f, SpriteEffects.None, 0);
            }
            spriteBatch.End();

            labelIndicationLineEffect.View = effect.View;
            labelIndicationLineEffect.Projection = effect.Projection;
            labelIndicationLineEffect.World = effect.World;
            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();
            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);
            camPos = Vector3.Transform(camPos, yaw);
            if (camPos.Z >= 0) //Show front label indication line
            {
                for (int i = 0; i < 16; i += 2)
                {
                    if (i != 0 && i != 8 && i != 10 && i != 14)
                    {
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.LineStrip,
                            skinLabelFarIndicationLinePointList,
                            //0,   // vertex buffer offset to add to each element of the index buffer
                            //2,   // number of vertices to draw
                            //lineStripeIndices,
                            i,   // first index element to read
                            1    // number of primitives to draw
                            );
                    }
                }

                for (int i = 0; i < 18; i += 2)
                {
                    if (i != 0 && i != 4 && i != 6)
                    {
                        GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.LineStrip,
                            skinLabelMedIndicationLinePointList,
                            //0,   // vertex buffer offset to add to each element of the index buffer
                            //2,   // number of vertices to draw
                            //lineStripeIndices,
                            i,   // first index element to read
                            1    // number of primitives to draw
                            );
                    }
                }

                for (int i = 0; i < 14; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        skinLabelNearIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
            else if (camPos.Z < 0) //Show back label indication line
            {
                for (int i = 16; i < 22; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        skinLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
        }

        private void DrawMuscularLayerLabel(BasicEffect effect)
        {
            //Label text
            String[] muscularFrontLabelText = { "Frontalis", "Zygomaticus", "Trapezius", "Deltoid", "Brachioradialis", 
                                                  "Pectoralis major", "Rectus abdominus", "Rectus femoris", "Fibularis longus", "Extensor digitorum longus" };
            String[] muscularBackLabelText = { "Sternocleidomastoid", "Latissimus dorsi", "Gluteus maximus", "Gastrocnemius" };

            //Label position
            Vector3[] muscularFrontLabelPosition = { new Vector3(12, 75, 0),  //Frontalis
                                              new Vector3(10, 55, 0),  //Zygomaticus
                                              new Vector3(-65, 50, 0),  //Trapezius
                                              new Vector3(26, 43, 0),  //Deltoid
                                              new Vector3(51, -2, 0),  //Brachioradialis
                                              new Vector3(55, 21, 0), //Pectoralis major
                                              new Vector3(-123, 5, 0), //Rectus abdominus
                                              new Vector3(-90, -36, 0), //Rectus femoris
                                              new Vector3(31, -70, 0), //Fibularis longus
                                              new Vector3(-150, -107, 0) //Extensor digitorum longus
                                          };
            Vector3[] muscularBackLabelPosition = { new Vector3(-108, 48, 0),  //Sternocleidomastoid
                                               new Vector3(40, 13, 0),  //Latissimus dorsi
                                               new Vector3(43, -15, 0), //Gluteus maximus
                                               new Vector3(33, -76, 0)  //Gastrocnemius
                                              };

            float alphaIn = 1;// ((userProximity - 350.0f) / 50.0f);
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
                spriteBatch.DrawString(labelFont, muscularFrontLabelText[i], new Vector2(muscularFrontLabelPosition[i].X, -muscularFrontLabelPosition[i].Y), muscularLayerColor * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 4; ++i)
            {
                //effect.World = Matrix.Identity;
                //effect.World = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(muscularBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, muscularBackLabelText[i], new Vector2(muscularBackLabelPosition[i].X, -muscularBackLabelPosition[i].Y), muscularLayerColor * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            labelIndicationLineEffect.View = effect.View;
            labelIndicationLineEffect.Projection = effect.Projection;
            labelIndicationLineEffect.World = effect.World;
            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();

            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);
            camPos = Vector3.Transform(camPos, yaw);
            if (camPos.Z >= 0) //Show front label indication line
            {
                for (int i = 0; i < 20; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        muscularLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
            else if (camPos.Z < 0) //Show back label indication line
            {
                for (int i = 20; i < 28; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        muscularLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
        }

        private void DrawOrganicLayerLabel(BasicEffect effect)
        {

            //Label text
            String[] organicFrontLabelText = { "Larynx", "Lung", "Liver", "Stomach", "Ascending colon", "Descending colon", "Rectum & anus" };
            String[] organicBackLabelText = { "Kidney", "Bladder" };
            //Label position
            Vector3[] organicFrontLabelPosition = { new Vector3(23, 48, 0),  //Larynx
                                              new Vector3(30, 21, 0),  //Lung
                                              new Vector3(-55, 10, 0),  //Liver
                                              new Vector3(35, 8, 0),  //Stomach
                                              new Vector3(-100, 0, 0),  //Ascending colon
                                              new Vector3(40, -4, 0), //Descending colon
                                              new Vector3(40, -20, 0), //Rectum & anus
                                          };
            Vector3[] organicBackLabelPosition = { new Vector3(40, 3, 0),  //Kidney
                                               new Vector3(-70, -10, 0),  //Bladder
                                              };

            float alphaIn = 1;// ((userProximity - 200.0f) / 50.0f);
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
                spriteBatch.DrawString(labelFont, organicFrontLabelText[i], new Vector2(organicFrontLabelPosition[i].X, -organicFrontLabelPosition[i].Y), organicLayerColor * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 2; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, organicBackLabelText[i], new Vector2(organicBackLabelPosition[i].X, -organicBackLabelPosition[i].Y), organicLayerColor * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();

            labelIndicationLineEffect.View = effect.View;
            labelIndicationLineEffect.Projection = effect.Projection;
            labelIndicationLineEffect.World = effect.World;
            labelIndicationLineEffect.Techniques[0].Passes[0].Apply();

            //View Matrix
            Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);
            camPos = Vector3.Transform(camPos, yaw);
            if (camPos.Z >= 0) //Show front label indication line
            {
                for (int i = 0; i < 14; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        organicLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
            else if (camPos.Z < 0) //Show back label indication line
            {
                for (int i = 14; i < 18; i += 2)
                {
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        organicLabelFarIndicationLinePointList,
                        //0,   // vertex buffer offset to add to each element of the index buffer
                        //2,   // number of vertices to draw
                        //lineStripeIndices,
                        i,   // first index element to read
                        1    // number of primitives to draw
                        );
                }
            }
        }

        private void DrawSkeletonLayerLabel(BasicEffect effect)
        {
            if (userProximity >= 375.0f)
                DrawSkeletonLayerLabelFar(effect, 1.0f);
            else if (userProximity >= 325.0f && userProximity < 375.0f)
            {
                DrawSkeletonLayerLabelFar(effect, (userProximity - 325.0f) / 50.0f);
                DrawSkeletonLayerLabelMed(effect, (375.0f - userProximity) / 50.0f);
            }
            //else if (userProximity >= 285.0f && userProximity < 325.0f)
            else
            {
                DrawSkeletonLayerLabelMed(effect, 1.0f);
            }
        }

        private void DrawSkeletonLayerLabelFar(BasicEffect effect, float alpha)
        {
            //Draw far distance label
            //Label position
            Vector3[] skeletonFrontLabelPositionFar = { new Vector3(-50, 70, 0),  //Skull
                                              new Vector3(20, 56, 0),  //Clavicle
                                              new Vector3(-50, 15, 0),  //Rib
                                              new Vector3(38, 42, 0),  //Humerus
                                              new Vector3(30, 5, 0),  //Vertebrae
                                              new Vector3(-73, -10, 0), //Os coxa
                                              new Vector3(32, -40, 0), //Femur
                                              new Vector3(37, -80, 0), //Tibia
                                          };
            Vector3[] skeletonBackLabelPositionFar = { new Vector3(45, 54, 0),  //Scapula
                                               new Vector3(-80, -2, 0),  //Vertebrae
                                               new Vector3(30, -80, 0),  //Fibula
                                               new Vector3(-55, -100, 0),  //Tarsals
                                              };

            float alphaIn = alpha;// ((userProximity - 200.0f) / 50.0f);
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 8; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skeletonFrontLabelTextFar[i], new Vector2(skeletonFrontLabelPositionFar[i].X, -skeletonFrontLabelPositionFar[i].Y), skeletonLayerColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 4; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skeletonBackLabelTextFar[i], new Vector2(skeletonBackLabelPositionFar[i].X, -skeletonBackLabelPositionFar[i].Y), skeletonLayerColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();
        }

        private void DrawSkeletonLayerLabelMed(BasicEffect effect, float alpha)
        {
            //Draw far distance label("Rib" label offset for new added label)
            //Label position
            Vector3[] skeletonFrontLabelPositionFar = { new Vector3(-50, 70, 0),  //Skull
                                              new Vector3(20, 56, 0),  //Clavicle
                                              new Vector3(-80, 15, 0),  //Rib
                                              new Vector3(38, 42, 0),  //Humerus
                                              new Vector3(30, 5, 0),  //Vertebrae
                                              new Vector3(-68, -10, 0), //Os coxa
                                              new Vector3(32, -40, 0), //Femur
                                              new Vector3(37, -80, 0), //Tibia
                                          };
            Vector3[] skeletonBackLabelPositionFar = { new Vector3(45, 54, 0),  //Scapula
                                               new Vector3(-70, -2, 0),  //Vertebrae
                                               new Vector3(30, -80, 0),  //Fibula
                                               new Vector3(-55, -100, 0),  //Tarsals
                                              };

            float alphaIn = alpha;// ((userProximity - 200.0f) / 50.0f);
            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 8; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skeletonFrontLabelTextFar[i], new Vector2(skeletonFrontLabelPositionFar[i].X, -skeletonFrontLabelPositionFar[i].Y), skeletonLayerColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            //Back label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullClockwise, effect);
            for (int i = 0; i < 4; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinBackLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skeletonBackLabelTextFar[i], new Vector2(skeletonBackLabelPositionFar[i].X, -skeletonBackLabelPositionFar[i].Y), skeletonLayerColorFar * alphaIn, 0, Vector2.Zero, 0.3f, SpriteEffects.FlipHorizontally, 0);
            }
            spriteBatch.End();

            //Draw medium distance label
            //Label position
            Vector3[] skeletonFrontLabelPositionMed = {
                                              new Vector3(-65, 23, 0),  //Sternum
                                              new Vector3(-65, 15, 0),  //Floating rib
                                              new Vector3(-65, 7, 0),  //Spinal column
                                          };

            //Front label
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, effect);
            for (int i = 0; i < 3; ++i)
            {
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, skeletonFrontLabelTextMed[i], new Vector2(skeletonFrontLabelPositionMed[i].X, -skeletonFrontLabelPositionMed[i].Y), skeletonLayerColorMed * alphaIn, 0, Vector2.Zero, 0.20f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }

        #endregion LabelInformation

        /// <summary>
        /// Renders the 3D model
        /// </summary>
        private void DrawModel(float scale)
        {
            Matrix[] transforms = new Matrix[anatomy.Bones.Count];
            
            anatomy.CopyAbsoluteBoneTransformsTo(transforms);

                camProjection =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                    aspectRatio, cutPlaneDistance, 10000.0f);
            
            //just changed, was backwards
            //Vector3 camPos = new Vector3(0.0f, 0.0f, Zoom);
            Matrix pitch = Matrix.CreateRotationX(RotationXWorld + RotationXModel);
            Matrix yaw = Matrix.CreateRotationY(RotationYWorld + RotationYModel);

            cameraPos = Vector3.Transform(cameraPos, pitch);
            cameraPos = Vector3.Transform(cameraPos, yaw);

            //Vector3 camPos = new Vector3(camX, camYAngleX, camZAngleX);
            camView = Matrix.CreateLookAt(cameraPos, cameraLookAt, Vector3.Up);


            if (peelMode == PeelMode.PeelingForward)
            {
                //Draw each layer based on mesh indices
                //the order of drawing is important (DON'T CHANGE!)

                //nervous
                drawLayer(180, 184, layerOpacities[5], scale, transforms);
                //circulatory
                drawLayer(177, 179, layerOpacities[4], scale, transforms);
                //urinary
                drawLayer(176, 176, layerOpacities[3], scale, transforms);
                //digestive
                drawLayer(4, 4, layerOpacities[3], scale, transforms);
                //respiratory
                drawLayer(172, 175, layerOpacities[3], scale, transforms);
                //skeletal
                drawLayer(146, 171, layerOpacities[2], scale, transforms);
                //muscle
                drawLayer(5, 145, layerOpacities[1], scale, transforms);
                //skin
                drawLayer(1, 3, layerOpacities[0], scale, transforms);

                if (activeLayerIndex == ModelLayer.Skin)
                {
                    int peelStep = (int)(layerOpacities[(int)activeLayerIndex] * 100) / 5;
                    switch (peelStep)
                    {
                        case 30:
                        case 29:
                        case 28:
                        case 27:
                        case 26:
                        case 25:
                        case 24:
                        case 23:
                        case 22:
                        case 21:
                        case 20:
                            drawLayer(185, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 19:
                            drawLayer(186, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 18:
                            drawLayer(187, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 17:
                            drawLayer(188, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 16:
                            drawLayer(189, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 15:
                            drawLayer(190, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 14:
                            drawLayer(191, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 13:
                            drawLayer(192, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 12:
                            drawLayer(193, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 11:
                            drawLayer(194, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 10:
                            drawLayer(195, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 9:
                            drawLayer(196, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 8:
                            drawLayer(197, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 7:
                            drawLayer(198, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 6:
                            drawLayer(199, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 5:
                            drawLayer(200, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 4:
                            drawLayer(201, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 3:
                            drawLayer(202, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 2:
                            drawLayer(203, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 1:
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 0:
                            break;

                    }
                }
            }
            else if (peelMode == PeelMode.PeelingBackward)
            {
                //Draw each layer based on mesh indices
                //the order of drawing is important (DON'T CHANGE!)

                //nervous
                drawLayer(180, 184, layerOpacities[5], scale, transforms);
                //circulatory
                drawLayer(177, 179, layerOpacities[4], scale, transforms);
                //urinary
                drawLayer(176, 176, layerOpacities[3], scale, transforms);
                //digestive
                drawLayer(4, 4, layerOpacities[3], scale, transforms);
                //respiratory
                drawLayer(172, 175, layerOpacities[3], scale, transforms);
                //skeletal
                drawLayer(146, 171, layerOpacities[2], scale, transforms);
                //muscle
                drawLayer(5, 145, layerOpacities[1], scale, transforms);

                if (activeLayerIndex == ModelLayer.Skin)
                {
                    int peelStep = (int)(layerOpacities[(int)activeLayerIndex] * 100) / 5;
                    switch (peelStep)
                    {
                        case 30:
                        case 29:
                        case 28:
                        case 27:
                        case 26:
                        case 25:
                        case 24:
                        case 23:
                        case 22:
                        case 21:
                        case 20:
                            drawLayer(185, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 19:
                            drawLayer(186, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 18:
                            drawLayer(187, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 17:
                            drawLayer(188, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 16:
                            drawLayer(189, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 15:
                            drawLayer(190, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 14:
                            drawLayer(191, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 13:
                            drawLayer(192, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 12:
                            drawLayer(193, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 11:
                            drawLayer(194, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 10:
                            drawLayer(195, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 9:
                            drawLayer(196, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 8:
                            drawLayer(197, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 7:
                            drawLayer(198, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 6:
                            drawLayer(199, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 5:
                            drawLayer(200, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 4:
                            drawLayer(201, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 3:
                            drawLayer(202, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 2:
                            drawLayer(203, 203, 1.0f, scale, transforms);
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 1:
                            drawLayer(0, 0, 1.0f, scale, transforms);
                            break;
                        case 0:
                            break;

                    }
                }
                drawLayer(0, 3, (((int)activeLayerIndex * 100.0f - 350.0f) + 50 + peelMoveThreshold * 2) / 50.0f, scale, transforms);
            }
            else
            {
                //nervous
                drawLayer(180, 184, layerOpacities[5], scale, transforms);
                //circulatory
                drawLayer(177, 179, layerOpacities[4], scale, transforms);
                //urinary
                drawLayer(176, 176, layerOpacities[3], scale, transforms);
                //digestive
                drawLayer(4, 4, layerOpacities[3], scale, transforms);
                //respiratory
                drawLayer(172, 175, layerOpacities[3], scale, transforms);
                //skeletal
                drawLayer(146, 171, layerOpacities[2], scale, transforms);
                //muscle
                drawLayer(5, 145, layerOpacities[1], scale, transforms);
                //skin
                drawLayer(185, 203, layerOpacities[0], scale, transforms);
                drawLayer(0, 3, layerOpacities[0], scale, transforms);
            }
        }

        private void drawLayer(int startIndex, int endIndex, float alphaIn, float scale, Matrix[] transforms)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                foreach (BasicEffect effect in anatomy.Meshes[i].Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = camView;
                    effect.AmbientLightColor = new Vector3(brightness, brightness, brightness);
                    effect.Projection = camProjection;
                    effect.World = gameWorldRotation *
                        transforms[anatomy.Meshes[i].ParentBone.Index] *
                        Matrix.CreateScale(scale);

                    if (alphaIn == 0.0f)
                    {
                        anatomy.Meshes[i].Tag = 0.0f;
                    }
                    if (pointing || focused)
                    {
                        if (alphaIn != 0.0f)
                        {
                            effect.Alpha = (float)anatomy.Meshes[i].Tag;
                        }
                    }
                    else
                    {
                        if (alphaIn < 1.0f && alphaIn != 0.0f)
                        {
                            anatomy.Meshes[i].Tag = alphaIn;
                        }
                        else
                        {
                            anatomy.Meshes[i].Tag = 1.0f;
                        }
                        effect.Alpha = (float)anatomy.Meshes[i].Tag;
                        //debug with heart
                    }
                    if ((float)anatomy.Meshes[i].Tag > 0.0f && alphaIn != 0.0f)
                    {
                        anatomy.Meshes[i].Draw();
                    }
                    //effect.Alpha = alphaIn;
                    //modelIn.Meshes[i].Draw();
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
            sliceEffect.Texture = calibrationTexture;
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

        private void checkIntersection(Vector3 point)
        {

            BoundingSphere boundingSphere;
            Matrix[] transforms = new Matrix[anatomy.Bones.Count];

            BasicEffect pointingLabelEffect = new BasicEffect(this.GraphicsDevice);

            pointingLabelEffect.TextureEnabled = true;
            pointingLabelEffect.VertexColorEnabled = true;
            pointingLabelEffect.View = camView;
            pointingLabelEffect.Projection = camProjection;
            pointingLabelEffect.World = Matrix.CreateScale(1, -1, 1);

            //when using mouse
            /*MouseState ms = Mouse.GetState();
            Vector3 nearPoint = new Vector3(ms.X, ms.Y, 0);
            Vector3 farPoint = new Vector3(ms.X, ms.Y, 1);*/

            //when using kinect
            Vector3 nearPoint = new Vector3(point.X, point.Y, 0);
            Vector3 farPoint = new Vector3(point.X, point.Y, 1);

            //set cursor position
            cursorPosition.X = nearPoint.X;
            cursorPosition.Y = nearPoint.Y;


            Viewport viewport = GraphicsDevice.Viewport;
            nearPoint = viewport.Unproject(nearPoint, camProjection, camView, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, camProjection, camView, Matrix.Identity);

            Ray ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

            //Console.WriteLine("Ray X: " + ray.Direction.X + " Y: " + ray.Direction.Y + " Z: " + ray.Direction.Z);
            //Console.WriteLine("Screen X: " + screenPos.X + " Y: " + screenPos.Y + " Z: " + screenPos.Z);
            //DrawTestModel(lander, 10.0f, );

            anatomy.CopyAbsoluteBoneTransformsTo(transforms);

            bool intersect = false;
            bool roughIntersection = false;
            //ModelMesh intersected = null;

            /*foreach (ModelMesh mesh in anatomy.Meshes)
            {
           
                boundingSphere = mesh.BoundingSphere.Transform(gameWorldRotation *
                        transforms[mesh.ParentBone.Index] *
                        Matrix.CreateScale(modelScale));

                if (!intersect)
                {
                    if (ray.Intersects(boundingSphere) != null && (float)mesh.Tag != 0.0f)
                    {
                        roughIntersection = true;

                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            intersect = CheckIntersectionTriangles(ray, part, gameWorldRotation *
                                                        transforms[mesh.ParentBone.Index] *
                                                        Matrix.CreateScale(modelScale));

                            if (intersect)
                            {
                                intersected = mesh;
                            }
                        }
                    }
                }
            }*/
            intersected = anatomy.Meshes[4];
            roughIntersection = true;
            if (intersected != null && roughIntersection)
            {
                pointingOpacity = 0.2f;
                SetModelTags(pointingOpacity);
                intersected.Tag = 1.0f;

                anatomy.Meshes[182].Tag = 0.05f;
                anatomy.Meshes[150].Tag = 0.05f;

                String pointingLabel;
                if (intersected.Name == "DigestiveSystem")
                {
                    pointingLabel = "Digestive System";
                }
                else
                {
                    pointingLabel = intersected.Name.Replace('_', '\n');
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, pointingLabelEffect);
                //effect.World = Matrix.CreateScale(1, -1, 1);// *Matrix.CreateTranslation(skinFrontLabelPosition[i]);
                spriteBatch.DrawString(labelFont, pointingLabel, pointingLabelPos, Color.DarkGray, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
                //spriteBatch.DrawString(labelFont, pointingLabel, pointingLabelPos, Color.White, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
                spriteBatch.End();
            }
            else
            {
                pointingOpacity = 0.6f;
                SetModelTags(pointingOpacity);
                intersected = null;
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

            BoundingBox thresholdBox = BoundingBox.CreateFromPoints(allVertex);

            if (ray.Intersects(thresholdBox) == null)
            {

                return false;
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
            //Console.WriteLine("U: " + barycentricU);
            if (barycentricU < 0.0f || barycentricU > det)
                return false;

            // Prepare to test barycentricV parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate barycentricV parameter and test bounds
            barycentricV = Vector3.Dot(ray.Direction, qvec);
            //Console.WriteLine("V: " + barycentricV);
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

            Vector3 newCamPos = new Vector3(0.0f, 0.0f, initialZoom);
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
                distortionEffect.World = gameWorldRotationViewport;
                GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, distortion.Vertices, 0, 3999);
            }
        }


        private void DrawCalibrationPattern()
        {
            this.GraphicsDevice.SetRenderTarget(null);
            this.GraphicsDevice.Clear(Color.Black);
            this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            distortionEffect.Texture = calibrationTexture;

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
            frameCounter++;
            //this.GraphicsDevice.SetRenderTarget(null);
            this.GraphicsDevice.SetRenderTarget(viewport);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.GraphicsDevice.Clear(Color.Black);

            this.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            SamplerState currState = GraphicsDevice.SamplerStates[0];

            DrawModel(modelScale);

            if (labels)
                DrawLabel();

            //drawSlicingPlane(Zoom - cutPlaneDistance - 1.0f);
            //check for intersection

            if (pointing)
                checkIntersection(rayPosition);

            //draw cursor after model
            if (pointing)
            {
                //Console.WriteLine(Dr
                spriteBatch.Begin();
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                //spriteBatch.Draw(colorFeed, new Rectangle(0, 0, 160, 120), Color.White);
                spriteBatch.Draw(cursorTexture, new Rectangle((int)cursorPosition.X - 30, (int)cursorPosition.Y - 30, 60, 60), Color.White);
                spriteBatch.End();

                GraphicsDevice.SamplerStates[0] = currState;
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            }

            DrawForCylinder();

            //for testing projector alignment
            //DrawCalibrationPattern();

            base.Draw(gameTime);
        }
    }
}
