using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GifSettingsForm : Form
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
    public GifSettingsForm(int gifCreatorFormHeight, int gifCreatorFormWidth, int currentScreenIndex)
    {

    }
}
