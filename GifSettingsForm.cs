using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class GifSettingsForm : Form
{
    private System.Windows.Forms.CheckBox optimizeGifCreationCheckBox;
    private System.Windows.Forms.Label orLabel;
    private System.Windows.Forms.CheckBox gifOutputCustomResolutionCheckBox;
    private System.Windows.Forms.ComboBox gifOutputResolutionsComboBox;
    private Dictionary<string, (int width, int height)> outputResolutions;
    //private System.Windows.Forms.TextBox gifOutputResolutionWidth;
    //private System.Windows.Forms.TextBox gifOutputResolutionHeight;
    private int gifOutputResolutionWidth;
    private int gifOutputResolutionHeight;
    private System.Windows.Forms.Label gifFpsLabel;
    private System.Windows.Forms.NumericUpDown gifFPS;
    private System.Windows.Forms.Label gifSecondsLabel;
    private System.Windows.Forms.NumericUpDown gifSeconds;
    private System.Windows.Forms.Label gifProfileLoaded;
    private System.Windows.Forms.Button gifSaveProfileButton;
    private System.Windows.Forms.Button gifLoadProfileButton;
    private System.Windows.Forms.Button gifSaveCurrentSettingsButton;
    private bool isLoadingAppConfig;

    public GifSettingsForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        //Set the icon for the form
        this.Icon = new Icon("Resources\\BS_Logo.ico");
        this.Size = new System.Drawing.Size(300, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Text = "GIF Settings";
        this.MinimizeBox = false;
        this.MaximizeBox = false;

        optimizeGifCreationCheckBox = new System.Windows.Forms.CheckBox()
        {
            AutoSize = true,
            Location = new System.Drawing.Point(30, 30),
            Name = "Optimize GIF Creation",
            Size = new System.Drawing.Size(74, 17),
            TabIndex = 0,
            Text = "Optimize GIF Creation*",
            UseVisualStyleBackColor = true,
            Checked = true
        };

        orLabel = new System.Windows.Forms.Label()
        {
            AutoSize = true,
            Text = "OR",
            Font = new Font("Arial", 9, FontStyle.Bold),
        };
        orLabel.Location = new System.Drawing.Point((this.ClientSize.Width - orLabel.Width) / 2, 48);

        gifOutputCustomResolutionCheckBox = new System.Windows.Forms.CheckBox
        {
            AutoSize = true,
            Location = new System.Drawing.Point(30, 60),
            Name = "Enable Custom Resolution",
            Size = new System.Drawing.Size(74, 17),
            TabIndex = 0,
            Text = "Enable Custom Resolution",
            UseVisualStyleBackColor = true,
            Enabled = false
        };

        gifOutputResolutionsComboBox = new System.Windows.Forms.ComboBox()
        {
            Enabled = false,
            Location = new Point(30, 90),
            Size = new Size(120, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        outputResolutions = new Dictionary<string, (int width, int height)>
  {
            { "240x240", (240, 240) },
            { "320x256", (320, 256) },
            { "360x640", (360, 640) },
            { "640x360", (640, 360) },
            { "640x480", (640, 480) },
            { "720x360", (720, 360) },
            { "720x1280", (720, 1280) },
            { "800x600", (800, 600) },
            { "960x640", (960, 640) },
            { "1080x720", (1080, 720) },
            { "1080x1080", (1080, 1080) },
            { "1280x540", (1280, 540) },
            { "1280x720", (1280, 720) },
            { "1280x1024", (1280, 1024) },
            { "1440x720", (1440, 720) },
            { "1440x960", (1440, 960) },
            { "1600x1280", (1600, 1280) },
            { "1920x1080", (1920, 1080) },
            { "2160x1080", (2160, 1080) },
            { "2160x1440", (2160, 1440) },
            { "2560x1080", (2560, 1080) },
            { "3440x1440", (3440, 1440) }
        };

        gifOutputResolutionsComboBox.Items.AddRange(outputResolutions.Keys.ToArray());

        /*gifOutputResolutionWidth = new System.Windows.Forms.TextBox
        {
            Enabled = false,
            AcceptsReturn = true,
            MaxLength = 4,
            Location = new Point(30, 120),
            Size = new Size(120, 20),
            Font = new Font("Arial", 10),
            TextAlign = HorizontalAlignment.Center,
        };

        gifOutputResolutionHeight = new System.Windows.Forms.TextBox
        {
            Enabled = false,
            AcceptsReturn = true,
            MaxLength = 4,
            Location = new Point(155, 120),
            Size = new Size(120, 20),
            Font = new Font("Arial", 10),
            TextAlign = HorizontalAlignment.Center,
        };*/



        gifFpsLabel = new Label
        {
            Location = new Point(30, 120),
            AutoSize = true,
            Text = "FPS (1-60)",
            Font = new Font("Arial", 10, FontStyle.Bold),
        };

        gifFPS = new System.Windows.Forms.NumericUpDown
        {
            Minimum = 1,
            Maximum = 60,
            Location = new Point(30, 140),
            Size = new Size(120, 20),
            Font = new Font("Arial", 10),
            TextAlign = HorizontalAlignment.Center,
            Value = 24
        };

        gifSecondsLabel = new Label
        {
            Location = new Point(30, 170),
            AutoSize = true,
            Text = "Length in Seconds (1-20)",
            Font = new Font("Arial", 10, FontStyle.Bold),
        };

        gifSeconds = new System.Windows.Forms.NumericUpDown
        {
            Minimum = 1,
            Maximum = 20,
            Location = new Point(30, 190),
            Size = new Size(120, 20),
            Font = new Font("Arial", 10),
            TextAlign = HorizontalAlignment.Center,
            Value = 5
        };

        gifProfileLoaded = new Label
        {
            Location = new Point(30, 220),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = $"Profile Loaded: {AppConfig.Instance.GetCurrentFileName()}",
            Font = new Font("Arial", 10, FontStyle.Bold),
        };

        gifSaveProfileButton = new System.Windows.Forms.Button
        {
            Size = new Size(100, 60),
            Location = new Point((this.ClientSize.Width - 100) / 2, 230),
            Text = "Save Settings \nas Profile"
        };

        gifLoadProfileButton = new System.Windows.Forms.Button
        {
            Size = new Size(100, 60),
            Text = "Load Settings \nProfile"
        };

        int totalWidth = gifSaveProfileButton.Width + gifLoadProfileButton.Width + 10;
        int startX = (this.ClientSize.Width - totalWidth) / 2;
        int startY = this.ClientSize.Height - gifSaveProfileButton.Height - 20;

        gifSaveProfileButton.Location = new Point(startX, startY);
        gifLoadProfileButton.Location = new Point(startX + gifSaveProfileButton.Width + 10, startY);

        gifSaveCurrentSettingsButton = new System.Windows.Forms.Button
        {
            Size = new Size(totalWidth, 25),
            Location = new Point((this.ClientSize.Width - totalWidth) / 2, 250),
            Text = "Save Current Settings"
        };

        this.Controls.Add(optimizeGifCreationCheckBox);
        this.Controls.Add(orLabel);
        this.Controls.Add(gifOutputResolutionsComboBox);
        this.Controls.Add(gifOutputCustomResolutionCheckBox);
        this.Controls.Add(gifFpsLabel);
        this.Controls.Add(gifFPS);
        this.Controls.Add(gifSecondsLabel);
        this.Controls.Add(gifSeconds);
        this.Controls.Add(gifProfileLoaded);
        this.Controls.Add(gifSaveCurrentSettingsButton);
        this.Controls.Add(gifSaveProfileButton);
        this.Controls.Add(gifLoadProfileButton);

        optimizeGifCreationCheckBox.CheckedChanged += optimizeGifCreationCheckBox_CheckedChanged;
        gifOutputResolutionsComboBox.SelectedIndexChanged += GifOutputResolutionsComboBox_SelectedIndexChanged;
        gifOutputCustomResolutionCheckBox.CheckedChanged += GifOutputCustomResolutionCheckBox_CheckedChanged;
        gifSaveCurrentSettingsButton.Click += GifSaveCurrentSettingsButton_Clicked;
        gifSaveProfileButton.Click += GifSaveProfileButton_Click;
        gifLoadProfileButton.Click += GifLoadProfileButton_Click;

        loadAppConfigVariables();
    }

    //Setting the current control values to AppConfig
    private void setAppConfigVariables()
    {
        AppConfig.Instance.OptimizeGifCreation = optimizeGifCreationCheckBox.Checked; 
        if(optimizeGifCreationCheckBox.Checked == true)
        {
            AppConfig.Instance.gifOutputResolution = (0, 0);
        }
        else
        {
            AppConfig.Instance.gifOutputResolution = (gifOutputResolutionWidth, gifOutputResolutionHeight);
        }
        AppConfig.Instance.FPS = (int)gifFPS.Value;
        AppConfig.Instance.Seconds = (int)gifSeconds.Value;
    }

    //Loading AppConfig variables to controls
    private void loadAppConfigVariables()
    {
        //Boolean used to prevent event handlers from executing
        isLoadingAppConfig = true;

        //Load configuration values
        optimizeGifCreationCheckBox.Checked = AppConfig.Instance.OptimizeGifCreation;
        gifOutputCustomResolutionCheckBox.Checked = !optimizeGifCreationCheckBox.Checked;
        gifOutputResolutionsComboBox.SelectedItem = $"{AppConfig.Instance.gifOutputResolution.Item1}x{AppConfig.Instance.gifOutputResolution.Item2}";
        gifFPS.Value = AppConfig.Instance.FPS;
        gifSeconds.Value = AppConfig.Instance.Seconds;

        //Explicitly set the enabled states
        gifOutputCustomResolutionCheckBox.Enabled = !optimizeGifCreationCheckBox.Checked;
        gifOutputResolutionsComboBox.Enabled = gifOutputCustomResolutionCheckBox.Checked;
        optimizeGifCreationCheckBox.Enabled = !gifOutputCustomResolutionCheckBox.Checked;

        //Reset the loading flag to false
        isLoadingAppConfig = false;
    }

    private void GifOutputResolutionsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string selectedResolution = gifOutputResolutionsComboBox.SelectedItem.ToString();
        (int width, int height) = outputResolutions[selectedResolution];
        gifOutputResolutionWidth = width;
        gifOutputResolutionHeight = height;
    }

    private void optimizeGifCreationCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (isLoadingAppConfig) return;
        gifOutputCustomResolutionCheckBox.Enabled = !optimizeGifCreationCheckBox.Checked;
    }

    //Toggles using custom resolution or the combobox
    private void GifOutputCustomResolutionCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (isLoadingAppConfig) return;

        optimizeGifCreationCheckBox.Enabled = !gifOutputCustomResolutionCheckBox.Checked;
        optimizeGifCreationCheckBox.Checked = false; // Clear the optimize checkbox
        gifOutputResolutionsComboBox.Enabled = gifOutputCustomResolutionCheckBox.Checked;
    }

    private void GifSaveCurrentSettingsButton_Clicked(object? sender, EventArgs e)
    {
        setAppConfigVariables();
        AppConfig.Instance.SaveConfig(AppConfig.Instance.GetCurrentFileName());
    }

    private void GifSaveProfileButton_Click(object? sender, EventArgs e)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string parentDirectory = Directory.GetParent(baseDirectory).FullName;
        string profilesPath = Path.Combine(parentDirectory, "Profiles");
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.InitialDirectory = profilesPath;
            saveFileDialog.Title = "Save Profile";
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                setAppConfigVariables();
                AppConfig.Instance.SaveConfig($"{saveFileDialog.FileName}");
                string selectedFileName = Path.GetFileNameWithoutExtension($"{saveFileDialog.FileName}.xml");
                AppConfig.Instance.SwitchToConfig(selectedFileName);
                gifProfileLoaded.Text = $"Profile Loaded: {AppConfig.Instance.GetCurrentFileName()}";
            }
        }
    }

    private void GifLoadProfileButton_Click(object? sender, EventArgs e)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string parentDirectory = Directory.GetParent(baseDirectory).FullName;
        string profilesPath = Path.Combine(parentDirectory, "Profiles");
        using (OpenFileDialog loadFileDialog = new OpenFileDialog())
        {
            loadFileDialog.InitialDirectory = profilesPath;
            loadFileDialog.Title = "Load Profile";
            loadFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            loadFileDialog.DefaultExt = "xml";
            loadFileDialog.AddExtension = true;
            if (loadFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = Path.GetFileNameWithoutExtension($"{loadFileDialog.FileName}.xml");
                AppConfig.Instance.SwitchToConfig(selectedFileName);
                gifProfileLoaded.Text = $"Profile Loaded: {AppConfig.Instance.GetCurrentFileName()}";
                loadAppConfigVariables();
            }
        }
    }
}