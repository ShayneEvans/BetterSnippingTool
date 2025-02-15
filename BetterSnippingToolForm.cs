using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BetterSnippingTool.Utilities;

namespace BetterSnippingTool.Forms
{
    // BetterSnippingTool form
    public class BetterSnippingToolForm : Form
    {
        //Current Screen Index used to switch between multiple displays with arrows keys, temp solution until find out how to highlight all windows (might be imp)
        private int currentScreenIndex;
        private bool isDragging = false;
        private System.Drawing.Point startPoint;
        private System.Drawing.Rectangle selectedArea;
        private bool isGIFMode = false;
        private Font textFont = new Font("Arial", 20, FontStyle.Bold); 

        public BetterSnippingToolForm(bool isGIFMode)
        {
            this.isGIFMode = isGIFMode;
            //Obtaining the current screen index
            currentScreenIndex = getCurrentScreenIndex();
            this.Icon = new Icon(FileUtilities.ButtonImagePaths["BS_Logo"]);
            InitializeComponent();
            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
			this.Bounds = Screen.AllScreens[currentScreenIndex].Bounds;
		}

        private void InitializeComponent()
        {
			this.StartPosition = FormStartPosition.Manual;
			this.BackColor = System.Drawing.Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Cursor = CursorUtilities.LoadCustomCursor(FileUtilities.ButtonImagePaths["green_crosshair"]);
            this.Opacity = 0.40;
            this.TopMost = true;
            this.DoubleBuffered = false;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        //Function used to obtain the current screen index so the form can open on whichever screen the user's cursor is on.
        private int getCurrentScreenIndex()
        {
			Screen currentScreen = Screen.FromPoint(Cursor.Position);
			int screenIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
			return screenIndex;
		}

        //On left click set isDragging to true
        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = e.Location;
                this.Invalidate();
            }
        }

        //While mouse is moving and left click is held down, create rectangle around dragged location
        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int width = Math.Abs(e.X - startPoint.X);
                int height = Math.Abs(e.Y - startPoint.Y);
                selectedArea = new System.Drawing.Rectangle(
                    Math.Min(e.X, startPoint.X),
                    Math.Min(e.Y, startPoint.Y),
                    width,
                    height);
            }

            Invalidate();
        }

        //On mouse up either open gif creator form or take screenshot an open snip media form
        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                if (selectedArea.Width > 0 && selectedArea.Height > 0)
                {
                    //Open gif creator
                    if (isGIFMode)
                    {
                        this.Visible = false;
                        using (GifCreatorForm gifCreatorForm = new GifCreatorForm(selectedArea, currentScreenIndex))
                        {
                            gifCreatorForm.FormClosed += (s, args) => this.Close();
                            gifCreatorForm.ShowDialog();
                        }
                    }

                    //Take screenshot and open media form
                    else
                    {
                        this.Opacity = 0;
                        TakeScreenshotOfSelectedArea();
                    }
                }
            }
        }

        //Changes mode snip or gif text at top
        private void drawModeText(Graphics g)
        {
            Screen selectedScreen = Screen.AllScreens[currentScreenIndex];
            string currentMode = isGIFMode ? "GIF" : "Snip";
            string otherMode = isGIFMode ? "Snip" : "GIF";

            //Create the text to display
            string displayText = $"Mode: {currentMode}, to switch to {otherMode} press 'g'.";

            //Draw the text on the form
            g.DrawString(displayText, textFont, new SolidBrush(System.Drawing.Color.Black),
                    new System.Drawing.PointF((selectedScreen.Bounds.Width - g.MeasureString(displayText, textFont).Width) / 2, 20));

        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }

            else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                MoveToNextScreen();
            }
            else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                MoveToPreviousScreen();
            }

            else if (e.KeyCode == Keys.G)
            {
                isGIFMode = !isGIFMode;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            drawModeText(e.Graphics);
            base.OnPaint(e);
            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
            if (isDragging && selectedArea != System.Drawing.Rectangle.Empty)
            {
                using (Pen pen = new Pen(System.Drawing.Color.Red, 3))
                {
                    e.Graphics.DrawRectangle(pen, selectedArea);
                }
            }
        }

        //Screenshots selected area and opens new screenshot media form
        private void TakeScreenshotOfSelectedArea()
        {
            using (Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height, PixelFormat.Format24bppRgb))
            {
                Screen selectedScreen = Screen.FromPoint(new System.Drawing.Point(selectedArea.Left + this.Left, selectedArea.Top + this.Top));

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(new System.Drawing.Point(selectedArea.Left +
                        selectedScreen.Bounds.Left, selectedArea.Top +
                        selectedScreen.Bounds.Top), System.Drawing.Point.Empty, selectedArea.Size);
                }

                // Hide the current form
                MediaForm screenshotForm = new MediaForm(screenshot, currentScreenIndex);
                this.Hide();
                screenshotForm.Closed += (s, args) => this.Close();
                screenshotForm.Show();
            }
        }

        //Method to move the form to the next screen
        private void MoveToNextScreen()
        {
            currentScreenIndex = (currentScreenIndex + 1) % Screen.AllScreens.Length;
            this.Bounds = Screen.AllScreens[currentScreenIndex].Bounds;
        }

        //Method to move the form to the previous screen
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
}