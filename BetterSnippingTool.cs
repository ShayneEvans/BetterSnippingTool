using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.Collections.Generic;
using ImageMagick;
using System.Drawing; // For Bitmap
using System.Drawing.Imaging;

//BetterSnippingTool form
public class BetterSnippingTool : Form
{
    //Current Screen Index used to switch between multiple displays with arrows keys, temp solution until find out how to highlight all windows (might be imp)
    private int currentScreenIndex;
    private bool isDragging = false;
    private Point startPoint;
    private Rectangle selectedArea;
    private bool isGIFMode = false;
    public enum SnipMode
    {
        Rectangle,
        FreeForm,
        FullScreen
    }
    private SnipMode currentSnipMode;

    public BetterSnippingTool(SnipMode snipMode = SnipMode.Rectangle)
    {
        //Current Snipping Mode, Rectangle by default
        currentSnipMode = snipMode;

        //Obtaining the primary screen index
        currentScreenIndex = getPrimaryScreenIndex();

        //Winform attributes
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Cursor = CursorUtilities.LoadCustomCursor("Resources\\green_crosshair.png");
        this.Opacity = 0.50;
        this.TopMost = true;
        this.Bounds = GetAllScreensBounds();
        this.DoubleBuffered = false;
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

        //Event Handlers
        this.MouseDown += new MouseEventHandler(Form_MouseDown);
        this.MouseMove += new MouseEventHandler(Form_MouseMove);
        this.MouseUp += new MouseEventHandler(Form_MouseUp);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);
    }

    //Function used to obtain the index of the primary screen so that program always opens on it/displays screenshot to it without switch.
    private int getPrimaryScreenIndex()
    {
        for (int i = 0; i < Screen.AllScreens.Length; i++)
        {
            if (Screen.AllScreens[i].Primary)
            {
                return i;
            }
        }

        return 0;
    }

    //Returns the combined screen bounds of all displays
    private Rectangle GetAllScreensBounds()
    {
        Rectangle allScreensBounds = Screen.AllScreens[0].Bounds;
        foreach (Screen screen in Screen.AllScreens)
        {
            allScreensBounds = Rectangle.Union(allScreensBounds, screen.Bounds);
        }

        return allScreensBounds;
    }

    //On left click set isDragging to true
    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDragging = true;
            startPoint = e.Location;

            if (currentSnipMode == SnipMode.FreeForm) 
            {
                selectedArea = new Rectangle(startPoint.X, startPoint.Y, 0, 0);            
            }

            this.Invalidate();
        }
    }

    //While mouse is moving and left click is held down, create rectangle around dragged location
    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            switch (currentSnipMode)
            {
                case SnipMode.Rectangle:
                    int width = Math.Abs(e.X - startPoint.X);
                    int height = Math.Abs(e.Y - startPoint.Y);
                    selectedArea = new Rectangle(
                        Math.Min(e.X, startPoint.X),
                        Math.Min(e.Y, startPoint.Y),
                        width,
                        height);
                    break;
                
                case SnipMode.FreeForm:
                    //selectedArea = Rectangle.Union(selectedArea, new Rectangle(e.X, e.Y, 1, 1));
                    break;
            }

        }

        Invalidate();
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDragging = false;

            switch(currentSnipMode)
            {
                case SnipMode.FullScreen:
                {
                    this.Opacity = 0;
                    //TakeScreenshotOfFullScreen();
                    break;
                }
                
                default:
                {
                    if (selectedArea.Width > 0 && selectedArea.Height > 0)
                    {
                        if(isGIFMode)
                        {
                            List<Bitmap> screenshots = TakeScreenshotForGIF(24, 5);
                            createMagickGIF(screenshots, 24);
                        }

                        else
                        {
                            this.Opacity = 0;
                            TakeScreenshotOfSelectedArea();
                        }

                    }
                    break;
                }
            }
        }
    }

    private void Form_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }

        else if (e.KeyCode == Keys.Right)
        {
            MoveToNextScreen();
        }
        else if (e.KeyCode == Keys.Left)
        {
            MoveToPreviousScreen();
        }

        else if (e.KeyCode == Keys.G)
        {
            isGIFMode = true;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.CompositingMode = CompositingMode.SourceCopy;
        if (isDragging && selectedArea != Rectangle.Empty)
        {
            using (Pen pen = new Pen(Color.Red, 3))
            {
                e.Graphics.DrawRectangle(pen, selectedArea);
            }
        }
    }

    private void TakeScreenshotOfSelectedArea()
    {
        using (Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height))
        {
            Screen selectedScreen = Screen.FromPoint(new Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(new Point(selectedArea.Left + selectedScreen.Bounds.Left, selectedArea.Top + selectedScreen.Bounds.Top), Point.Empty, selectedArea.Size);
            }

            // Hide the current form
            ScreenshotForm screenshotForm = new ScreenshotForm(screenshot, currentScreenIndex);
            this.Hide();
            screenshotForm.Closed += (s, args) => this.Close();
            screenshotForm.Show();
        }
    }

    private List<Bitmap> TakeScreenshotForGIF(int FPS, int seconds)
    {
        int interval = 1000 / FPS;
        List<Bitmap> screenshots = new List<Bitmap>();

        for (int i = 0; i < seconds * FPS; i++)
        {
            Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height);
            Screen selectedScreen = Screen.FromPoint(new Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(new Point(selectedArea.Left + selectedScreen.Bounds.Left, selectedArea.Top + selectedScreen.Bounds.Top), Point.Empty, selectedArea.Size);
            }
            
            screenshots.Add(screenshot);
            Thread.Sleep(interval);
        }

        Console.WriteLine(screenshots.Count);

        return screenshots;
    }

    private void createMagickGIF(List<Bitmap> screenshots, int FPS)
    {
        using (var collection = new MagickImageCollection())
        {
            foreach (Bitmap screenshot in screenshots)
            {
                using (var stream = new MemoryStream())
                {
                    screenshot.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);

                    var magickImage = new MagickImage(stream);
                    collection.Add(magickImage);
                    collection[collection.Count - 1].AnimationDelay = 100 / FPS;
                }
            }
            collection.Coalesce(); // Ensures all frames are properly handled
            collection.Write("GIF\\test.gif");
        }
        this.Close();
    }




    // Method to move the form to the next screen
    private void MoveToNextScreen()
    {
        currentScreenIndex = (currentScreenIndex + 1) % Screen.AllScreens.Length;
        this.Bounds = Screen.AllScreens[currentScreenIndex].Bounds;
    }

    // Method to move the form to the previous screen
    private void MoveToPreviousScreen()
    {
        currentScreenIndex = (currentScreenIndex - 1 + Screen.AllScreens.Length) % Screen.AllScreens.Length;
        this.Bounds = Screen.AllScreens[currentScreenIndex].Bounds;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.MouseDown -= Form_MouseDown;
            this.MouseMove -= Form_MouseMove;
            this.MouseUp -= Form_MouseUp;
            this.KeyDown -= Form_KeyDown;
        }

        base.Dispose(disposing);
    }
}