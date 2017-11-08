using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic; // List
using System.Linq;

using Windows.Kinect;

using OpenCVForUnity;

public class VisionController: MonoBehaviour
{
    Manager manager; // game states    

    KinectSensor sensor;
    ColorFrameReader reader;
    DepthFrameReader depth_reader;
    MultiSourceFrameReader multi_reader;

    CoordinateMapper coordinateMapper;
    DepthSpacePoint[] depthSpacePoints;


    Texture2D texture;
    Texture2D depth_texture;

    byte[] data;
    public ushort[] depthData;

    Mat rgbaMat;
    Mat rgbMat; // height and width will be specified in Start() by the frame from Kinect

    Mat hsvMat;
    Mat thresholdMat_red;
    Mat thresholdMat_green; 
    Mat thresholdMat_yellow; 
    Mat thresholdMat;
    Mat demo;
    Mat mask;

    Mat depthMat;
    Mat showDepthMat;


    bool isShowMask = true;
    public bool isShowDepth = false;

    public int morphCore_red = 5;
    public int morphCore_green = 5;
    public int morphCore_yellow = 5;
    public int blurCore = 3;

    // smooth
    public float smoothTime = 0.3f; //     oneSecond -= Time.deltaTime;

    // initialize three books 
    public SpellBook red_book = new SpellBook("red"); 
    public SpellBook green_book = new SpellBook("green");
    public SpellBook yellow_book = new SpellBook("yellow");

    // HSV values on color picker, adjusted by sliders, will be converted to opencv values later
    public float Hmin_red = 340;
    public float Hmax_red = 20;
    public float Smin_red = 50;
    public float Smax_red = 100;
    public float Vmin_red = 50;
    public float Vmax_red = 100;

    public float Hmin_green = 120;
    public float Hmax_green = 170;
    public float Smin_green = 40;
    public float Smax_green = 100;
    public float Vmin_green = 50;
    public float Vmax_green = 100;

    /*
    public Slider H_MIN_RED; public Text H_MIN_RED_sliderValue;
    public Slider H_MAX_RED; public Text H_MAX_RED_sliderValue;
    public Slider S_MIN_RED; public Text S_MIN_RED_sliderValue;
    public Slider S_MAX_RED; public Text S_MAX_RED_sliderValue;
    public Slider V_MIN_RED; public Text V_MIN_RED_sliderValue;
    public Slider V_MAX_RED; public Text V_MAX_RED_sliderValue;
    */

    //const int MAX_NUM_OBJECTS = 1;
    public int MIN_OBJECT_AREA = 100;

    public enum modeType
    {
        source,
        threshold,
        depth,
        mask,
    }
    public modeType mode;

    public bool isDraw = true;

    public enum devMode
    {
        color,
        depth,
        multi,
    }
    devMode testMode;

    // intialize multi source
    // DWORD ? -> uint            
    

    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        testMode = devMode.color;

        if (manager.getIsVision())
        {
            sensor = KinectSensor.GetDefault();

            mode = modeType.source;

            if (sensor != null)
            {
                FrameDescription frameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
                FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;

                texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
                data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];

                depth_texture = new Texture2D(512, 424, TextureFormat.RGBA32, false);
                depthData = new ushort[depthFrameDesc.LengthInPixels];

                depthSpacePoints = new DepthSpacePoint[frameDesc.LengthInPixels];
                

                if (testMode == devMode.color)
                {
                    reader = sensor.ColorFrameSource.OpenReader();
                }
                else if (testMode == devMode.depth)
                {
                    depth_reader = sensor.DepthFrameSource.OpenReader();
                }
                else if (testMode == devMode.multi)
                {
                    multi_reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
                }
                
                if (!sensor.IsOpen)
                {
                    sensor.Open();
                }

                // needed..
                rgbaMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);
                rgbMat = new Mat(texture.height, texture.width, CvType.CV_8UC3);                              
                hsvMat = new Mat();
                thresholdMat_red = new Mat();
                thresholdMat_green = new Mat();
                thresholdMat_yellow = new Mat();
                thresholdMat = new Mat();
                demo = new Mat(texture.height, texture.width, CvType.CV_8UC4);

                depthMat = new Mat(depth_texture.height, depth_texture.width, CvType.CV_8UC1);
                showDepthMat = new Mat(depth_texture.height, depth_texture.width, CvType.CV_8UC4);

            }
            else
            {
                UnityEngine.Debug.LogError("No ready Kinect found!");
            }

        }      


    }

    void Update()
    {
        if (manager.getIsVision())
        {

            if (testMode == devMode.color && reader != null)
            {
                ColorFrame frame = reader.AcquireLatestFrame();              
                
                if (frame != null)
                {
                    frame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Rgba);

                    frame.Dispose();
                    frame = null;
                }
                
                processColorImg();

                visualizeColorImg();
            }

            // depth
            else if (testMode == devMode.depth && depth_reader != null)
            {
                FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;

                depth_texture = new Texture2D(512, 424, TextureFormat.RGBA32, false);
                depthData = new ushort[depthFrameDesc.LengthInPixels];

                DepthFrame depthFrame = depth_reader.AcquireLatestFrame();
                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(depthData);

                    depthFrame.Dispose();
                    depthFrame = null;

                    visualizeDepthImg();
                }

            }

            else if (testMode == devMode.multi && multi_reader != null)
            {
                MultiSourceFrame multiSourceFrame = multi_reader.AcquireLatestFrame();

                if (multiSourceFrame != null)
                {
                    using (ColorFrame colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                        {
                            colorFrame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Rgba);
                        }

                        processColorImg();
                        visualizeColorImg();

                    }                                


                    using (DepthFrame depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame())
                    {
                        if (depthFrame != null)
                        {
                            //Debug.Log ("bodyIndexFrame not null");
                            depthFrame.CopyFrameDataToArray(depthData);
                        }

                    }

                }
            }

            else
            {
                return;
            }                   



            // visualize results
            if(testMode != devMode.depth)
            {
                gameObject.transform.localScale = new Vector3(texture.width, texture.height, 1);
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;
                Camera.main.orthographicSize = texture.height / 2;

                
            }
            else
            {
                
            }

        }
        
        
    }


    void OnApplicationQuit()
    {
        if (testMode == devMode.color && reader != null)
        {
            reader.Dispose();
            reader = null;
        }
        else if (testMode == devMode.depth && depth_reader != null)
        {
            depth_reader.Dispose();
            depth_reader = null;
        }
        else if (testMode == devMode.multi && multi_reader != null)
        {
            multi_reader.Dispose();
            multi_reader = null;
        }


        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            sensor = null;
        }
    }

    void OnGUI()
    {
        //float screenScale = Screen.width / 1080.0f;
        // Matrix4x4 scaledMatrix = Matrix4x4.Scale(new Vector3(screenScale, screenScale, screenScale));
        //.matrix = scaledMatrix;
        
        GUILayout.BeginVertical();

        if (GUILayout.Button("source"))
        {
            mode = modeType.source;

            if(testMode == devMode.color)
            {
                return;
            }
            if (testMode == devMode.depth && depth_reader != null)
            {
                depth_reader.Dispose();
                depth_reader = null;
            }
            else if (testMode == devMode.multi && multi_reader != null)
            {
                multi_reader.Dispose();
                multi_reader = null;
            }
            
            testMode = devMode.color;
            if(reader == null)
            {
                reader = sensor.ColorFrameSource.OpenReader();
            }

        }

        if (GUILayout.Button("threshold"))
        {
            mode = modeType.threshold;
        }
        if (GUILayout.Button("depth"))
        {
            isShowDepth = !isShowDepth;
            /*
            if (testMode == devMode.depth)
            {
                return;
            }
            if (testMode == devMode.color && reader != null)
            {
                reader.Dispose();
                reader = null;                
            }
            else if (testMode == devMode.multi && multi_reader != null)
            {
                multi_reader.Dispose();
                multi_reader = null;             
            }

            testMode = devMode.depth;
            if(depth_reader == null)
            {
                depth_reader = sensor.DepthFrameSource.OpenReader();
            }
            */
        }

        if (GUILayout.Button("multi"))
        {
            if (testMode == devMode.multi)
            {
                return;
            }
            if (testMode == devMode.color && reader != null)
            {
                reader.Dispose();
                reader = null;
            }
            else if (testMode == devMode.depth && depth_reader != null)
            {
                depth_reader.Dispose();
                depth_reader = null;
            }

            testMode = devMode.multi;

            if(multi_reader == null)
            {
                multi_reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            }
        }


        if (GUILayout.Button("Draw"))
        {
            isDraw = !isDraw;
        }
        if (GUILayout.Button("Mask"))
        {
            isShowMask = !isShowMask;
        }


        GUILayout.EndVertical();
    }

    void draw(SpellBook book, Mat frame)
    {
        if (book.isTrackedm == true)
        {
            Imgproc.circle(frame, new Point(book.xm, book.ym), 20, new Scalar(255, 255, 0));
            Imgproc.putText(frame, book.xm + ", " + book.ym, new Point(book.xm - 500, book.ym + 20), 5, 5, new Scalar(255, 255, 0), 2);
            Imgproc.putText(frame, book.color, new Point(book.xm, book.ym - 20), 5, 5, new Scalar(255, 255, 0), 2);
            // Debug.Log("drawing"+book.color + ": " + book.xm.ToString() + ", " + book.ym.ToString());
        }else
        {
            // Debug.Log(book.color + ": not found");
        }
    }
    void visualizeColorImg()
    {
        if (mode == modeType.source)
        {
            if (isDraw)
            {
                draw(red_book, rgbaMat);
                draw(green_book, rgbaMat);
                draw(yellow_book, rgbaMat);
            }

            if (isShowMask)
            {
                Imgproc.cvtColor(mask, mask, Imgproc.COLOR_RGB2RGBA);
                rgbaMat = rgbaMat & mask;

            }
            Utils.matToTexture(rgbaMat, texture);

        }
        if (mode == modeType.threshold)
        {
            if (isDraw)
            {
                draw(red_book, demo);
                draw(yellow_book, demo);
                draw(green_book, demo);
            }
            if (isShowMask)
            {
                Imgproc.cvtColor(mask, mask, Imgproc.COLOR_RGB2RGBA);
                demo = demo & mask;
            }

            Utils.matToTexture(demo, texture);

        }
    }

    void visualizeDepthImg()
    {
        // visualize depth image
        isShowMask = false;
        isDraw = false;

        Imgproc.cvtColor(depthMat, showDepthMat, Imgproc.COLOR_GRAY2RGBA);
        Utils.matToTexture(showDepthMat, depth_texture);
        
        gameObject.transform.localScale = new Vector3(depth_texture.width, depth_texture.height, 1);
        gameObject.GetComponent<Renderer>().material.mainTexture = depth_texture;
        Camera.main.orthographicSize = depth_texture.height / 2;
    }

    void processColorImg()
    {
        Utils.copyToMat(data, rgbaMat);

        Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);

        // generate the mask
        mask = new Mat(rgbMat.size(), CvType.CV_8UC3);
        mask.setTo(new Scalar(255, 255, 255));
        Imgproc.circle(mask, new Point(mask.cols() / 2, mask.rows() / 2), 125, new Scalar(0, 0, 0), -1, 8, 0);

        rgbMat = rgbMat & mask;

        //first find red objects
        // OpenCV uses H: 0 - 180, S: 0 - 255, V: 0 - 255

        /*
        Hmin_red = H_MIN_RED.value; H_MIN_RED_sliderValue.text = "H_min_red: " + Hmin_red.ToString();
        Hmax_red = H_MAX_RED.value; H_MAX_RED_sliderValue.text = "H_max_red: " + Hmax_red.ToString();
        Smin_red = S_MIN_RED.value; S_MIN_RED_sliderValue.text = "S_min_red: " + Smin_red.ToString();
        Smax_red = S_MAX_RED.value; S_MAX_RED_sliderValue.text = "S_max_red: " + Smax_red.ToString();
        Vmin_red = V_MIN_RED.value; V_MIN_RED_sliderValue.text = "V_min_red: " + Vmin_red.ToString();
        Vmax_red = V_MAX_RED.value; V_MAX_RED_sliderValue.text = "V_max_red: " + Vmax_red.ToString();*/

        Imgproc.cvtColor(rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);

        // OpenCV uses H: 0 - 180, S: 0 - 255, V: 0 - 255
        if (Hmin_red <= Hmax_red)
        {
            red_book.hsv_min = new Scalar(Hmin_red / 2, Smin_red * 2.55f, Vmin_red * 2.55f);
            red_book.hsv_max = new Scalar(Hmax_red / 2, Smax_red * 2.55f, Vmax_red * 2.55f);
            Core.inRange(hsvMat, red_book.hsv_min, red_book.hsv_max, thresholdMat_red);
        }
        else
        {
            Mat thr = new Mat(), thr1 = new Mat();
            red_book.hsv_min = new Scalar(Hmin_red / 2, Smin_red * 2.55f, Vmin_red * 2.55f);
            red_book.hsv_max = new Scalar(360 / 2, Smax_red * 2.55f, Vmax_red * 2.55f);

            red_book.hsv1_min = new Scalar(0 / 2, Smin_red * 2.55f, Vmin_red * 2.55f);
            red_book.hsv1_max = new Scalar(Hmax_red / 2, Smax_red * 2.55f, Vmax_red * 2.55f);

            Core.inRange(hsvMat, red_book.hsv_min, red_book.hsv_max, thr);
            Core.inRange(hsvMat, red_book.hsv1_min, red_book.hsv1_max, thr1);

            thresholdMat_red = thr | thr1;
        }

        Imgproc.GaussianBlur(thresholdMat_red, thresholdMat_red, new Size(blurCore, blurCore), 0, 0);

        morphOps(thresholdMat_red, morphCore_red);
        trackFilteredObject(red_book, thresholdMat_red);

        green_book.hsv_min = new Scalar(Hmin_green / 2, Smin_green * 2.55f, Vmin_green * 2.55f);
        green_book.hsv_max = new Scalar(Hmax_green / 2, Smax_green * 2.55f, Vmax_green * 2.55f);

        Core.inRange(hsvMat, green_book.hsv_min, green_book.hsv_max, thresholdMat_green);
        Imgproc.GaussianBlur(thresholdMat_green, thresholdMat_green, new Size(blurCore, blurCore), 0, 0);
        morphOps(thresholdMat_green, morphCore_green);

        trackFilteredObject(green_book, thresholdMat_green);



        Core.inRange(hsvMat, yellow_book.hsv_min, yellow_book.hsv_max, thresholdMat_yellow);
        Imgproc.GaussianBlur(thresholdMat_yellow, thresholdMat_yellow, new Size(blurCore, blurCore), 0, 0);
        morphOps(thresholdMat_yellow, morphCore_yellow);

        trackFilteredObject(yellow_book, thresholdMat_yellow);

        thresholdMat = thresholdMat_red | thresholdMat_green | thresholdMat_yellow;

        // smooth results: x, y, istracked
        smoothTrackingResults(red_book);
        smoothTrackingResults(yellow_book);
        smoothTrackingResults(green_book);

        if (manager.num_players == 1)
        {
            if (red_book.isTrackedm || green_book.isTrackedm || yellow_book.isTrackedm)
                manager.setAllTracked(true);
            else
                manager.setAllTracked(false);
        }
        if (manager.num_players == 2)
        {
            if ((red_book.isTrackedm && green_book.isTrackedm) ||
                 (red_book.isTrackedm && yellow_book.isTrackedm) ||
                 (green_book.isTrackedm && yellow_book.isTrackedm))
                manager.setAllTracked(true);
            else
                manager.setAllTracked(false);
        }
        if (manager.num_players == 3)
        {
            // when three books are visble in the camera, start the ritual
            if (red_book.isTrackedm && green_book.isTrackedm && yellow_book.isTrackedm)
                manager.setAllTracked(true);
            else
                manager.setAllTracked(false);
        }

        Imgproc.cvtColor(thresholdMat, demo, Imgproc.COLOR_GRAY2RGBA);
        //Imgproc.cvtColor(mask, demo, Imgproc.COLOR_RGB2RGBA);

        // Utils.matToTexture2D(rgbMat, texture);
    }


    /// <summary>
    /// Morphs the ops
    /// 
    /// </summary>
    /// <param name="thresh">Thresh.</param>
    void morphOps(Mat thresh, int core)
    {
        //create structuring element that will be used to "dilate" and "erode" image.
        //the element chosen here is a 3px by 3px rectangle
        Mat element = Imgproc.getStructuringElement(0, new Size(core, core));
        //dilate with larger element so make sure object is nicely visible

        Imgproc.morphologyEx(thresh, thresh, Imgproc.MORPH_OPEN, element);
        Imgproc.morphologyEx(thresh, thresh, Imgproc.MORPH_CLOSE, element);

    }

    void trackFilteredObject(SpellBook book, Mat threshold)
    {
        book.x = -1; book.y = -1; book.isTracked = false;  
        Debug.Log("tracking " + book.color.ToString());
        Mat temp = new Mat();
        threshold.copyTo(temp);

        Imgproc.Canny(temp, temp, 50, 100);
        //these two vectors needed for output of findContours
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Mat hierarchy = new Mat();
        //find contours of filtered image using openCV findContours function
        Imgproc.findContours(temp, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

        double max_area = MIN_OBJECT_AREA; // initialize
        //use moments method to find our filtered object

        if (hierarchy.rows() > 0)
        {
            int numObjects = contours.Count;
            //Debug.Log("numObj: " + numObjects.ToString());
            //Debug.Log("hierarchy " + hierarchy.ToString());
            for (int i = 0; i < numObjects; i++)
            {
                //Debug.Log("i = " + i.ToString());
                Moments moment = Imgproc.moments(contours[i]);
                double area = moment.get_m00();

                //we only want the object with the largest area so we save a reference area each
                //iteration and compare it to the area in the next iteration.
                if (area > max_area)
                {
                    book.x = (int)(moment.get_m10() / area);
                    book.y = (int)(moment.get_m01() / area);
                    max_area = area;
                }

            }
            if (book.x != -1)
                book.isTracked = true;
            else
                book.isTracked = false;
        }
    }

    // running and storing tracking results for 1 second
    // get the mode of the stored results
    // then update the isTracked, and x,y values with the smoothed ones

    void smoothTrackingResults(SpellBook book)
    {
        smoothTime -= 0.1f;
        if (smoothTime > 0)
        {
            // add only tracked value
            if (book.x !=-1)
            {
                book.xs.Add(book.x);
                book.ys.Add(book.y);
            }
           
        }
        else // one sencond is down
        {          
                        
            // check if it is empty, else running average
            if (!book.xs.Any() || !book.ys.Any())
            {
                book.xm = -1;
                book.ym = -1;
                book.isTrackedm = false;
            }
                
            else
            {
                book.xm = (int)book.xs.Average();
                book.ym = (int)book.ys.Average();
                book.isTrackedm = true;
            }

            // clear list and reset oneSecond
            book.xs = new List<int>();
            book.ys = new List<int>();
            smoothTime = 0.3f;

        }
    }
}
