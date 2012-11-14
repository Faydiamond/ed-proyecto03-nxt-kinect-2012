using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;

using NxtNet;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Kinect;
using Microsoft.Kinect.Interop;


namespace WpfApplication1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Runtime kinect;
        const int BLUE_IDX = 0;
        const int GREEN_IDX = 1;
        const int RED_IDX=2;
        byte[] depthFrame32 = new byte[320 * 240 * 4];
        private Nxt _nxt;
        Thread Motor;

        bool flagConexion = true;
        bool izq = false;
        bool der = false;
        bool ava = false;
        bool rev = false;
        bool stop = false;
        

        
        //Para la detección de las manos
        int HandLeftX=-1, HandLeftY=-1,
            HandRightX, HandRightY;

        public MainWindow() {
            
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closed += new EventHandler(MainWindow_Closed);
           
            /*
            //CONEXION CON EL ROBOT
            this._nxt = new Nxt();
            this._nxt.Connect("COM8");
            Motor = new Thread(new ThreadStart(detenerse));
            Motor.Start();
             */
            }

        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            kinect = new Runtime();

            try {
                kinect.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepth);
                }
            catch (InvalidOperationException) {
                System.Windows.MessageBox.Show("La inicialización en tiempo de ejecución falló. Asegúrate de que el Kinect está conectado.");
                return;
                }

            //kinect.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
            try {
                kinect.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                }
            catch (InvalidOperationException) {
                System.Windows.MessageBox.Show("Error al abrir el video");
                return;
                }

            kinect.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinect_VideoFrameReady);
            kinect.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth);
            kinect.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinect_DepthFrameReady);
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            }
    
        void MainWindow_Closed(object sender, EventArgs e) {
            kinect.Uninitialize();
            Environment.Exit(0);
            }

        void kinect_VideoFrameReady(object sender, ImageFrameReadyEventArgs e) {
            PlanarImage Image = e.ImageFrame.Image;
            image1.Source = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);
            }
        
        void kinect_DepthFrameReady(object sender, ImageFrameReadyEventArgs e) {
            PlanarImage Image = e.ImageFrame.Image;
            byte[] convertedFrame = convertDepthFrame(Image.Bits);
            image2.Source = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, convertedFrame, Image.Width * 4);
            byte[] depthFrame32 = new byte[320 * 240 * 4];
            try
                {
                    //this.Title = HandLeftX + "," + HandLeftY;
                    //if (HandLeftX == 0) { return; }
                    cIx.Content = HandLeftX;
                    cIy.Content = HandLeftY;

                    //if (HandRightX == 0) { return; }
                    cDx.Content = HandRightX;
                    cDy.Content = HandRightY;

                //jaja no se que hice aqui, mas de rato lo acomodo el punto era q funcionara
                    
                        if(flagConexion){
                            this._nxt = new Nxt();
                            this._nxt.Connect("COM7");
                            msjConexion.Text = "Lego Conectado!";
                     
                            flagConexion = false;
                            Motor = new Thread(new ThreadStart(detenerse));
                            Motor.Start();
                            Thread.Sleep(1000);
                        }
                    
                        verificarLego();
                }
            catch{ }
            }

        byte[] convertDepthFrame(byte[] depthFrame16) {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4) {
                
                //desplazar el segundo byte 8 posiciones y hacer la OR con el primer byte
                int distance = ((depthFrame16[i16]) | (depthFrame16[i16 + 1] << 8));
                
                //Fondo negro
                if (distance <= 900){
                    depthFrame32[i32 + RED_IDX] = 0;
                    depthFrame32[i32 + GREEN_IDX] = 0;
                    depthFrame32[i32 + BLUE_IDX] = 0;
                    //mensaje.Text = "alejate";
                    
                    }

                //verde
                else if (distance > 900 && distance < 1100) {
                    //amarilloBool = false;   
                    depthFrame32[i32 + RED_IDX] = 0;
                    depthFrame32[i32 + GREEN_IDX] = 255;
                    depthFrame32[i32 + BLUE_IDX] = 0;
                    mensaje.Text = "Alejate un poco";
                    //MessageBox.Show("Hola");
                    
                    /*
                    if(!verdeBool ){
                        Motor = new Thread(new ThreadStart(avanzar));
                        Motor.Start();
                        verdeBool = true;
                        amarilloBool = false;
                        }
                    */
                    }

                //Azul
                else if (distance > 1100 && distance < 1300){
              
                    depthFrame32[i32 + RED_IDX] = 0;
                    depthFrame32[i32 + GREEN_IDX] = 0;
                    depthFrame32[i32 + BLUE_IDX] = 255;
                    }

                //rojo
                else if (distance > 1300 && distance < 1800) {
                    depthFrame32[i32 + RED_IDX] = 255;
                    depthFrame32[i32 + GREEN_IDX] = 0;
                    depthFrame32[i32 + BLUE_IDX] = 0;
                    //mensaje.Text = "Ahi estas bien!!!";
                    if (distance > 1400 && distance < 1700){
                        mensaje.Text = "Justo Ahí. No te muevas";
                    
                        }
                    
                }

                //amarillo
                else if (distance > 1800 && distance < 2000){
                    depthFrame32[i32 + RED_IDX] = 255;
                    depthFrame32[i32 + GREEN_IDX] = 255;
                    depthFrame32[i32 + BLUE_IDX] = 0;
                    mensaje.Text = "Acercate un poco";
                    /*
                    if (!amarilloBool){
                        Motor = new Thread(new ThreadStart(reversa));
                        Motor.Start();
                        amarilloBool = true;
                        verdeBool = false;
                        }
                     */
                    }

                //Cyan
                else if (distance > 2000 && distance < 2200){
                    depthFrame32[i32 + RED_IDX] = 0;
                    depthFrame32[i32 + GREEN_IDX] = 255;
                    depthFrame32[i32 + BLUE_IDX] = 255;
                
                    }

                //Violeta
                else if (distance > 2200){
                    depthFrame32[i32 + RED_IDX] = 143;
                    depthFrame32[i32 + GREEN_IDX] = 0;
                    depthFrame32[i32 + BLUE_IDX] = 255;
                    }

              /*  if (HandRightX >= 280 && HandRightX <= 301)
                {
                    mensaje.Text = "Muevete Lego!!!";
                    
                }
                */

                }
            return depthFrame32;
            }

        public void avanzar() {
            this._nxt.SetOutputState(MotorPort.PortA, -75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, -75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            
            }


        public void detenerse() {
            this._nxt.PlayTone(601, 1000);
            this._nxt.SetOutputState(MotorPort.PortA, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 0, MotorModes.Brake, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }

        public void reversa() {
            this._nxt.SetOutputState(MotorPort.PortA, 75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 75, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }

        public void derecha() {
            
            this._nxt.SetOutputState(MotorPort.PortA, 80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, -80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }
        
        public void izquierda() {
            this._nxt.SetOutputState(MotorPort.PortA, -80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            this._nxt.SetOutputState(MotorPort.PortB, 80, MotorModes.On, MotorRegulationMode.Sync, 0, MotorRunState.Running, 0);
            }

        public void verificarLego()
        {
            if (HandRightX >= 400 && HandRightY >= 100 && HandRightY <= 300 && !der && HandLeftX >= 180)
            {
               Motor = new Thread(new ThreadStart(derecha));
                Motor.Start();
                der = true;
                izq = false;
                ava = false;
                rev = false;
                stop = false;
                msjLego.Text = "Derecha!";
            }
   
            else if (HandLeftX <= 130 && HandLeftY >= 100 && HandLeftY <= 300 && !izq && HandRightX < 400)
            {
             Motor = new Thread(new ThreadStart(izquierda));
               Motor.Start();
                der = false;
                izq = true;
                ava = false;
                rev = false;
                stop = false;
                msjLego.Text = "Izquierda!";
            }

            else if (HandRightY <= 100 && HandLeftY <= 100 && !ava /*&& HandRightX >= 300 && HandLeftX >= 180 && HandLeftX >= 200*/ )
            {
                Motor = new Thread(new ThreadStart(avanzar));
                Motor.Start();
                der = false;
                izq = false;
                ava = true;
                rev = false;
                stop = false;
                msjLego.Text = "Avanzar!";
            }

            else if (HandRightY <= 300 && HandLeftY <= 300 && !rev && HandLeftY >= 100 && HandRightY >= 100 && HandRightX < 400 && HandRightX > 300 && HandLeftX > 130 && HandLeftX < 200)
           {
                Motor = new Thread(new ThreadStart(reversa));
                Motor.Start();
                der = false;
                izq = false;
                ava = false;
                rev = true;
                stop = false;
                msjLego.Text = "Reversa!";
            }
            
            else if( HandRightY >= 330 && HandLeftY >= 330 && !stop )
            {
                Motor = new Thread(new ThreadStart(detenerse));
                Motor.Start();
                der = false;
                izq = false;
                ava = false;
                rev = false;
                stop = true;
                msjLego.Text = "Stop!";
            }

            
        }

     
        //Métodos que permiten la detección de las manos
        //Hey this is just the most important part that detects whether the hand is making a fist
        bool isMakingAFist(ImageSource imgHand){
            bool wasBlack = false;
            int BlackWidth = 0;
            int BlackTimes = 0;

            for (int yy = 10; yy < imgHand.Height - 10; yy += 10)
            {
                for (int xx = 3; xx </*MaxX*/ imgHand.Width; xx++)
                {
                    if (PixelColor(imgHand, xx, yy) == Colors.Black)
                    {
                        if (!wasBlack)
                        {
                            if (BlackWidth > 1 && BlackWidth < 15)
                            { BlackTimes++; }
                            BlackWidth = 0;
                        }
                        else
                        {
                            BlackWidth++;
                        }

                        wasBlack = true;
                    }
                    else
                    {
                        wasBlack = false;
                    }
                }
                if (BlackTimes > 1) { return false; }
            }
            //this.Title = BlackTime.ToString();
            return true;
        }

        Color PixelColor(ImageSource img, int PixelX, int PixelY)
        {
            /// pick the color to a byte array
            CroppedBitmap cb = new CroppedBitmap((BitmapSource)img, new Int32Rect(PixelX, PixelY, 1, 1));
            byte[] pix = new byte[4];
            cb.CopyPixels(pix, 4, 0);

            /// return the picked color
            return Color.FromRgb(pix[2], pix[1], pix[0]);
        }

        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            kinect.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320));  //convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240));  //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            kinect.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(image1.Width * colorX / 640.0) - 30, (int)(image1.Height * colorY / 480) - 30);
        }

        void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        #region Update XY

                        //MessageBox.Show(joint.ID.ToString());
                        switch (joint.ID.ToString())
                        {
                            case "HandLeft":
                                Point getPl = getDisplayPosition(joint);
                                HandLeftX = (int)getPl.X;
                                HandLeftY = (int)getPl.Y;
                                break;
                            case "HandRight":
                                Point getPr = getDisplayPosition(joint);
                                HandRightX = (int)getPr.X;
                                HandRightY = (int)getPr.Y;
                                break;
                        }
                        #endregion Update XY
                    }
                }
                iSkeleton++;
            } // for each skeleton
        }

      

        }
    }
