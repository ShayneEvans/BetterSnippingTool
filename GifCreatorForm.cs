using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class GifCreatorForm : Form
{
    private bool _disposed = false;
    private System.Drawing.Rectangle selectedArea;
    private int borderSize = 5;
    private ProgressBar progressBar;
    private System.Windows.Forms.Timer updateTimer;
    private int totalScreenshots;
    private int currentScreenshot = 0;
    private MenuStrip gifCreatorBar;
    private ToolStripMenuItem playItem;
    private ToolStripMenuItem pauseItem;
    private ToolStripMenuItem redoItem;
    private Image playItemButton;
    private Image pauseItemButton;
    private Image redoItemButton;
    private int gifCreatorBarHeight = 100;
    private Rectangle gifArea;
    public GifCreatorForm(string tempDir, string outputDir, System.Drawing.Rectangle selectedArea, int currentScreenIndex)
    {
        this.selectedArea = selectedArea;
        this.gifArea = new Rectangle(borderSize / 2, borderSize / 2, selectedArea.Width + borderSize, selectedArea.Height + borderSize);
        InitializeComponent();
        this.StartPosition = FormStartPosition.Manual;
        Screen selectedScreen = Screen.FromPoint(new System.Drawing.Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));
        this.Location = new System.Drawing.Point(selectedArea.X + selectedScreen.Bounds.X -  borderSize, selectedArea.Y + selectedScreen.Bounds.Y - borderSize);
        this.Paint += new PaintEventHandler(DrawCustomBorder);

        Task.Run(async () =>
        {
            (int, int) resizeResolution = obtainResizeResolution(selectedArea.Width, selectedArea.Height);
            int framerate = 24;
            int seconds = 5;
            Console.WriteLine("Processing screenshots into GIF");
            await Task.Run(() => CreateGIFScreenshots(framerate, 5, tempDir, selectedArea, resizeResolution.Item1, resizeResolution.Item2));
            await Task.Run(() =>
            {
                FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
                $"-framerate {framerate} -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -vf \"palettegen=max_colors=256:reserve_transparent=0\" -y \"{Path.Combine(outputDir, "palette.png")}\"");
                FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
                $"-framerate {framerate} -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -i \"{Path.Combine(outputDir, "palette.png")}\" -filter_complex \"fps={framerate},format=rgba,paletteuse=dither=sierra2_4a\" -y \"{Path.Combine(outputDir, $"output_{framerate}.gif")}\"");
            });

            this.Invoke((Action)(() =>
            {
                MediaForm gifForm = new MediaForm(Path.Combine(outputDir, $"output_{framerate}.gif"), currentScreenIndex);
                this.Hide();
                gifForm.Closed += (s, args) => this.Close();
                gifForm.Show();
            }));
        });
    }

    private void InitializeComponent()
    {
        Console.WriteLine("GIF Creator Opened");
        this.Icon = new Icon("Resources\\BS_Logo.ico");
        this.Text = "GIF Creator";
        this.StartPosition = FormStartPosition.Manual;
        this.FormBorderStyle = FormBorderStyle.None;
        int titleBarHeight = SystemInformation.CaptionHeight;
        Console.WriteLine(titleBarHeight);
        this.BackColor = Color.LimeGreen;
        this.TransparencyKey = Color.LimeGreen;
        this.ResizeRedraw = true;
        this.DoubleBuffered = true;

        //Tool Strip Menu Items (Buttons)
        playItemButton = Image.FromFile("Resources\\Button_Images\\Play_Button.png");
        pauseItemButton = Image.FromFile("Resources\\Button_Images\\Pause_Button.png");
        redoItemButton = Image.FromFile("Resources\\Button_Images\\Redo_Button.png");

        playItem = new ToolStripMenuItem(playItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Create New Snip"
        };

        pauseItem = new ToolStripMenuItem(pauseItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Change Mode"
        };

        redoItem = new ToolStripMenuItem(redoItemButton)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Save Screenshot"
        };

        progressBar = new ProgressBar()
        {
            Location = new Point(borderSize / 2, gifArea.Bottom + 1 + borderSize / 2),
            Width = gifArea.Width,
            Maximum = 100,
            Value = 0,
            Style = ProgressBarStyle.Continuous
        };



        this.Controls.Add(progressBar); 

        //Adding gif creator bar
        gifCreatorBar = new MenuStrip();
        gifCreatorBar.Items.AddRange(new ToolStripMenuItem[] { playItem, pauseItem, redoItem });
        gifCreatorBar.BackColor = Color.Gray;
        gifCreatorBar.Dock = DockStyle.None;  // Disable automatic docking
        gifCreatorBar.Location = new Point(borderSize / 2, progressBar.Bottom + borderSize / 2);
        gifCreatorBar.Width = 500;
        gifCreatorBar.Height = gifCreatorBarHeight;

        this.Controls.Add(gifCreatorBar);

        if (gifArea.Width < gifCreatorBar.Width)
        {
            progressBar.Width = gifCreatorBar.Width;
        }

        //Attach Event Handlers
        playItem.Click += playItem_Click;
        pauseItem.Click += pauseItem_Click;
        redoItem.Click += redoItem_Click;

        //THE SIZE OF THE Winform
        this.Size = new Size(
            gifArea.Width + 5 + gifCreatorBar.Width, // Width of the red square
            gifArea.Height + progressBar.Height + gifCreatorBar.Height // Height of the red square plus the menu strip height
        );
    }
    private void playItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("playItem");
    }

    private void pauseItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("pauseItem");
    }
    private void redoItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("redoItem");
    }


    //Draws the border around the selected area
    private void DrawCustomBorder(object sender, PaintEventArgs e)
    {
        using (Pen redPen = new Pen(Color.Red, borderSize))
        {
            e.Graphics.DrawRectangle(redPen, gifArea);
        }
    }



    //Deletes all contents from temp folder
    private void ClearTemp(string tempDir)
    {
        Array.ForEach(Directory.GetFiles(tempDir), File.Delete);
    }

    //Finds the aspect ratio of screenshot taken by user then finds a LOW, MEDIUM, or High resolution to auto scale to
    private (int, int) obtainResizeResolution(int width, int height)
    {
        Console.WriteLine($"WIDTH: {width}, HEIGHT: {height}");
        float aspectRatio = (float)width / height;
        float closestRatio = 10000;
        int ratioIndex = 0;
        var aspectRatios = new List<(string Name, float Ratio, List<(int Width, int Height)> Resolutions)>
        {
            ("9:16", 9f / 16f, new List<(int, int)> { (360, 640), (720, 1280), (1080, 1920) }),
            ("5:4", 5f / 4f, new List<(int, int)> { (320, 256), (1280, 1024), (1600, 1280) }),
            ("4:3", 4f / 3f, new List<(int, int)> { (640, 480), (800, 600), (1024, 768) }),
            ("3:2", 3f / 2f, new List<(int, int)> { (960, 640), (1440, 960), (2160, 1440) }),
            ("1:1", 1f, new List<(int, int)> { (240, 240), (480, 480), (1080, 1080) }),
            ("16:9", 16f / 9f, new List<(int, int)> { (640, 360), (1280, 720), (1920, 1080) }),
            ("18:9", 18f / 9f, new List<(int, int)> { (720, 360), (1440, 720), (2160, 1080) }),
            ("21:9", 21f / 9f, new List<(int, int)> { (1280, 540), (2560, 1080), (3440, 1440) })
        };

        //Finding the closest aspect ratio
        for(int i = 0; i < aspectRatios.Count; i++)
        {
            float difference = Math.Abs(aspectRatio - aspectRatios[i].Ratio);
            if (difference < closestRatio)
            {
                closestRatio = difference;
                ratioIndex = i;
            }
        }

        return aspectRatios[ratioIndex].Resolutions[0];
    }

    //Creates resized screenshots for GIF creation
    private void CreateGIFScreenshots(int FPS, int seconds, string tempDir, System.Drawing.Rectangle selectedArea, int resizeWidth, int resizeHeight)
    {
        int totalIterations = seconds * FPS;
        int delay = 1000 / FPS;

        //Checks temp directory and if doesn't exists creates it
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }
        ClearTemp(tempDir);

        using (Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height, PixelFormat.Format24bppRgb))
        using (Bitmap resizedScreenshot = new Bitmap(resizeWidth, resizeHeight, PixelFormat.Format24bppRgb))
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < totalIterations; i++)
            {
                long frameStartTime = stopwatch.ElapsedMilliseconds;
                //Updates progress bar every 10 iterations
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Taking Screenshots {i+1}/{totalIterations}");
                    this.Invoke((Action)(() =>
                    {
                        progressBar.Value = (i * progressBar.Maximum) / totalIterations;
                        progressBar.Refresh();
                    }));
                }

                Screen selectedScreen = Screen.FromPoint(new System.Drawing.Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(new System.Drawing.Point(selectedArea.Left +
                        selectedScreen.Bounds.Left, selectedArea.Top +
                        selectedScreen.Bounds.Top), System.Drawing.Point.Empty, selectedArea.Size);
                }

                using (Graphics gResized = Graphics.FromImage(resizedScreenshot))
                {
                    //gResized.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    gResized.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    gResized.DrawImage(screenshot, 0, 0, resizeWidth, resizeHeight);
                }

                string filePath = Path.Combine(tempDir, $"screenshot_{i:D4}.png");
                resizedScreenshot.Save(filePath, ImageFormat.Png);

                //Timing for next frame
                long frameEndTime = stopwatch.ElapsedMilliseconds;
                long elapsedFrameTime = frameEndTime - frameStartTime;
                long sleepTime = delay - elapsedFrameTime;
                Console.WriteLine(sleepTime);

                if (sleepTime > 0)
                {
                    Thread.Sleep((int)sleepTime); // Sleep only for the necessary time
                }
            }
        }
    }

    public new void Dispose()
    {
        Dispose(true);
        // Suppress finalization since resources have been cleaned up
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Unsubscribe event handlers
            //this.FormClosing -= MainForm_FormClosing;
            //this.KeyDown -= Form_KeyDown;
            playItem.Click -= playItem_Click;
            pauseItem.Click -= pauseItem_Click;
            redoItem.Click -= redoItem_Click;
            // Dispose of controls and resources
            playItemButton?.Dispose();
            pauseItemButton?.Dispose();
            redoItemButton?.Dispose();

            progressBar?.Dispose();
            gifCreatorBar?.Dispose();
        }

        base.Dispose(disposing);
    }


    ~GifCreatorForm()
    {
        Dispose(false);
    }
}
