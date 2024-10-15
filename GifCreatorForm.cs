using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    private MenuStrip gifCreatorButtons;
    private ToolStripMenuItem playItem;
    private ToolStripMenuItem pauseItem;
    private ToolStripMenuItem stopItem;
    private ToolStripMenuItem redoItem;
    private ToolStripMenuItem settingsItem;
    private ToolStripMenuItem exitItem;
    private Image playItemButton;
    private Image pauseItemButton;
    private Image stopItemButton;
    private Image redoItemButton;
    private Image settingsItemButton;
    private Image exitItemButton;
    private Rectangle gifArea;
    private BackgroundWorker backgroundWorker;
    private int titleBarHeight;
    private string tempDir;
    private string outputDir;
    private bool _isPaused = false;
    private bool _isStopped = false;
    private int currentScreenIndex;
    private int framerate;
    private int gifSeconds;
    private (int, int) gifDimensions;
    public GifCreatorForm(string tempDir, string outputDir, System.Drawing.Rectangle selectedArea, int currentScreenIndex)
    {
        this.titleBarHeight = SystemInformation.CaptionHeight;
        this.selectedArea = selectedArea;
        this.outputDir = outputDir;
        this.tempDir = tempDir;
        this.currentScreenIndex = currentScreenIndex;
        //borderSize / 2 to avoid red lines in gif output
        this.gifArea = new Rectangle(borderSize / 2, borderSize / 2, selectedArea.Width + borderSize, selectedArea.Height + borderSize);
        InitializeComponent();

        backgroundWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        backgroundWorker.DoWork += backgroundWorker_DoWork;
        this.StartPosition = FormStartPosition.Manual;
        Screen selectedScreen = Screen.AllScreens[currentScreenIndex];
        this.Location = new System.Drawing.Point(
            selectedArea.X + selectedScreen.Bounds.X - borderSize - 8,
            selectedArea.Y + selectedScreen.Bounds.Y - borderSize - titleBarHeight - 8
        );

        this.Paint += new PaintEventHandler(DrawCustomBorder);
    }

    private void InitializeComponent()
    {
        //Loading in configuration variables
        this.framerate = AppConfig.Instance.FPS;
        this.gifSeconds = AppConfig.Instance.Seconds;
        this.gifDimensions = AppConfig.Instance.gifOutputResolution;
        Console.WriteLine(this.gifDimensions);

        Console.WriteLine("GIF Creator Opened");
        this.Icon = new Icon("Resources\\BS_Logo.ico");
        this.Text = "GIF Creator";
        this.StartPosition = FormStartPosition.Manual;
        //this.FormBorderStyle = FormBorderStyle.None;
        this.ControlBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.BackColor = Color.LimeGreen;
        this.TransparencyKey = Color.LimeGreen;
        this.ResizeRedraw = true;
        this.DoubleBuffered = true;

        //Tool Strip Menu Items (Buttons)
        playItemButton = Image.FromFile("Resources\\Button_Images\\Play_Button.png");
        pauseItemButton = Image.FromFile("Resources\\Button_Images\\Pause_Button.png");
        stopItemButton = Image.FromFile("Resources\\Button_Images\\Stop_Button.png");
        redoItemButton = Image.FromFile("Resources\\Button_Images\\Redo_Button.png");
        settingsItemButton = Image.FromFile("Resources\\Button_Images\\Settings_Button.png");
        exitItemButton = Image.FromFile("Resources\\Button_Images\\Exit_Button.png");

        playItem = new ToolStripMenuItem(playItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Create New GIF"
        };

        pauseItem = new ToolStripMenuItem(pauseItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Pause GIF Creation"
        };

        stopItem = new ToolStripMenuItem(stopItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Stop Image Processing, Begin GIF Creation"
        };

        redoItem = new ToolStripMenuItem(redoItemButton)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Save GIF"
        };

        settingsItem = new ToolStripMenuItem(settingsItemButton)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Save Screenshot"
        };

        exitItem = new ToolStripMenuItem(exitItemButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Exit Program"
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
        gifCreatorButtons = new MenuStrip();
        gifCreatorButtons.Items.AddRange(new ToolStripMenuItem[] { playItem, pauseItem, stopItem, redoItem, settingsItem, exitItem });
        (int totalGifCreatorButtonsWidth, int totalGifCreatorButtonsHeight) = caculateCreatorButtonsSize();
        gifCreatorButtons.Width = totalGifCreatorButtonsWidth;
        gifCreatorButtons.Height = totalGifCreatorButtonsHeight;
        gifCreatorButtons.BackColor = Color.Gray;
        gifCreatorButtons.Dock = DockStyle.None;  // Disable automatic docking
        gifCreatorButtons.Padding = new Padding(0);
        gifCreatorButtons.Margin = new Padding(0);
        this.Controls.Add(gifCreatorButtons);

        //Attach Event Handlers
        playItem.Click += playItem_Click;
        pauseItem.Click += pauseItem_Click;
        stopItem.Click += stopItem_Click;
        redoItem.Click += redoItem_Click;
        settingsItem.Click += settingsItem_Click;
        exitItem.Click += exitItem_Click;
        this.KeyDown += new KeyEventHandler(Form_KeyDown);
        this.LocationChanged += MyForm_LocationChanged;

        //Size and positioning of form, gif area, progress bar, and creator bar

        //If the gif screenshot area width is larger than the creator buttons then the buttons should be centered to the progress bar
        if (gifArea.Width > gifCreatorButtons.Width)
        {
            //Center according to progressBar
            gifCreatorButtons.Location = new Point((progressBar.Width - gifCreatorButtons.Width) / 2, progressBar.Bottom + borderSize / 2);
        }
        //Set the progress bar width to the gif creator buttons width
        else if(gifArea.Width < gifCreatorButtons.Width)
        {
            progressBar.Width = gifCreatorButtons.Width;
            gifCreatorButtons.Location = new Point(progressBar.Left, progressBar.Bottom + borderSize / 2);
        }

        int totalWidth = Math.Max(gifCreatorButtons.Width, gifArea.Width);
        int totalHeight = gifArea.Height + progressBar.Height + gifCreatorButtons.Height;
        this.Size = new Size(
            totalWidth + (2 * borderSize) + 10,  // Include left and right borders
            totalHeight + titleBarHeight + (2 * borderSize) + 10 // Include top and bottom borders plus title bar
        );
    }

    private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;
        (int, int) resizeResolution = (selectedArea.Width, selectedArea.Height);
        int framerate = AppConfig.Instance.FPS;
        int seconds = AppConfig.Instance.Seconds;
        if (AppConfig.Instance.OptimizeGifCreation == true)
        {
            resizeResolution = obtainResizeResolution(selectedArea.Width, selectedArea.Height);
        }
        else
        {
            resizeResolution = AppConfig.Instance.gifOutputResolution;
        }


        Console.WriteLine("Processing screenshots into GIF");
        Console.WriteLine($"SETTINGS:\nFPS:{framerate}\nSeconds:{gifSeconds}\nOutput Resolution:{resizeResolution}");
        CreateGIFScreenshots(framerate, seconds, tempDir, selectedArea, resizeResolution.Item1, resizeResolution.Item2, worker, e);
        FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
        $"-framerate {framerate} -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -vf \"palettegen=max_colors=256:reserve_transparent=0\" -y \"{Path.Combine(outputDir, "palette.png")}\"");
        FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
        $"-framerate {framerate} -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -i \"{Path.Combine(outputDir, "palette.png")}\" -filter_complex \"fps={framerate},format=rgba,paletteuse=dither=sierra2_4a\" -y \"{Path.Combine(outputDir, $"output_{framerate}.gif")}\"");

        this.Invoke((Action)(() =>
        {
            MediaForm gifForm = new MediaForm(Path.Combine(outputDir, $"output_{framerate}.gif"), currentScreenIndex);
            this.Hide();
            gifForm.Closed += (s, args) => this.Close();
            gifForm.Show();
        }));
    }

    //Used to start work and resume
    private void playItem_Click(object? sender, EventArgs e)
    {
        //Unpause if paused
        if (_isPaused)
        {
            _isPaused = false;
        }

        Console.WriteLine("playItem");
        if (backgroundWorker.IsBusy != true)
        {
            //Start the asynchronous operation.
            backgroundWorker.RunWorkerAsync();
        }
    }

    private (int,int) caculateCreatorButtonsSize()
    {
        int totalWidth = 0;
        int totalHeight = 0;

        foreach (ToolStripItem item in gifCreatorButtons.Items)
        {
            // Get the size of the item
            Size itemSize = item.Size; // You can also use item.GetPreferredSize(new Size(int.MaxValue, int.MaxValue)) if you want a more accurate size

            // Accumulate the total width and height
            totalWidth += itemSize.Width;
            totalHeight = Math.Max(totalHeight, itemSize.Height); // Get the max height
        }

        return (totalWidth, totalHeight);
    }

    private void pauseItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("pauseItem");
        if (backgroundWorker.IsBusy == true)
        {
            _isPaused = true;
        }
    }

    private void stopItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("stopItem");
        if (backgroundWorker.IsBusy == true)
        {
            _isStopped = true;
        }
    }
    private void redoItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("redoItem");
        if (backgroundWorker.WorkerSupportsCancellation == true)
        {
            // Cancel the asynchronous operation.
            backgroundWorker.CancelAsync();
        }

        BetterSnippingTool newSnip = new BetterSnippingTool();
        this.Hide();
        newSnip.Closed += (s, args) => this.Close();
        newSnip.Show();
    }

    private void settingsItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("settingsItem");
        GifSettingsForm gifSettingsForm = new GifSettingsForm();
        gifSettingsForm.Show();
    }

    private void exitItem_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("exitItem");
        Environment.Exit(0);
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

        //If incoming resolution is larger than downscaled, return downscaled
        if((width * height) >= (aspectRatios[ratioIndex].Resolutions[0].Width * aspectRatios[ratioIndex].Resolutions[0].Height)) {
            return aspectRatios[ratioIndex].Resolutions[0];
        }

        return (width, height);
    }

    //Creates resized screenshots for GIF creation
    private void CreateGIFScreenshots(int FPS, int seconds, string tempDir, System.Drawing.Rectangle selectedArea, int resizeWidth, int resizeHeight, BackgroundWorker worker, DoWorkEventArgs e)
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
                //Pausing functionality
                while (_isPaused)
                {
                    //Check if cancellation was requested while paused
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    System.Threading.Thread.Sleep(100); // Pause loop, but allow UI responsiveness
                }

                if(_isStopped)
                {
                    break;
                }

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

    //Update selectedArea location when the form moves
    private void MyForm_LocationChanged(object sender, EventArgs e)
    {
        selectedArea.Location = new Point(this.Location.X + borderSize + 8, this.Location.Y + borderSize + titleBarHeight + 8);
    }

    private void Form_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Environment.Exit(0);
        }

        else if (e.KeyCode == Keys.D1)
        {
            playItem_Click(sender, e);
        }
        else if (e.KeyCode == Keys.D2)
        {
            pauseItem_Click(sender, e);
        }

        else if (e.KeyCode == Keys.D3)
        {
            stopItem_Click(sender, e);
        }

        else if (e.KeyCode == Keys.D4)
        {
            redoItem_Click(sender, e);
        }

        else if (e.KeyCode == Keys.D5)
        {
            settingsItem_Click(sender, e);
        }

        else if (e.KeyCode == Keys.D0)
        {
            exitItem_Click(sender, e);
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
            this.KeyDown -= Form_KeyDown;
            playItem.Click -= playItem_Click;
            pauseItem.Click -= pauseItem_Click;
            stopItem.Click -= stopItem_Click;
            redoItem.Click -= redoItem_Click;
            settingsItem.Click -= settingsItem_Click;
            exitItem.Click -= exitItem_Click;
            playItemButton?.Dispose();
            pauseItemButton?.Dispose();
            stopItemButton?.Dispose();
            redoItemButton?.Dispose();
            settingsItemButton?.Dispose();
            exitItemButton?.Dispose();
            progressBar?.Dispose();
            gifCreatorButtons?.Dispose();
        }

        base.Dispose(disposing);
    }


    ~GifCreatorForm()
    {
        Dispose(false);
    }
}