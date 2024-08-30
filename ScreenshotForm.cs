//Screenshot form opened after a screenshot is taken. Modify and do stuff with screenshot as well as save to system
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.DataFormats;

public class ScreenshotForm : Form
{
    private bool _disposed = false;
    private PictureBox pictureBox;
    private Panel panel;
    private int padding = 200;
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenuItem;
    private ToolStripMenuItem editMenuItem;
    private ToolStripMenuItem prefsMenuItem;
    private MenuStrip submenuStrip;
    private ToolStripMenuItem newSnipItem;
    private ToolStripMenuItem modeItem;
    private ToolStripMenuItem saveItem;
    private ToolStripSplitButton drawItem;
    private ToolStripMenuItem eraseItem;
    private bool isDrawing = false;
    private Point lastPoint;
    private Pen drawPen;
    private Cursor paintCursor = Cursors.Default;
    private Color penColor;
    private int penSize;
    private Bitmap drawingBitmap;
    private Bitmap clonedBitmap;
    private Image paintButtonOff;
    private Image paintButtonOn;
    private Stack<Image> undoStack;
    private Stack<Image> redoStack;


    public ScreenshotForm(Bitmap bitmap, int monitorIndex)
    {
        //Initializing drawing pen
        penColor = ColorTranslator.FromHtml("#0cff00");
        penSize = 20;
        
        drawPen = new Pen(penColor, penSize)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };

        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null");

        //Copying the image to clipboard
        PInvoke_Clipboard.SetBitmapToClipboard(bitmap);

        //Set the icon for the form
        this.Icon = new Icon("Resources\\BS_Logo.ico");

        //Create a clone of the bitmap to ensure the original can be disposed of
        drawingBitmap = new Bitmap(bitmap);
        undoStack = new Stack<Image>();
       
        redoStack = new Stack<Image>();
        clonedBitmap = new Bitmap(drawingBitmap);
        undoStack.Push(clonedBitmap);


        //Primary Menu Strip
        menuStrip = new MenuStrip();
        this.MainMenuStrip = menuStrip;

        fileMenuItem = new ToolStripMenuItem("File");
        fileMenuItem.DropDownItems.Add("New Snip", null, New_Snip_Click);
        fileMenuItem.DropDownItems.Add("Save As", null, Save_As_Click);
        fileMenuItem.DropDownItems.Add("Set Default Directory", null, Set_Default_Directory_Click);
        fileMenuItem.DropDownItems.Add("Exit", null, Close_Program);

        editMenuItem = new ToolStripMenuItem("Edit");
        prefsMenuItem = new ToolStripMenuItem("Preferences");

        menuStrip.Items.AddRange(new ToolStripMenuItem[] { fileMenuItem, editMenuItem, prefsMenuItem });

        //Controls
        this.Controls.Add(menuStrip);

        //Tool Strip Menu Items (Buttons)
        Image newSnipButton = Image.FromFile("Resources\\Button_Images\\New_Snip_Button.png");
        Image modeButton = Image.FromFile("Resources\\Button_Images\\Mode_Button.png");
        Image saveButton = Image.FromFile("Resources\\Button_Images\\Save_Button.png");
        paintButtonOff = Image.FromFile("Resources\\Button_Images\\Paint_Button_Off.png");
        paintButtonOn = Image.FromFile("Resources\\Button_Images\\Paint_Button_On.png");
        Image eraseButton = Image.FromFile("Resources\\Button_Images\\Erase_Button.png");

        newSnipItem = new ToolStripMenuItem(newSnipButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Create New Snip"
        };

        modeItem = new ToolStripMenuItem(modeButton)
        {
            ImageAlign = ContentAlignment.TopCenter,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Change Mode"
        };

        saveItem = new ToolStripMenuItem(saveButton)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Save Screenshot"
        };

        drawItem = new ToolStripSplitButton(paintButtonOff)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Draw on Screenshot"
        };
        drawItem.DropDownItems.Add("Choose Color", null, Choose_Color);
        drawItem.DropDownItems.Add("Choose Pen Size", null, Choose_Pen_Size);
        drawItem.DropDownItems.Add("Toggle Paint        Ctrl + P", null, Paint_Click);
        drawItem.DropDownItems.Add("Undo                    Ctrl + Z", null, Undo_Paint_Click);
        drawItem.DropDownItems.Add("Redo                     Ctrl + Y", null, Redo_Paint_Click);
        drawItem.DropDownButtonWidth = 30;


        eraseItem = new ToolStripMenuItem(eraseButton)
        {
            ImageAlign = ContentAlignment.MiddleLeft,
            ImageScaling = ToolStripItemImageScaling.None,
            BackColor = Color.LightBlue,
            ToolTipText = "Erase Drawing"
        };

        //Secondary Menu Strip
        submenuStrip = new MenuStrip();
        submenuStrip.Items.AddRange(new ToolStripItem[] { newSnipItem, modeItem, saveItem, drawItem, eraseItem });
        submenuStrip.BackColor = Color.Gray;
        submenuStrip.Dock = DockStyle.None; // Disable default docking
        submenuStrip.Location = new Point(0, menuStrip.Height); // Set custom location

        this.Controls.Add(submenuStrip);

        //Attach Event Handlers
        newSnipItem.Click += New_Snip_Click;
        modeItem.Click += New_Snip_Click;
        saveItem.Click += Save_As_Click;
        drawItem.ButtonClick += Paint_Click;
        eraseItem.Click += New_Snip_Click;
        this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);
        this.Text = "Better Snipping Tool";

        pictureBox = new PictureBox
        {
            Image = clonedBitmap,
            SizeMode = PictureBoxSizeMode.AutoSize,
            //Size = clonedBitmap.Size
        };

        pictureBox.MouseDown += new MouseEventHandler(paint_MouseDown);
        pictureBox.MouseMove += new MouseEventHandler(paint_MouseMove);
        pictureBox.MouseUp += new MouseEventHandler(paint_MouseUp);

        panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(padding),
            AutoScroll = true
        };

        //Add the PictureBox to the Panel
        panel.Controls.Add(pictureBox);
        pictureBox.Location = new Point((panel.ClientSize.Width - pictureBox.Width) / 2, (panel.ClientSize.Height - pictureBox.Height) / 2);
        panel.Resize += (sender, e) => pictureBox.Location = new Point((panel.ClientSize.Width - pictureBox.Width) / 2, (panel.ClientSize.Height - pictureBox.Height) / 2);

        //Adding Controls
        this.Controls.Add(panel);

        //Adjust the form size to include padding
        this.ClientSize = new Size(bitmap.Width + 2 * padding, bitmap.Height + 2 * padding);

        //Center the form on the primary monitor
        Rectangle screenBounds = Screen.AllScreens[monitorIndex].Bounds;
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(screenBounds.X + (screenBounds.Width - this.Width) / 2, screenBounds.Y + (screenBounds.Height - this.Height) / 2);
    }

    private void New_Snip_Click(object? sender, EventArgs e)
    {
        this.Dispose();
        BetterSnippingTool newSnip = new BetterSnippingTool();
        this.Hide();
        newSnip.Closed += (s, args) => this.Close();
        newSnip.Show();
    }

    private void Save_As_Click(object? sender, EventArgs e)
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Title = "Save As";
            saveFileDialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|Bitmap Files (*.bmp)|*.bmp";

            //Use the saved directory from configuration if available
            if (!string.IsNullOrWhiteSpace(AppConfig.Instance.DefaultDirectory))
            {
                saveFileDialog.InitialDirectory = AppConfig.Instance.DefaultDirectory;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                drawingBitmap.Save(saveFileDialog.FileName);
            }
        }
    }

    //NOT IMPLEMENTED
    private void Snipping_Mode_Click(object? sender, EventArgs e)
    {
        using (BetterSnippingTool newSnip = new BetterSnippingTool())
        {
            this.Hide();
            newSnip.ShowDialog();
        }
        this.Close();
        this.Dispose();
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

    private void paint_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && isDrawing)
        {
            lastPoint = e.Location;
        }
    }

    private void paint_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && isDrawing)
        {
            PInvoke_Clipboard.SetBitmapToClipboard(drawingBitmap);
            Bitmap clonedBitmap = new Bitmap(drawingBitmap);
            undoStack.Push(clonedBitmap);
        }
    }

    private void paint_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isDrawing && e.Button == MouseButtons.Left && drawPen != null)
        {
            using (Graphics g = Graphics.FromImage(drawingBitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(drawPen, lastPoint, e.Location);
            }
            lastPoint = e.Location;
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
                AppConfig.Instance.SaveConfig();
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

    private void EditMenuItem_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("Edit menu clicked");
    }

    private void ToolsMenuItem_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("Tools menu clicked");
    }

    private void HelpMenuItem_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("Help menu clicked");
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
        Environment.Exit(0);
    }

    private void Close_Program(object? sender, EventArgs e)
    {
        Environment.Exit(0);
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
            this.FormClosing -= MainForm_FormClosing;
            this.KeyDown -= Form_KeyDown;
            newSnipItem.Click -= New_Snip_Click;
            modeItem.Click -= New_Snip_Click;
            saveItem.Click -= Save_As_Click;
            drawItem.ButtonClick -= Paint_Click;
            eraseItem.Click -= New_Snip_Click;

            // Dispose of undo and redo stacks
            while (undoStack.Count > 0)
            {
                undoStack.Pop()?.Dispose();
            }

            while (redoStack.Count > 0)
            {
                redoStack.Pop()?.Dispose();
            }

            // Dispose of controls and resources
            pictureBox?.Image?.Dispose();
            pictureBox?.Dispose();
            drawingBitmap?.Dispose();
            clonedBitmap?.Dispose();
            paintButtonOff?.Dispose();
            paintButtonOn?.Dispose();
            panel?.Dispose();
            menuStrip?.Dispose();
            submenuStrip?.Dispose();
            fileMenuItem?.Dispose();
            editMenuItem?.Dispose();
            prefsMenuItem?.Dispose();
            newSnipItem?.Dispose();
            modeItem?.Dispose();
            saveItem?.Dispose();
            drawItem?.Dispose();
            eraseItem?.Dispose();
            drawPen?.Dispose();
            paintCursor?.Dispose();
        }

        base.Dispose(disposing);
    }


    ~ScreenshotForm()
    {
        Dispose(false);
    }
}