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

        private Vector3 prevHandPos;
        private Vector3 currHandPos;

        private Vector3 prevLeftHandPos;
        private Vector3 currLeftHandPos;

        //threshold for peel gesture
        private float peelMoveThreshold = 0;
        //this lock is to confirm that change one layer in one time guesture.
        private bool peelLock = false;

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

        //For peel animation
        enum PeelMode
        {
            None = 0,
            PeelingForward,
            PeelingBackward
        }
        PeelMode peelMode = PeelMode.None;

        //timer used for peeling
        public int frameCounter = 0;
        //limit for a gesture to be performed
        int frameLimit = 300;

        //duration before gestures are detected
        int gestureGapDuration = 50;

        bool bPeelValid = false;

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
            Stub = 0,
            Skeleton,
            Organic,
            Muscle,
            Skin
        }
        ModelLayer modelLayer;

        static Color Red =  new Color(204,0,51);
        static Color Yellow = new Color(255,204,0);
        static Color Purple = new Color(153,0,102);
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
        Color muscularLayerColor = new Color(153,204,0);
        Color organicLayerColor = new Color(255,153,51);

        //Skeleton label color
        Color skeletonLayerColorFar = new Color(153, 204, 204);
        Color skeletonLayerColorMed = new Color(255, 153, 51);
        Color skeletonLayerColorNear = new Color(255,255,0);
        //Skeleton label text
        String[] skeletonFrontLabelTextFar = { "Skull", "Clavicle", "Rib", "Humerus", "Vertebrae", "Os coxa", "Femur", "Tibia" };
        String[] skeletonBackLabelTextFar = { "Scapula", "Vertebrae", "Fibula", "Tarsals" };
        String[] skeletonFrontLabelTextMed = { "Sternum", "Floating rib", "Spinal column" };

        Vector3 glNearPoint;
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

            labelEffect = new BasicEffect(GraphicsDevice);

            modelLayer = ModelLayer.Skin;

            InitializeLabelIndicationLine();
            base.Initialize();
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
                    //Console.WriteLine(MathHelper.ToDegrees(angleX));

                    currLeftHandPos.X = handLeft.X * 100f;
                    currLeftHandPos.Y = handLeft.Y * 100f;
                    currLeftHandPos.Z = handLeft.Z * 100f;

                    currLeftHandXAngle = getAngleTrig(handLeft.X, handLeft.Z);
                    currLeftHandYAngle = getAngleTrig(handLeft.Y, handLeft.Z);

                    ////arbitrary thresholds
                    //if (MathHelper.ToDegrees(angleY) > 140.0f && MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f &&
                    //    MathHelper.ToDegrees(angleLeftY) > 140.0f && MathHelper.ToDegrees(angleLeftX) > 30.0f && MathHelper.ToDegrees(angleLeftX) < 150.0f)
                    //{
                    //    if (!peelLock)
                    //    {
                    //        if (currHandPos.X - prevHandPos.X > 0 && prevLeftHandPos.X - currLeftHandPos.X > 0)
                    //        {
                    //            if (peelMode == PeelMode.None)
                    //            {
                    //                peelMode = PeelMode.PeelOut;
                    //            }
                    //            else if (peelMode == PeelMode.PeelOut)
                    //            {
                    //                if (peelMoveThreshold < 25.0f)
                    //                {
                    //                    peelMoveThreshold += currHandPos.X - prevHandPos.X;
                    //                    if (peelMoveThreshold > 25.0f)
                    //                    {
                    //                        if (modelLayer > ModelLayer.Skeleton)
                    //                        {
                    //                            modelLayer--;

                    //                            //CutoffPlaneZ = 0.1f;
                    //                            peelMoveThreshold = 0.0f;
                    //                            peelLock = true;
                    //                            peelMode = PeelMode.None;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            else if (peelMode == PeelMode.PeelIn)
                    //            {
                    //                if (peelMoveThreshold > 0)
                    //                {
                    //                    peelMoveThreshold += prevHandPos.X - currHandPos.X;
                    //                    if (peelMoveThreshold <= 0)
                    //                    {
                    //                        if (modelLayer > ModelLayer.Skeleton)
                    //                        {
                    //                            modelLayer--;

                    //                            //CutoffPlaneZ = 0.1f;
                    //                            peelMoveThreshold = 0.0f;
                    //                            peelLock = true;
                    //                            peelMode = PeelMode.None;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        else if (currHandPos.X - prevHandPos.X < 0 && prevLeftHandPos.X - currLeftHandPos.X < 0)
                    //        {
                    //            if (peelMode == PeelMode.None)
                    //            {
                    //                peelMode = PeelMode.PeelIn;
                    //            }
                    //            else if (peelMode == PeelMode.PeelIn)
                    //            {
                    //                if (peelMoveThreshold < 25.0f)
                    //                {
                    //                    peelMoveThreshold += prevHandPos.X - currHandPos.X;
                    //                    if (peelMoveThreshold > 25.0f)
                    //                    {
                    //                        if (modelLayer < ModelLayer.Skin)
                    //                        {
                    //                            modelLayer++;

                    //                            //CutoffPlaneZ = 0.1f;
                    //                            peelMoveThreshold = 0.0f;
                    //                            peelLock = true;
                    //                            peelMode = PeelMode.None;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            else if (peelMode == PeelMode.PeelOut)
                    //            {
                    //                if (peelMoveThreshold > 0)
                    //                {
                    //                    peelMoveThreshold += currHandPos.X - prevHandPos.X;
                    //                    if (peelMoveThreshold <= 0)
                    //                    {
                    //                        if (modelLayer < ModelLayer.Skin)
                    //                        {
                    //                            modelLayer++;

                    //                            //CutoffPlaneZ = 0.1f;
                    //                            peelMoveThreshold = 0.0f;
                    //                            peelLock = true;
                    //                            peelMode = PeelMode.None;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    peelLock = false;
                    //    peelMode = PeelMode.None;

                    //    if (MathHelper.ToDegrees(angleY) > 140.0f)
                    //    {
                    //        if (MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f)
                    //        {
                    //            RotationYModel -= (prevHandYAngle - currHandYAngle) * 5;
                    //            //Peng after talk we suppress x rotation 
                    //            //RotationXModel += (prevHandXAngle - currHandXAngle) * 5;
                    //        }
                    //        peelMoveThreshold = 0;
                    //    }
                    //    if (MathHelper.ToDegrees(angleLeftY) > 140.0f)
                    //    {
                    //        if (MathHelper.ToDegrees(angleLeftX) > 30.0f && MathHelper.ToDegrees(angleLeftX) < 150.0f)
                    //        {
                    //            Zoom -= (prevLeftHandYAngle - currLeftHandYAngle) * 300;
                    //        }
                    //        peelMoveThreshold = 0;
                            
                    //    }
                    //}

                    if (MathHelper.ToDegrees(angleY) > 140.0f && MathHelper.ToDegrees(angleX) > 30.0f && MathHelper.ToDegrees(angleX) < 150.0f &&
                        MathHelper.ToDegrees(angleLeftY) > 140.0f && MathHelper.ToDegrees(angleLeftX) > 30.0f && MathHelper.ToDegrees(angleLeftX) < 150.0f)
                    {
                        float currHandDistance = Vector2.Distance(new Vector2(currHandPos.X, currHandPos.Y), new Vector2(currLeftHandPos.X, currLeftHandPos.Y));
                        float prevHandDistance = Vector2.Distance(new Vector2(prevHandPos.X, prevHandPos.Y), new Vector2(prevLeftHandPos.X, prevLeftHandPos.Y));
                        if ( currHandDistance > prevHandDistance )
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
                           && modelLayer > ModelLayer.Skeleton
                           && (frameCounter > gestureGapDuration))
                            {
                                Console.WriteLine("Start Peel");
                                frameCounter = 0;
                                peelMode = PeelMode.PeelingForward;
                            }

                            if (MathHelper.ToDegrees(angleLeftX) < 60.0f && MathHelper.ToDegrees(angleLeftX) > 40
                                && peelMode == PeelMode.None
                                && modelLayer < ModelLayer.Skin
                                && (frameCounter > gestureGapDuration))
                            {
                                Console.WriteLine("Start Reverse Peel");
                                frameCounter = 0;
                                peelMode = PeelMode.PeelingBackward;
                            }


                            if (peelMode == PeelMode.PeelingForward)
                            {
                                Console.WriteLine("Frame: " + frameCounter);
                                if (!bPeelValid && MathHelper.ToDegrees(angleLeftX) < 130.0f)
                                {
                                    bPeelValid = true;
                                    peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                }
                                else if (MathHelper.ToDegrees(angleLeftX) < 60.0f)
                                {
                                    Console.WriteLine("Finish Peel");
                                    peelMode = PeelMode.None;
                                    frameCounter = 0;
                                    bPeelValid = false;

                                    modelLayer--;
                                }
                                else if (bPeelValid && MathHelper.ToDegrees(angleLeftX) > 130.0)
                                {
                                    peelMode = PeelMode.None;
                                    frameCounter = 0;
                                    bPeelValid = false;
                                }
                                else if (frameCounter < frameLimit)
                                {
                                    peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                    //layerOpacities[activeLayerIndex] = (MathHelper.ToDegrees(angleX) - 60.0f) / 70.0f;
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
                                    peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                }
                                else if (MathHelper.ToDegrees(angleLeftX) > 130.0f)
                                {
                                    Console.WriteLine("Finish Reverse Peel");
                                    peelMode = PeelMode.None;
                                    frameCounter = 0;
                                    bPeelValid = false;

                                    modelLayer++;
                                    //layerOpacities[activeLayerIndex] = 1.0f;
                                }
                                else if (bPeelValid && MathHelper.ToDegrees(angleLeftX) < 60.0f)
                                {
                                    peelMode = PeelMode.None;
                                    frameCounter = 0;
                                    bPeelValid = false;
                                }
                                else if (frameCounter < frameLimit)
                                {
                                    peelMoveThreshold = (MathHelper.ToDegrees(angleLeftX) - 60.0f) / 70.0f;
                                    //layerOpacities[activeLayerIndex] = (MathHelper.ToDegrees(angleX) - 60.0f) / 70.0f;
                                    //layerOpacities[activeLayerIndex] -= 0.01f;
                                    //Console.WriteLine("Opacity " + activeLayerIndex + ": " + layerOpacities[activeLayerIndex]);
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


                    prevHandPos = currHandPos;
                    prevHandYAngle = currHandYAngle;
                    prevHandXAngle = currHandXAngle;

                    prevLeftHandPos = currLeftHandPos;
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
                //stream.Close();
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
            if (state.IsKeyDown(Keys.PageDown))
            {
                if (modelLayer > ModelLayer.Skeleton)
                    modelLayer--;
            }
            if (state.IsKeyDown(Keys.PageUp))
            {
                if (modelLayer < ModelLayer.Skin)
                    modelLayer++;
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

            glNearPoint = nearPoint;
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

            labelEffect.TextureEnabled = true;
            labelEffect.VertexColorEnabled = true;
            labelEffect.View = view;
            labelEffect.Projection = projection;
            labelEffect.World = Matrix.CreateScale(1, -1, 1);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, labelEffect);
            //spriteBatch.DrawString(labelFont, "" + glNearPoint/*peelMoveThreshold*/, new Vector2(-52, -70), Color.Red, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            //spriteBatch.End();

            switch (modelLayer)
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
            if(userProximity >= 375.0f)
                DrawSkinLayerLabelFar(effect, 1.0f);
            else if (userProximity >= 325.0f && userProximity < 375.0f)
            {
                DrawSkinLayerLabelFar(effect, (userProximity - 325.0f)/50.0f);
                DrawSkinLayerLabelMed(effect, (375.0f - userProximity)/50.0f);
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
            String[] organicFrontLabelText = { "Larynx", "Lung", "Liver", "Stomach", "Ascending colon", "Descending colon", "Rectum & anus"};
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
                aspectRatio, /*cutPlaneDistance*/0.1f, 10000.0f);

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

            if (peelMode == PeelMode.PeelingForward)
            {
                //Draw each layer bade on mesh indices
                //Opacity is determined by user position
                //the order of drawing is important (DON'T CHANGE!)

                //nervous
                drawLayer(m, 180, 184, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //circulatory
                drawLayer(m, 177, 179, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //skeletalq
                drawLayer(m, 146, 171, ((int)modelLayer * 100.0f - 50.0f) / 50.0f, scale, view, projection, transforms);

                //urinary
                drawLayer(m, 176, 176, (((int)modelLayer * 100.0f - 150.0f)- (50 - peelMoveThreshold *100/2)) / 50.0f, scale, view, projection, transforms);
                //digestive
                drawLayer(m, 4, 4, (((int)modelLayer * 100.0f - 150.0f)- (50 - peelMoveThreshold *100/2)) / 50.0f, scale, view, projection, transforms);
                //respiratory
                drawLayer(m, 172, 175, (((int)modelLayer * 100.0f - 150.0f)- (50 -peelMoveThreshold *100 /2)) / 50.0f, scale, view, projection, transforms);


                //muscle
                drawLayer(m, 5, 145, (((int)modelLayer * 100.0f - 250.0f) - ( 50 - peelMoveThreshold * 100 / 2)) / 50.0f, scale, view, projection, transforms);

                //skin
                drawLayer(m, 1, 3, (peelMoveThreshold * 100 / 2 - ((int)modelLayer * 100.0f - 350.0f)) / 50.0f, scale, view, projection, transforms);
                if (modelLayer == ModelLayer.Skin)
                {
                    int peelStep = (int)(peelMoveThreshold * 100) / 5;
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
                            drawLayer(m, 185, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 19:
                            drawLayer(m, 186, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 18:
                            drawLayer(m, 187, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 17:
                            drawLayer(m, 188, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 16:
                            drawLayer(m, 189, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 15:
                            drawLayer(m, 190, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 14:
                            drawLayer(m, 191, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 13:
                            drawLayer(m, 192, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 12:
                            drawLayer(m, 193, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 11:
                            drawLayer(m, 194, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 10:
                            drawLayer(m, 195, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 9:
                            drawLayer(m, 196, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 8:
                            drawLayer(m, 197, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 7:
                            drawLayer(m, 198, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 6:
                            drawLayer(m, 199, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 5:
                            drawLayer(m, 200, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 4:
                            drawLayer(m, 201, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 3:
                            drawLayer(m, 202, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 2:
                            drawLayer(m, 203, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 1:
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 0:
                            break;

                    }
                }
                //if (peelMoveThreshold > 20)
                //    ;// drawLayer(m, 185, 189, (((int)modelLayer * 100.0f - 350.0f) - peelMoveThreshold * 2) / 50.0f, scale, view, projection, transforms);
                //else if(peelMoveThreshold > 13)
                //    drawLayer(m, 188, 189, 1.0f/*(((int)modelLayer * 100.0f - 350.0f) - peelMoveThreshold * 2) / 50.0f*/, scale, view, projection, transforms);
                //else if(peelMoveThreshold > 7)
                //    drawLayer(m, 186, 189, 1.0f/*(((int)modelLayer * 100.0f - 350.0f) - peelMoveThreshold * 2) / 50.0f*/, scale, view, projection, transforms);
                
            }
            else if (peelMode == PeelMode.PeelingBackward)
            {
                //Draw each layer bade on mesh indices
                //Opacity is determined by user position
                //the order of drawing is important (DON'T CHANGE!)

                //nervous
                drawLayer(m, 180, 184, (((int)modelLayer * 100.0f - 150.0f)+ 50 + peelMoveThreshold * 100 / 2) / 50.0f, scale, view, projection, transforms);
                //circulatory
                drawLayer(m, 177, 179, (((int)modelLayer * 100.0f - 150.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);
                //skeletalq
                drawLayer(m, 146, 171, (((int)modelLayer * 100.0f - 50.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);

                //urinary
                drawLayer(m, 176, 176, (((int)modelLayer * 100.0f - 150.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);
                //digestive
                drawLayer(m, 4, 4, (((int)modelLayer * 100.0f - 150.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);
                //respiratory
                drawLayer(m, 172, 175, (((int)modelLayer * 100.0f - 150.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);


                //muscle
                drawLayer(m, 5, 145, (((int)modelLayer * 100.0f - 250.0f) + 50 + peelMoveThreshold * 100/2) / 50.0f, scale, view, projection, transforms);

                //skin
                if (modelLayer == ModelLayer.Muscle)
                {
                    int peelStep = (int)(peelMoveThreshold * 100) / 5;
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
                            drawLayer(m, 185, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 19:
                            drawLayer(m, 186, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 18:
                            drawLayer(m, 187, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 17:
                            drawLayer(m, 188, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 16:
                            drawLayer(m, 189, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 15:
                            drawLayer(m, 190, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 14:
                            drawLayer(m, 191, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 13:
                            drawLayer(m, 192, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 12:
                            drawLayer(m, 193, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 11:
                            drawLayer(m, 194, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 10:
                            drawLayer(m, 195, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 9:
                            drawLayer(m, 196, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 8:
                            drawLayer(m, 197, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 7:
                            drawLayer(m, 198, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 6:
                            drawLayer(m, 199, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 5:
                            drawLayer(m, 200, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 4:
                            drawLayer(m, 201, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 3:
                            drawLayer(m, 202, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 2:
                            drawLayer(m, 203, 203, 1.0f, scale, view, projection, transforms);
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 1:
                            drawLayer(m, 0, 0, 1.0f, scale, view, projection, transforms);
                            break;
                        case 0:
                            break;

                    }
                }
                drawLayer(m, 0, 3, (((int)modelLayer * 100.0f - 350.0f) + 50 + peelMoveThreshold * 2) / 50.0f, scale, view, projection, transforms);
            }
            else if (bInPickMode)
            {
                for (int i = 0; i < m.Meshes.Count; i++)
                {
                    foreach (BasicEffect effect in m.Meshes[i].Effects)
                    {
                        if (i != iPickIndex)
                            effect.Alpha = 0.3f;
                        else
                            effect.Alpha = 0.3f;

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
                drawLayer(m, 180, 184, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //circulatory
                drawLayer(m, 177, 179, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //skeletalq
                drawLayer(m, 146, 171, ((int)modelLayer * 100.0f - 50.0f) / 50.0f, scale, view, projection, transforms);

                //urinary
                drawLayer(m, 176, 176, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //digestive
                drawLayer(m, 4, 4, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);
                //respiratory
                drawLayer(m, 172, 175, ((int)modelLayer * 100.0f - 150.0f) / 50.0f, scale, view, projection, transforms);


                //muscle
                drawLayer(m, 5, 145, ((int)modelLayer * 100.0f - 250.0f) / 50.0f, scale, view, projection, transforms);

                //skin
                drawLayer(m, 0, 3, ((int)modelLayer * 100.0f - 350.0f) / 50.0f, scale, view, projection, transforms);
                drawLayer(m, 185, 203, ((int)modelLayer * 100.0f - 350.0f) / 50.0f, scale, view, projection, transforms);

                //drawLayer(m, 177, 179, 1.0f, scale, view, projection, transforms);
            }
        }

        private void drawLayer(Model modelIn, int startIndex, int endIndex, float alphaIn, float scale, Matrix view, Matrix projection, Matrix[] transforms)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                foreach (BasicEffect effect in modelIn.Meshes[i].Effects)
                {
                    if (alphaIn >= 1.0f)
                        effect.Alpha = 1.0f;
                    else if (alphaIn >= 0 && alphaIn < 1.0f)
                        effect.Alpha = alphaIn;
                    else
                        effect.Alpha = 0.0f;
                    //if (i == 0)
                    //{
                    //    effect.Alpha = 0.5f;
                    //}
                    //else if (i == 185)
                    //{
                    //    effect.Alpha = 1.0f;
                    //}
                    //else
                    //{
                    //if (alphaIn >= 0)
                    //    effect.Alpha = 1.0f;
                    //else
                    //    effect.Alpha = 0.0f;
                    //}
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

            frameCounter++;

            //set to null to draw to screen
            this.GraphicsDevice.SetRenderTarget(null);

            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.GraphicsDevice.Clear(Color.Black);

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
