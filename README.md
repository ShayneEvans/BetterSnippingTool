
# Better Snipping Tool
The Better Snipping Tool was born from a simple annoyance: the extra click required by the default Snipping Tool. I wanted a faster, smoother tool with my own unique touch. What started as a personal project evolved into a practical and customizable solution for snipping, painting, and creating GIFs, all while maintaining simplicity and efficiency.

# Examples
## Snipping
![snip_example](https://github.com/user-attachments/assets/d79b4ced-f3df-4816-b099-30f338e26d91)  
*Example of a screenshot captured using the Better Snipping Tool.*

## Painting
![snip_paint_example](https://github.com/user-attachments/assets/1085f151-228e-4c7a-8886-541911131ce0)  
*Example of a painting on a screenshot captured with the Better Snipping Tool.*

## GIF Creation
![chuck_fall_gif](https://github.com/user-attachments/assets/a96dc453-350b-4b2a-b79b-d8eddc716ce6)  
*Example of an endlessly looping GIF captured using Better Snipping Tool.*

## Trimming
![trimming](https://github.com/user-attachments/assets/644e1766-41cf-4993-ac51-77ca726317ed)  
*Trimming menu that pops up when enabling trimming, user selects starting and ending frames.*

## GIF Settings
![gif_settings](https://github.com/user-attachments/assets/be2ccc01-6685-4afe-a66e-cbaca3da5f5f)  
*The GIF Settings form.*

# Requirements
- Operating System: Windows 10/11 (Linux planned).
- .NET 8.0
- FFmpeg with GIF and PNG support (minimal build included in source code and installer).

# Installation
### 1. Download Release
- Download the latest release at https://github.com/ShayneEvans/BetterSnippingTool/releases
- Click on BetterSnippingTool.vX.Y.Z.Setup.zip and then use the setup executable to install.
- A shortcut should appear on your desktop, you're now ready to snip!

### 2. Compile Yourself
- git clone https://github.com/yourusername/BetterSnippingTool.git
- Choose to use the pre compiled minimal build of FFmpeg or create/download your own.
- Run it in Visual Studio.

# Architecture
The Better Snipping Tool is built entirely in C# with WinForms providing the graphical user interface (GUI). It offers advanced functionality for capturing and editing screenshots, creating GIFs, and managing user settings efficiently.
## Snipping
- Screenshots are captured based on the user-defined selection area and converted into `Bitmap` objects. These images are then displayed in a `PictureBox` on the **MediaForm**.
- From the **MediaForm**, users can:

	-   Save the screenshot to their computer.
	-   Edit the screenshot in Paint.
	-   Capture a new screenshot.
	-   Create a GIF using multiple screenshots.
## Clipboard
-   The clipboard functionality required a memory-efficient solution, especially for managing frequent updates during painting operations.
-   The default `Clipboard.SetImage()` method was unsuitable because it retained each image in memory, leading to gradual memory increases with every paintbrush stroke.
-   To resolve this, **platform invoking (P/Invoke)** was used to clear the clipboard history. This ensured that memory usage remained stable, even with frequent updates.
-   The clipboard supports both screenshots and GIFs, allowing captures to be instantly pasted where needed.

## GIFs
-   GIFs are composed of multiple screenshots, determined by the user's selected frames per second (FPS) and duration in seconds.
    -   For example, a 24 FPS GIF lasting 5 seconds will consist of 120 screenshots (`5 × 24 = 120`).
-   Screenshots are captured using the same method as standard snips and are temporarily stored in the **AppData/Roaming** directory.
-   Once all screenshots are captured:
    1.  **FFmpeg** is used to generate a color palette from the screenshots.
    2.  The GIF is then created using this palette for optimal quality.
-   The completed GIF is displayed in a `PictureBox` on the **MediaForm**, where users can optionally trim it by selecting starting and ending frames.

## Configs and Profiles
-   Users can customize several settings in the application, including:
    -   Default save directory for screenshots and GIFs.
    -   Resolution options: **Optimized**, **Snipped Resolution**, or **Custom Resolution**.
    -   GIF FPS (1–60).
    -   GIF duration in seconds (1–20).
-   Configurations can be saved as XML profiles, allowing users to easily switch between different setups for GIF creation.
-   A singleton pattern is used to manage the application's configuration. These settings are loaded at the start of the application, ensuring seamless operation.

# Future Fixes, Improvements, and Planned Features:
- Fix: GIF stays in memory even after being disposed.
- Improvement: Mimic snipping tool transparency when snipping.
- Improvement: Smaller FFmpeg compile.
- Improvement: Add unit testing using MVP pattern.
- Feature: Ability to add text to GIFs post creation.
- Feature: More options for screenshots such as full screen, free form shape, etc.
- Feature: Optimize screenshot output as toggleable option (reducing resolution to save space).
- Feature: Cropping.
