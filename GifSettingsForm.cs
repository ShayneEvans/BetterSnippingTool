using BetterSnippingTool.Config;
using BetterSnippingTool.Utilities;
using System.Windows.Forms;

namespace BetterSnippingTool.Forms
{
    public class GifSettingsForm : Form
    {
        private System.Windows.Forms.Label? gifOptimizationLabel;
        private System.Windows.Forms.ComboBox? gifOptimizationLevelComboBox;
        private Dictionary<string, AppConfig.OptimizeGifCreation>? outputGifOptimizationChoices;
        private System.Windows.Forms.Label? gifCustomResolutionLabel;
        private System.Windows.Forms.ComboBox? gifOutputResolutionsComboBox;
        private Dictionary<string, (int width, int height)>? outputResolutions;
        private int gifOutputResolutionWidth;
        private int gifOutputResolutionHeight;
        private System.Windows.Forms.Label? gifFpsLabel;
        private System.Windows.Forms.NumericUpDown? gifFPS;
        private System.Windows.Forms.Label? gifSecondsLabel;
        private System.Windows.Forms.NumericUpDown? gifSeconds;
        private System.Windows.Forms.Label? gifProfileLoaded;
        private System.Windows.Forms.Button? gifSaveProfileButton;
        private System.Windows.Forms.Button? gifLoadProfileButton;
        private System.Windows.Forms.Button? gifSaveCurrentSettingsButton;
        private bool isLoadingAppConfig;
        private FileUtilities fileUtilities;

        public GifSettingsForm()
        {
            fileUtilities = new FileUtilities();
            this.Icon = new Icon(fileUtilities.buttonImagePaths["BS_Logo"]);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            //Set the icon for the form
            this.Size = new System.Drawing.Size(300, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Text = "GIF Settings";
            this.MinimizeBox = false;
            this.MaximizeBox = false;


            gifOptimizationLabel = new Label
            {
                Location = new Point(25, 10),
                AutoSize = true,
                Text = "Resolution Method",
                Font = new Font("Arial", 10, FontStyle.Bold),
            };

            gifOptimizationLevelComboBox = new System.Windows.Forms.ComboBox()
            {
                Enabled = true,
                Location = new Point(30, 30),
                Size = new Size(240, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            outputGifOptimizationChoices = new Dictionary<string, AppConfig.OptimizeGifCreation>
            {
                { "Optimize GIF Resolution", AppConfig.OptimizeGifCreation.Optimize },
                { "Use Snipped Resolution", AppConfig.OptimizeGifCreation.SnippedRes },
                { "Choose Custom Resolution", AppConfig.OptimizeGifCreation.CustomRes },
            };

            gifOptimizationLevelComboBox.Items.AddRange(outputGifOptimizationChoices.Keys.ToArray());

            gifCustomResolutionLabel = new Label
            {
                Location = new Point(25, 70),
                AutoSize = true,
                Text = $"Custom Resolutions: Disabled",
                Font = new Font("Arial", 10, FontStyle.Bold),
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

            gifFpsLabel = new Label
            {
                Location = new Point(25, 120),
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
                Location = new Point(25, 170),
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
                Location = new Point(25, 220),
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

            this.Controls.Add(gifOptimizationLabel);
            this.Controls.Add(gifOptimizationLevelComboBox);
            this.Controls.Add(gifCustomResolutionLabel);
            this.Controls.Add(gifOutputResolutionsComboBox);
            this.Controls.Add(gifFpsLabel);
            this.Controls.Add(gifFPS);
            this.Controls.Add(gifSecondsLabel);
            this.Controls.Add(gifSeconds);
            this.Controls.Add(gifProfileLoaded);
            this.Controls.Add(gifSaveCurrentSettingsButton);
            this.Controls.Add(gifSaveProfileButton);
            this.Controls.Add(gifLoadProfileButton);

            gifOptimizationLevelComboBox.SelectedIndexChanged += gifOptimizationLevelComboBox_SelectedIndexChanged;
            gifOutputResolutionsComboBox.SelectedIndexChanged += GifOutputResolutionsComboBox_SelectedIndexChanged;
            gifSaveCurrentSettingsButton.Click += GifSaveCurrentSettingsButton_Clicked;
            gifSaveProfileButton.Click += GifSaveProfileButton_Click;
            gifLoadProfileButton.Click += GifLoadProfileButton_Click;

            loadAppConfigVariables();
        }

        //Setting the current control values to AppConfig
        private void setAppConfigVariables()
        {
            string selectedItem = gifOptimizationLevelComboBox.SelectedItem as string;

            if (selectedItem != null && outputGifOptimizationChoices.TryGetValue(selectedItem, out AppConfig.OptimizeGifCreation selectedValue))
            {
                AppConfig.Instance.OptimizationLevel = selectedValue;
            }

            if (selectedItem == "Choose Custom Resolution")
            {
                AppConfig.Instance.gifOutputResolution = (gifOutputResolutionWidth, gifOutputResolutionHeight);
            }
            else
            {
                AppConfig.Instance.gifOutputResolution = (0, 0);
            }
            AppConfig.Instance.FPS = (int)gifFPS.Value;
            AppConfig.Instance.Seconds = (int)gifSeconds.Value;
        }

        //Loading AppConfig variables to controls
        private void loadAppConfigVariables()
        {
            //Boolean used to prevent event handlers from executing
            isLoadingAppConfig = true;
            var optimizationLevel = AppConfig.Instance.OptimizationLevel;

            //Load configuration values
            if (AppConfig.Instance.OptimizationLevel == AppConfig.OptimizeGifCreation.Optimize)
            {
                gifOptimizationLevelComboBox.SelectedItem = "Optimize GIF Resolution";
                gifOutputResolutionsComboBox.Enabled = false;
            }
            else if (AppConfig.Instance.OptimizationLevel == AppConfig.OptimizeGifCreation.SnippedRes)
            {
                gifOptimizationLevelComboBox.SelectedItem = "Use Snipped Resolution";
                gifOutputResolutionsComboBox.Enabled = false;
            }
            else if (AppConfig.Instance.OptimizationLevel == AppConfig.OptimizeGifCreation.CustomRes)
            {
                gifOptimizationLevelComboBox.SelectedItem = "Choose Custom Resolution";
                gifOutputResolutionsComboBox.Enabled = true;
            }

            gifOutputResolutionsComboBox.SelectedItem = $"{AppConfig.Instance.gifOutputResolution.Item1}x{AppConfig.Instance.gifOutputResolution.Item2}";
            gifFPS.Value = AppConfig.Instance.FPS;
            gifSeconds.Value = AppConfig.Instance.Seconds;

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

        private void gifOptimizationLevelComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (isLoadingAppConfig) return;
            if (gifOptimizationLevelComboBox.SelectedItem == "Choose Custom Resolution")
            {
                gifOutputResolutionsComboBox.Enabled = true;
                gifCustomResolutionLabel.Text = "Custom Resolutions: Enabled";
            }
            else
            {
                gifOutputResolutionsComboBox.Enabled = false;
                gifCustomResolutionLabel.Text = "Custom Resolutions: Disabled";
            }
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
                saveFileDialog.ShowHelp = false;
                saveFileDialog.AutoUpgradeEnabled = true;
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
            using (OpenFileDialog loadFileDialog = new OpenFileDialog())
            {
                loadFileDialog.InitialDirectory = fileUtilities.profilesDir;
                loadFileDialog.Title = "Load Profile";
                loadFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                loadFileDialog.DefaultExt = "xml";
                loadFileDialog.AddExtension = true;
                loadFileDialog.ShowHelp = false;
                loadFileDialog.ShowReadOnly = false;
                loadFileDialog.AutoUpgradeEnabled = true;
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
}