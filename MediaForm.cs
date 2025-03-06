//Screenshot or GIF opens in this form and you can edit and save to your system.
using System.Collections.Specialized;
using BetterSnippingTool.Config;
using BetterSnippingTool.Interop;
using BetterSnippingTool.Utilities;
using System.Text.RegularExpressions;
using BetterSnippingTool.Tools;

namespace BetterSnippingTool.Forms
{
    public class MediaForm : Form
    {
        private bool _disposed = false;
        private PictureBox pictureBox;
        private Panel panel;
        private int padding = 100;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenuItem;
        private MenuStrip submenuStrip;
        private ToolStripMenuItem newSnipItem;
        private ToolStripMenuItem newGifItem;
        private ToolStripMenuItem saveItem;
        private ToolStripSplitButton trimGifItem;
        private ToolStripSplitButton drawItem;
        private bool isDrawing = false;
        private bool isTrimming = false;
        private Point lastPoint;
        private Pen drawPen;
        private Cursor paintCursor = Cursors.Default;
        private bool inImagesBounds = false;
        private Color penColor;
        private int penSize;
        private Bitmap drawingBitmap;
        private Bitmap clonedBitmap;
        private Image paintButtonOff;
        private Image paintButtonOn;
        private Image trimButtonOff;
        private Image trimButtonOn;
        private Stack<Image> undoStack;
        private Stack<Image> redoStack;
        private Image gifImage;
        private string gifFilePath;
        private string trimStartDir;
        private string trimEndDir;
        private System.Windows.Forms.Label gifTrimStartLabel;
        private System.Windows.Forms.Label gifTrimEndLabel;
        public bool IsGifMode { get; private set; }
        public string PictureBoxImageLocation => pictureBox.ImageLocation;
        public Color CurrentPenColor => penColor;
        public int CurrentPenSize => penSize;
        public int UndoStackCount => undoStack.Count;
        public Image PictureBoxImage => pictureBox.Image;
        private StringCollection filePaths;

        //For Gifs
        public MediaForm(string gifFilePath, int monitorIndex)
        {
            this.Icon = new Icon(FileUtilities.ButtonImagePaths["BS_Logo"]);
            IsGifMode = true;
            this.gifFilePath = gifFilePath;

            //Copying GIF to clipboard
            filePaths = new StringCollection();
            filePaths.Add(gifFilePath);
            Clipboard.SetFileDropList(filePaths);

            InitializeComponent();
            gifImage = Image.FromFile(gifFilePath);
            pictureBox.Image = gifImage;
            CenterFormOnMonitor(monitorIndex);
        }

        //For screenshots
        public MediaForm(Bitmap bitmap, int monitorIndex)
        {
            this.Icon = new Icon(FileUtilities.ButtonImagePaths["BS_Logo"]);
            IsGifMode = false;
            InitializeComponent();

            //Initializing drawing pen
            penColor = ColorTranslator.FromHtml("#0cff00");
            penSize = 10;

            drawPen = new Pen(penColor, penSize)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };

            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null");

            //Copying the image to clipboard
            PInvoke_Clipboard.SetBitmapToClipboard(bitmap);

            //Create a clone of the bitmap to ensure the original can be disposed of
            drawingBitmap = new Bitmap(bitmap);
            undoStack = new Stack<Image>();
            redoStack = new Stack<Image>();
            clonedBitmap = new Bitmap(drawingBitmap);
            undoStack.Push(clonedBitmap);

            pictureBox.Image = clonedBitmap;
            CenterFormOnMonitor(monitorIndex);
        }

        private void InitializeComponent()
        {
            this.Text = "Better Snipping Tool";
            //Set the icon for the form

            //Primary Menu Strip
            menuStrip = new MenuStrip();
            this.MainMenuStrip = menuStrip;

            fileMenuItem = new ToolStripMenuItem("File");
            fileMenuItem.DropDownItems.Add("New Snip", null, New_Snip_Click);
            fileMenuItem.DropDownItems.Add("New GIF", null, New_GIF_Click);
            fileMenuItem.DropDownItems.Add("Set Default Directory", null, Set_Default_Directory_Click);
            fileMenuItem.DropDownItems.Add("Save As", null, Save_As_Click);
            fileMenuItem.DropDownItems.Add("Exit", null, Close_Program);

            menuStrip.Items.AddRange(new ToolStripMenuItem[] { fileMenuItem });

            //Controls
            this.Controls.Add(menuStrip);
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.TransparencyKey = default;
            this.Controls.Add(this.pictureBox);

            //Tool Strip Menu Items (Buttons)
            Image newSnipButton = Image.FromFile(FileUtilities.ButtonImagePaths["New_Snip_Button"]);
            Image newGifButton = Image.FromFile(FileUtilities.ButtonImagePaths["New_GIF_Button"]);
            Image saveButton = Image.FromFile(FileUtilities.ButtonImagePaths["Save_Button"]);
            Image trimButton = Image.FromFile(FileUtilities.ButtonImagePaths["Trim_Button"]);

            newSnipItem = new ToolStripMenuItem(newSnipButton)
            {
                ImageAlign = ContentAlignment.TopCenter,
                ImageScaling = ToolStripItemImageScaling.None,
                BackColor = Color.LightBlue,
                ToolTipText = "Create New Snip"
            };

            newGifItem = new ToolStripMenuItem(newGifButton)
            {
                ImageAlign = ContentAlignment.TopCenter,
                ImageScaling = ToolStripItemImageScaling.None,
                BackColor = Color.LightBlue,
                ToolTipText = "Create New GIF"
            };

            saveItem = new ToolStripMenuItem(saveButton)
            {
                ImageAlign = ContentAlignment.MiddleLeft,
                ImageScaling = ToolStripItemImageScaling.None,
                BackColor = Color.LightBlue,
                ToolTipText = "Save Screenshot"
            };

            //Non GIF Mode gets extra buttons for drawing
            if (!IsGifMode)
            {
                paintButtonOff = Image.FromFile(FileUtilities.ButtonImagePaths["Paint_Button_Off"]);
                paintButtonOn = Image.FromFile(FileUtilities.ButtonImagePaths["Paint_Button_On"]);
                drawItem = new ToolStripSplitButton(paintButtonOff)
                {
                    ImageAlign = ContentAlignment.MiddleLeft,
                    ImageScaling = ToolStripItemImageScaling.None,
                    BackColor = Color.LightBlue,
                    ToolTipText = "Draw on Screenshot"
                };

                submenuStrip = new MenuStrip();
                submenuStrip.Items.AddRange(new ToolStripItem[] { newSnipItem, newGifItem, saveItem, drawItem });
                drawItem.ButtonClick += Paint_Click;
                drawItem.DropDownItems.Add("Choose Color", null, Choose_Color);
                drawItem.DropDownItems.Add("Choose Pen Size", null, Choose_Pen_Size);
                drawItem.DropDownItems.Add("Toggle Paint        Ctrl + P", null, Paint_Click);
                drawItem.DropDownItems.Add("Undo                    Ctrl + Z", null, Undo_Paint_Click);
                drawItem.DropDownItems.Add("Redo                     Ctrl + Y", null, Redo_Paint_Click);
                drawItem.DropDownButtonWidth = 30;

                pictureBox.MouseDown += new MouseEventHandler(paint_MouseDown);
                pictureBox.MouseMove += new MouseEventHandler(paint_MouseMove);
                pictureBox.MouseUp += new MouseEventHandler(paint_MouseUp);
            }
            //GIF Mode
            else
            {
                submenuStrip = new MenuStrip();
                trimButtonOff = Image.FromFile(FileUtilities.ButtonImagePaths["Trim_Button_Off"]);
                trimButtonOn = Image.FromFile(FileUtilities.ButtonImagePaths["Trim_Button_On"]);
                trimGifItem = new ToolStripSplitButton(trimButtonOff)
                {
                    ImageAlign = ContentAlignment.MiddleLeft,
                    ImageScaling = ToolStripItemImageScaling.None,
                    BackColor = Color.LightBlue,
                    ToolTipText = "TrimGif"
                };

                gifTrimStartLabel = new Label
                {
                    Location = new Point(460, 30),
                    AutoSize = true,
                    Text = "GIF Starting Frame:",
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Visible = false
                };

                gifTrimEndLabel = new Label
                {
                    Location = new Point(460, 50),
                    AutoSize = true,
                    Text = "GIF Ending Frame:",
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Visible = false
                };

                submenuStrip.Items.AddRange(new ToolStripItem[] { newGifItem, newSnipItem, saveItem, trimGifItem });

                trimGifItem.ButtonClick += Trim_GIF_Click;
                trimGifItem.DropDownItems.Add("Choose Starting Frame", null, Select_Trim_Start_Frame);
                trimGifItem.DropDownItems.Add("Choose Ending Frame", null, Select_Trim_End_Frame);
                trimGifItem.DropDownItems.Add("Trim GIF", null, Trim_GIF);
                trimGifItem.DropDown.Enabled = false;
                this.Controls.Add(gifTrimStartLabel);
                this.Controls.Add(gifTrimEndLabel);
            }

            submenuStrip.BackColor = Color.Gray;
            submenuStrip.Dock = DockStyle.None;
            submenuStrip.Location = new Point(0, menuStrip.Height);
            this.Controls.Add(submenuStrip);

            //Attach Event Handlers
            newSnipItem.Click += New_Snip_Click;
            newGifItem.Click += New_GIF_Click;
            saveItem.Click += Save_As_Click;
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            this.KeyDown += new KeyEventHandler(Form_KeyDown);

            panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
            };

            //Add the PictureBox to the Panel
            panel.Controls.Add(pictureBox);
            pictureBox.Location = new Point((panel.ClientSize.Width - pictureBox.Width) / 2, (panel.ClientSize.Height - pictureBox.Height) / 2);
            pictureBox.Padding = new Padding(padding);
            this.pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            UpdatePictureBoxLocation();
            panel.Resize += (sender, e) => UpdatePictureBoxLocation();
            this.Controls.Add(panel);
        }

        private void UpdatePictureBoxLocation()
        {
            //Calculate total padding
            int paddingX = panel.Padding.Left + panel.Padding.Right;
            int paddingY = panel.Padding.Top + panel.Padding.Bottom;

            //Calculate new location, making sure it doesn't go negative (out of bounds)
            int newX = Math.Max(panel.Padding.Left, (panel.ClientSize.Width - paddingX - pictureBox.Width) / 2);
            int newY = Math.Max(panel.Padding.Top, (panel.ClientSize.Height - paddingY - pictureBox.Height) / 2);

            //Apply the new location to the PictureBox
            pictureBox.Location = new Point(newX, newY);
        }

        private void New_Snip_Click(object? sender, EventArgs e)
        {
            this.Dispose();
            BetterSnippingToolForm newSnip = new BetterSnippingToolForm(false);
            this.Hide();
            newSnip.Closed += (s, args) => this.Close();
            newSnip.Show();
        }

        private void New_GIF_Click(object? sender, EventArgs e)
        {
            this.Dispose();
            BetterSnippingToolForm newGif = new BetterSnippingToolForm(true);
            this.Hide();
            newGif.Closed += (s, args) => this.Close();
            newGif.Show();
        }

        private void Save_As_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                //Use the saved directory from configuration if available
                if (!string.IsNullOrWhiteSpace(AppConfig.Instance.DefaultDirectory))
                {
                    saveFileDialog.InitialDirectory = AppConfig.Instance.DefaultDirectory;
                }

                if (IsGifMode)
                {
                    saveFileDialog.Title = "Save As";
                    saveFileDialog.Filter = "GIF Files (*.gif)|*.gif|All Files (*.*)|*.*";
                    saveFileDialog.DefaultExt = "gif";
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.AutoUpgradeEnabled = true;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Move file to the new directory with new name and delete the original
                        gifImage.Save(saveFileDialog.FileName);
                    }
                }

                else
                {
                    saveFileDialog.Title = "Save As";
                    saveFileDialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|Bitmap Files (*.bmp)|*.bmp";
                    saveFileDialog.AutoUpgradeEnabled = true;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        drawingBitmap.Save(saveFileDialog.FileName);
                    }
                }
            }
        }

        //Selecting the starting 
        private void Select_Trim_Start_Frame(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Starting Frame";
                openFileDialog.InitialDirectory = FileUtilities.GifTempScreenshotsDir;
                openFileDialog.Filter = "GIF Frames (frame_*)|frame_*";
                openFileDialog.ShowReadOnly = true;
                openFileDialog.ShowHelp = false;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.AutoUpgradeEnabled = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Verify that the selected file matches the pattern
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    if (Regex.IsMatch(fileName, @"^frame_(\d+)\.png$"))
                    {
                        this.trimStartDir = openFileDialog.FileName;
                        gifTrimStartLabel.Text = $"GIF Starting Frame: {Path.GetFileName(trimStartDir)}";
                    }
                    else
                    {
                        //Invalid file format selected
                        MessageBox.Show("Invalid file selected. Please select a file in the format frame_0, frame_1, etc.");
                    }
                }
            }
        }

        private void Select_Trim_End_Frame(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Ending Frame";
                openFileDialog.InitialDirectory = FileUtilities.GifTempScreenshotsDir;
                openFileDialog.Filter = "GIF Frames (frame_*)|frame_*";
                openFileDialog.ShowReadOnly = true;
                openFileDialog.ShowHelp = false;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.AutoUpgradeEnabled = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Verify that the selected file matches the pattern
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    if (Regex.IsMatch(fileName, @"^frame_(\d+)\.png$"))
                    {
                        this.trimEndDir = openFileDialog.FileName;
                        gifTrimEndLabel.Text = $"GIF Ending Frame: {Path.GetFileName(trimEndDir)}";
                    }
                    else
                    {
                        //Invalid file format selected
                        MessageBox.Show("Invalid file selected. Please select a file in the format format frame_0, frame_1, etc.");
                    }
                }
            }
        }

        private int getFrameNumber(string frameDir)
        {
            int frameNumber = int.Parse(Regex.Match(frameDir, @"\d+").Value);

            return frameNumber;
        }

        private void Trim_GIF(object? sender, EventArgs e)
        {
            int start_frame = getFrameNumber(Path.GetFileName(trimStartDir));
            int end_frame = getFrameNumber(Path.GetFileName(trimEndDir));
            Console.WriteLine(start_frame);
            Console.WriteLine(end_frame);
            string trimmed_output_path = Path.Combine(FileUtilities.GifTempDir, $"output_{AppConfig.Instance.FPS}_trimmed.gif");
			//User must select a starting and ending frame to trim
			if (File.Exists(trimmed_output_path))
			{
				File.Delete(trimmed_output_path);
			}
			if (trimStartDir != null && trimEndDir != null)
            {
                FFmpeg.run_command(FileUtilities.FFmpegDir,
                    $"-framerate {AppConfig.Instance.FPS} -start_number {start_frame} -i \"{Path.Combine(FileUtilities.GifTempScreenshotsDir, "frame_%d.png")}\" " +
                    $"-i \"{Path.Combine(FileUtilities.GifTempDir, "palette.png")}\" -filter_complex " +
                    $"\"fps={AppConfig.Instance.FPS},format=rgba,paletteuse=dither=sierra2_4a\" " +
                    $"-frames:v {end_frame - start_frame + 1} -loop 0 -an -y \"{trimmed_output_path}\"");

                gifFilePath = trimmed_output_path;
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }
                gifImage = Image.FromFile(gifFilePath);
                pictureBox.Image = gifImage;
                filePaths[0] = gifFilePath;
                Clipboard.SetFileDropList(filePaths);
            }
            else
            {
                MessageBox.Show("Please select a start and ending frame to trim the GIF.");
            }
        }

        private void Paint_Click(object? sender, EventArgs e)
        {
            //Toggle isDrawing
            isDrawing = !isDrawing;

            if (isDrawing)
            {
                paintCursor = CursorUtilities.CreatePaintCursor(penColor, penSize);
                this.Cursor = paintCursor;
                //Change the image on the paint button
                drawItem.Image = paintButtonOn;
            }

            else
            {
                if (paintCursor != null)
                {
                    paintCursor?.Dispose();
                }
                this.Cursor = Cursors.Default;

                drawItem.Image = paintButtonOff;
            }
        }

        private void Trim_GIF_Click(object? sender, EventArgs e)
        {
            isTrimming = !isTrimming;

            if (isTrimming)
            {
                trimGifItem.Image = trimButtonOn;
                gifTrimStartLabel.Visible = true;
                gifTrimEndLabel.Visible = true;
                trimGifItem.DropDown.Enabled = true;
            }

            else
            {
                trimGifItem.Image = trimButtonOff;
                gifTrimStartLabel.Visible = false;
                gifTrimEndLabel.Visible = false;
                trimGifItem.DropDown.Enabled = false;
            }
        }

        private void Choose_Color(object? sender, EventArgs e)
        {
            ColorDialog colorPicker = new ColorDialog();

            if (colorPicker.ShowDialog() == DialogResult.OK && isDrawing)
            {
                penColor = colorPicker.Color;
                drawPen.Color = penColor;
                paintCursor = CursorUtilities.CreatePaintCursor(penColor, penSize);
                this.Cursor = paintCursor;
            }
        }

        private void Choose_Pen_Size(object? sender, EventArgs e)
        {
            Form penSizeDialog = new Form
            {
                Text = "Choose Pen Size (2-100)",
                Size = new Size(250, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
            };

            penSizeDialog.StartPosition = FormStartPosition.CenterScreen;

            NumericUpDown numericUpDown = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 100,
                Value = penSize,
                Location = new Point(50, 20),
                Width = 150
            };

            Button okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(75, 60),
                Width = 100
            };


            penSizeDialog.AcceptButton = okButton;
            penSizeDialog.Controls.Add(numericUpDown);
            penSizeDialog.Controls.Add(okButton);

            if (penSizeDialog.ShowDialog() == DialogResult.OK)
            {
                penSize = (int)numericUpDown.Value;
                drawPen.Width = penSize;
                paintCursor = CursorUtilities.CreatePaintCursor(penColor, penSize);
                this.Cursor = paintCursor;
            }
        }

        private bool isInsideImage(Point clickLocation)
        {
            if (clickLocation.X >= padding && clickLocation.X <= pictureBox.Image.Size.Width + padding && clickLocation.Y >= padding && clickLocation.Y <= pictureBox.Image.Size.Height + padding)
            {
                return true;
            }

            return false;
        }

        private void paint_MouseDown(object? sender, MouseEventArgs e)
        {
            if (isInsideImage(e.Location))
            {
                this.inImagesBounds = true;
                if (e.Button == MouseButtons.Left && isDrawing)
                {
                    lastPoint = new Point(e.Location.X - padding, e.Location.Y - padding);
                }
            }
            else
            {
                this.inImagesBounds = false;
            }
        }

        private void paint_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing && inImagesBounds)
            {
                PInvoke_Clipboard.SetBitmapToClipboard(drawingBitmap);
                Bitmap clonedBitmap = new Bitmap(drawingBitmap);
                undoStack.Push(clonedBitmap);
            }
        }

        private void paint_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDrawing && e.Button == MouseButtons.Left && drawPen != null && inImagesBounds)
            {
                using (Graphics g = Graphics.FromImage(drawingBitmap))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.DrawLine(drawPen, lastPoint, new Point(e.Location.X - padding, e.Location.Y - padding));
                }
                lastPoint = new Point(e.Location.X - padding, e.Location.Y - padding);
                pictureBox.Image = drawingBitmap;
                pictureBox.Invalidate();
            }
        }

        private void Set_Default_Directory_Click(object? sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string directoryPath = fbd.SelectedPath;

                    //Update and save the configuration
                    AppConfig.Instance.DefaultDirectory = directoryPath;
                    AppConfig.Instance.SaveConfig(AppConfig.Instance.GetCurrentFileName());
                }
            }
        }

        private void Undo_Paint_Click(object? sender, EventArgs e)
        {
            //Making sure there is a undo on the stack
            if (undoStack.Count > 1)
            {
                //Create a copy of the current top of the undo stack for the redo stack
                Image topUndoImage = undoStack.Pop();
                redoStack.Push(new Bitmap(topUndoImage));

                //Dispose of the old bitmap if it exists
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }

                //Set the pictureBox to the new bitmap and update the drawingBitmap
                Image newUndoImage = undoStack.Peek();
                pictureBox.Image = new Bitmap(newUndoImage);
                drawingBitmap = (Bitmap)pictureBox.Image;

                //Copy the current image onto clipboard from newest undo
                PInvoke_Clipboard.SetBitmapToClipboard(drawingBitmap);

                //Dispose of the undo image and refresh picture box
                topUndoImage.Dispose();
                pictureBox.Invalidate();
            }
        }

        private void Redo_Paint_Click(object? sender, EventArgs e)
        {
            //Making sure there is a redo on the stack
            if (redoStack.Count > 0)
            {
                //Create a copy of the current top of the redo stack for the undo stack
                Image topRedoImage = redoStack.Pop();

                //Dispose of the old bitmap if it exists
                if (pictureBox.Image != null)
                    pictureBox.Image.Dispose();

                //Set the pictureBox to the new bitmap and update the drawingBitmap
                pictureBox.Image = new Bitmap(topRedoImage);
                drawingBitmap = (Bitmap)pictureBox.Image;

                //Push the popped redo image as bitmap to undo stack
                undoStack.Push(new Bitmap(topRedoImage));

                //Copy the current image onto clipboard from newest redo
                PInvoke_Clipboard.SetBitmapToClipboard(drawingBitmap);

                //Dispose of the redo image and refresh picture box
                topRedoImage.Dispose();
                pictureBox.Invalidate();
            }
        }

        private void CenterFormOnMonitor(int monitorIndex)
        {
            int screenWidth = Screen.AllScreens[monitorIndex].Bounds.Width;
            int screenHeight = Screen.AllScreens[monitorIndex].Bounds.Height;

            //Adjust the form size to include padding
            //This is to account for trim text fitting in without resize on smaller gifs
            if (pictureBox.Image.Width < 600) 
            {
                this.ClientSize = new Size((pictureBox.Image.Width + 550) + 2 * padding, pictureBox.Image.Height + 2 * padding);
            }
            else
            {
                this.ClientSize = new Size(pictureBox.Image.Width + 2 * padding, pictureBox.Image.Height + 2 * padding);
            }


            //Center the form on the primary monitor
            Rectangle screenBounds = Screen.AllScreens[monitorIndex].Bounds;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(screenBounds.X + (screenBounds.Width - this.Width) / 2, screenBounds.Y + (screenBounds.Height - this.Height) / 2);

            //Maximize winform if the snip dimensions are greater than or equal to 85% of monitor resolution
            if (this.Width * this.Height >= (screenWidth * screenHeight) * .85)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Environment.Exit(0);
            }

            else if (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control)
            {
                Undo_Paint_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Y && e.Modifiers == Keys.Control)
            {
                Redo_Paint_Click(sender, e);
            }

            else if (e.KeyCode == Keys.S && e.Modifiers == Keys.Control)
            {
                Save_As_Click(sender, e);
            }

            else if (e.KeyCode == Keys.N && e.Modifiers == Keys.Control)
            {
                New_Snip_Click(sender, e);
            }

            else if (e.KeyCode == Keys.P && e.Modifiers == Keys.Control)
            {
                Paint_Click(sender, e);
            }
        }
        private void MainForm_FormClosing(Object? sender, FormClosingEventArgs e)
        {
            if (IsGifMode)
            {
                //Dispose of gif inside of picturebox so it can be deleted from GIF folder
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }
                FileUtilities.ClearTemp();
                FileUtilities.ClearGifFolder(gifFilePath);
            }
            Environment.Exit(0);
        }

        private void Close_Program(object? sender, EventArgs e)
        {
            if (IsGifMode)
            {
                //Dispose of gif inside of picturebox so it can be deleted from GIF folder
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }
                FileUtilities.ClearTemp();
                FileUtilities.ClearGifFolder(gifFilePath);
            }
            Environment.Exit(0);
        }

        public new void Dispose()
        {
            Dispose(true);
            //Suppress finalization since resources have been cleaned up
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
                //Unsubscribe event handlers
                this.FormClosing -= MainForm_FormClosing;
                this.KeyDown -= Form_KeyDown;
                newSnipItem.Click -= New_Snip_Click;
                saveItem.Click -= Save_As_Click;


                if (!IsGifMode)
                {
                    //Dispose of undo and redo stacks
                    while (undoStack.Count > 0)
                    {
                        undoStack.Pop()?.Dispose();
                    }

                    while (redoStack.Count > 0)
                    {
                        redoStack.Pop()?.Dispose();
                    }

                    drawingBitmap?.Dispose();
                    clonedBitmap?.Dispose();
                    paintButtonOff?.Dispose();
                    paintButtonOn?.Dispose();
                    drawItem.ButtonClick -= Paint_Click;
                    drawItem?.Dispose();
                    drawPen?.Dispose();
                    paintCursor?.Dispose();
                }

                else
                {
                    //Dispose of gif inside of picturebox so it can be deleted from GIF folder
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                        pictureBox.Image = null;
                    }
                    FileUtilities.ClearTemp();
                    FileUtilities.ClearGifFolder(gifFilePath);
                    gifImage?.Dispose();
                    gifTrimEndLabel?.Dispose();
                    gifTrimStartLabel?.Dispose();
                    trimButtonOff?.Dispose();
                    trimButtonOn?.Dispose();
                    trimGifItem?.Dispose();
                }


                //Dispose of controls and resources
                pictureBox?.Image?.Dispose();
                pictureBox?.Dispose();
                panel?.Dispose();
                menuStrip?.Dispose();
                submenuStrip?.Dispose();
                fileMenuItem?.Dispose();
                newSnipItem?.Dispose();
                saveItem?.Dispose();
            }

            base.Dispose(disposing);
        }

        ~MediaForm()
        {
            Dispose(false);
        }
    }
}