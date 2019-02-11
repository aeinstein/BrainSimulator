using System;
using System.ComponentModel;
using System.Linq;
using System.Drawing;

using GoodAI.Core.Memory;
using GoodAI.Core.Nodes;
using GoodAI.Core.Task;
using GoodAI.Core.Utils;
using YAXLib;

using System.IO;

//namespace GoodAI.Modules.Vision
namespace HTSLmodule.Worlds
{
    /// <author>GoodAI</author>
    /// <meta>CireNeikual,jv</meta>
    /// <status>Finished</status>
    /// <summary>Presents given sequence of image frames to the output.</summary>
    /// <description>The dataset can be used either default one or custom before the simulation. Here (compared to the ImageDatasetWorld), 
    /// the dataset can be updated at runtime (path to the images and their count). The only requirement is to preserve image resolution 
    /// chosen before the simulation.
    /// 
    /// <h3>Parameters</h3>
    /// <ul>
    ///     <li> <b>UseCustomDataset:</b> If true, the world will attempt to read dataset by given RootFileName, Digits, Extension and NumFrames.</li>
    ///     <li> <b>NumFrames:</b> Number of frames to be loaded from the dataset (starting from 0) and sequentially presented to output.</li>
    ///     <li> <b>RootFileName:</b> Defines path to the file and base part of the name. Name is composed as follows: [RootFineName][numDigits][Extension]. 
    ///     Where items are numbered from 0.</li>
    ///     <li> <b>Digits:</b> How many digits is in the filename? E.g. for 5 it is "RootFilename_00000.png", "RootFilename_00001.png", etc..</li>
    ///     <li> <b>Extension:</b> E.g. ".png"</li>
    /// </ul>
    /// 
    /// Note that the same parameters are in the Load task. This task can be run once for changing the dataset at runtime.
    /// </description>
    public class MyAnimationPredictionWorld : MyWorld
    {
        #region parameters
        [MyBrowsable, Category("File Size"), YAXSerializableField(DefaultValue = 320)]
        public int ImageWidth
        {
            get { return m_iw; }
            set
            {
                if (value > 0)
                {
                    m_iw = value;
                }
            }
        }
        private int m_iw;

        [MyBrowsable, Category("File Size"), YAXSerializableField(DefaultValue = 160)]
        public int ImageHeight
        {
            get { return m_ih; }
            set
            {
                if (value > 0)
                {
                    m_ih = value;
                }
            }
        }
        private int m_ih;

        [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = false)]
        public bool UseCustomDataset { get; set; }

        [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = 0),
        Description("Max Number of Files, 0 for disabled")]
        public int NumFrames
        {
            get { return m_numFrames; }
            set
            {
                if (value >= 0)
                {
                    m_numFrames = value;
                }
            }
        }
        private int m_numFrames;


        [MyBrowsable, Category("Params"), YAXSerializableField(DefaultValue = false),
        Description("Import all 3 Channels")]
        public Boolean IsRGB { get; set; }
        //private Boolean m_isRGB = false;

        [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = ""),
        Description("Path to files including the name prefix")]
        public String RootFileName { get; set; }

        [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = ""),
        Description("file name has the following form: {search}_*.{Extension}.")]
        public String Search { get; set; }

        [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = "jpg"),
        Description("Example: jpg")]
        public String Extension { get; set; }

        #endregion

        private int m_defNumFrames = 0;
        private String m_defRootFileName = MyResources.GetMyAssemblyPath() + "\\" + @"\res\animationpredictionworld\";
        private String m_defExtension = "png";
        private String m_defSearch = "SwitchTest_";
        private bool m_defIsRGB = false;
        //private int m_defImageWidth = 320;
        //private int m_defImageHeight = 160;

        private int m_currentFrame;
        private int m_steps = 1;
        

        [MyOutputBlock(0)]
        public MyMemoryBlock<float> Image
        {
            get { return GetOutput(0); }
            set { SetOutput(0, value); }
        }

        public Bitmap[] m_bitmaps;

        public MyAnimationPredictionLoadTask AnimationPredictionLoadTask { get; private set; }
        public MyAnimationPredictionPresentTask AnimationPredictionPresentTask { get; private set; }

        public override void UpdateMemoryBlocks()
        {
            if(IsRGB)
            {
                //Image.ColumnHint = ImageWidth * 3;
                Image.Count = ImageWidth * ImageHeight * 3;
                Image.Dims = new TensorDimensions(ImageWidth, ImageHeight, 3);
            } else
            {
                //Image.ColumnHint = ImageWidth;
                Image.Count = ImageWidth * ImageHeight;
                Image.Dims = new TensorDimensions(ImageWidth, ImageHeight);
            }
                
        }

        public override void Validate(MyValidator validator)
        {
            base.Validate(validator);

            if (UseCustomDataset)
            {
                try
                {
                    this.m_bitmaps = LoadBitmaps(NumFrames, RootFileName, Search, Extension);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    validator.AddWarning(this, "Loading default dataset, cause: "+ e.Message);
                    //UseDefaultBitmaps();
                }
                catch(IndexOutOfRangeException e)
                {
                    validator.AddWarning(this, "Loading the default dataset, cause: "+e.Message);
                    //UseDefaultBitmaps();
                }
                catch (Exception)
                {
                    validator.AddWarning(this, "Loading the default dataset, cause: could not read file(s)");
                    //UseDefaultBitmaps();
                }
            }
            else
            {
                UseDefaultBitmaps();
            }
        }

        private void UseDefaultBitmaps()
        {
            NumFrames = 18;
            ImageWidth = 64;
            ImageHeight = 64;
            m_currentFrame = 0;
            IsRGB = false;
            this.m_bitmaps = LoadBitmaps(m_defNumFrames, m_defRootFileName, m_defSearch, m_defExtension);
        }

        private Bitmap[] LoadBitmaps(int numFrames, String rootFileName, String search, String extension)
        {
            MyLog.INFO.WriteLine("opening: " + rootFileName);

            DirectoryInfo d = new DirectoryInfo(rootFileName);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles(search + "*." + extension); //Getting Text files

            int tmpNumFrames = Files.Length;

            if(tmpNumFrames == 0)
            {
                MyLog.WARNING.WriteLine("No Images found in:" + rootFileName);
                return new Bitmap[0];
                
            }

            Bitmap[] bitmaps;
            if (numFrames > 0)
            {
                bitmaps = new Bitmap[numFrames];
            } else
            {
                bitmaps = new Bitmap[tmpNumFrames];
            }
            
            String fileName = "";


            int i = 0;
            foreach (FileInfo file in Files)
            {
                fileName = rootFileName + file.Name;

                MyLog.INFO.WriteLine("loading: " + fileName);

                bitmaps[i] = new Bitmap(fileName);

                if (bitmaps[i].Width != ImageWidth || bitmaps[i].Height != ImageHeight)
                {
                    throw new IndexOutOfRangeException("Incorrect width or height of a given image: " + fileName);
                }

                i++;
                if(numFrames > 0 && i >= numFrames -1)
                {
                    MyLog.INFO.WriteLine(numFrames + " loaded");
                    return bitmaps;
                }
            }

            return bitmaps;
        }

        /// <summary>
        /// Tries to reload the images during the simulation. Old bitmaps are preserved if the attempt is unsuccessful and simulation continues.
        /// If loading is OK, task can be disabled again to increase the speed of simulation.
        /// </summary>
        [Description("Reload images."), MyTaskInfo(Disabled = true)]
        public class MyAnimationPredictionLoadTask : MyTask<MyAnimationPredictionWorld>
        {
            [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = ""),
            Description("Path to files with file name prefix")]
            public String RootFileName { get; set; }

            [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = ""),
            Description("file name has the following form: {search}_*.{Extension}.")]
            public String Search { get; set; }

            [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = "jpg"),
            Description("Example: jpg")]
            public String Extension { get; set; }

            [MyBrowsable, Category("File"), YAXSerializableField(DefaultValue = 0),
            Description("Max Number of Files, 0 for disabled")]
            public int NumFrames { get; set; }
      
            public override void Init(int nGPU)
            {
            }

            public override void Execute()
            {
                try
                {
                    Owner.m_bitmaps = Owner.LoadBitmaps(NumFrames, RootFileName, Search, Extension);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    MyLog.WARNING.WriteLine("Reload images Task: leaving defautl dataset, cause: " + e.Message);
                    
                }
                catch (IndexOutOfRangeException e)
                {
                    MyLog.WARNING.WriteLine("Reload images Task: leaving the default dataset, cause: " + e.Message);
                    
                }
                catch (Exception)
                {
                    MyLog.WARNING.WriteLine("Reload images Task: leaving the default dataset, cause: could not read file(s)");    
                }
            }
        }

        /// <summary>
        /// Puts the current bitmap onto the output.
        /// </summary>
        [Description("Show images.")]
        public class MyAnimationPredictionPresentTask : MyTask<MyAnimationPredictionWorld>
        {
            [MyBrowsable, Category("Simulation"), YAXSerializableField(DefaultValue = false),
            Description("Continue from the current frame after restarting the simulation?")]
            public bool StartFromFirstFrame { get; set; }

            [MyBrowsable, Category("Params"), YAXSerializableField(DefaultValue = 1),
            Description("Number of Steps one image is visible")]
            public int ExpositionTime { get; set; }


            float[] image;
            byte[] byteArray;
            public override void Init(int nGPU)
            {
                Owner.m_steps = ExpositionTime;

                if (StartFromFirstFrame)
                {
                    Owner.m_currentFrame = 0;
                }
            }

            // Could be optimized so that the images are located at GPU, but the dataset may not potentially fit into the GPU memory.
            public override void Execute()
            {
                if (Owner.m_currentFrame >= Owner.m_bitmaps.Count() -1)
                {
                    Owner.m_currentFrame = 0;
                }

                MyLog.DEBUG.WriteLine("images: " + Owner.m_bitmaps.Count());
                //MyLog.DEBUG.WriteLine("current image: " + Owner.m_currentFrame);

                image = new float[Owner.Image.Count];

                int blocksize = Owner.ImageWidth * Owner.ImageHeight;

                for (int y = 0; y < Owner.ImageHeight; y++)
                {
                    int offset = y * Owner.ImageWidth;

                    for (int x = 0; x < Owner.ImageWidth; x++)
                    {
                        Color c = Owner.m_bitmaps[Owner.m_currentFrame].GetPixel(x, y);

                        if (Owner.IsRGB)
                        {
                            image[x + offset] = c.R / 255.0f;
                            image[x + offset + blocksize] = c.G / 255.0f;
                            image[x + offset + blocksize *2] = c.B / 255.0f;
                        }
                        else
                        {
                            // convert to Black/White
                            image[y * Owner.ImageWidth + x] = 0.333f * (c.R / 255.0f + c.G / 255.0f + c.B / 255.0f);
                        }
                    }
                }

                
                // Create memory block from data
                byteArray = new byte[image.Length * 4];
                Buffer.BlockCopy(image, 0, byteArray, 0, byteArray.Length);
                Owner.Image.Fill(byteArray);

                if(ExpositionTime > 0)
                {
                    if (Owner.m_steps == 0)
                    {
                        Owner.m_currentFrame++;
                        Owner.m_steps = ExpositionTime;

                    }
                    else
                        Owner.m_steps--;
                }  
            }
        }
    }
}
