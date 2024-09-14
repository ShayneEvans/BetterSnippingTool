using System;
using System.Collections.Generic;
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
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await Task.Run(() => CreateGIFScreenshots(24, 5, tempDir, selectedArea));

            await Task.Run(() =>
            {
                FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
                $"-framerate 25 -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -vf \"palettegen\" -y \"{Path.Combine(outputDir, "palette.png")}\"");
                FFmpeg.run_command("E:\\Visual Studio\\SnippingToolClone\\ffmpeg\\ffmpeg.exe",
                $"-framerate 25 -i \"{Path.Combine(tempDir, "screenshot_%04d.png")}\" -i \"{Path.Combine(outputDir, "palette.png")}\" -filter_complex \"fps=25,format=rgba[p];[p][1:v]paletteuse\" -y \"{Path.Combine(outputDir, "output_25.gif")}\"");
            });


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"It took {elapsedMs} Ms");

            this.Invoke((Action)(() =>
            {
                MediaForm gifForm = new MediaForm(Path.Combine(outputDir, "output_25.gif"), currentScreenIndex);
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

    //Function that creates TEMP image folder for GIF creation
    private void CreateGIFScreenshots(int FPS, int seconds, string tempDir, System.Drawing.Rectangle selectedArea)
    {
        int totalIterations = seconds * FPS;
        //Creating folder in temp if it does not exist already
        //Directory location: {DEFAULT_DRIVE}:\Users\{USER_NAME}\AppData\Local\Temp\BetterSnippingTool_GIF_Screenshots
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        ClearTemp(tempDir);
        int delay = 1000 / FPS;

        for (int i = 0; i < totalIterations; i++)
        {
            this.Invoke((Action)(() =>
            {
                progressBar.Value = (i * progressBar.Maximum) / totalIterations;
                progressBar.Refresh();
            }));
            using (Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height))
            {
                Screen selectedScreen = Screen.FromPoint(new System.Drawing.Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(new System.Drawing.Point(selectedArea.Left +
                        selectedScreen.Bounds.Left, selectedArea.Top +
                        selectedScreen.Bounds.Top), System.Drawing.Point.Empty, selectedArea.Size);
                }

                string filePath = Path.Combine(tempDir, $"screenshot_{i:D4}.png");
                screenshot.Save(filePath, ImageFormat.Png);
            }
            System.Threading.Thread.Sleep(delay);
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
