
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
#### **1. Prerequisites**

Before building, ensure you have the following installed:

-   [Visual Studio](https://visualstudio.microsoft.com/) (Community Edition is free)
    -   During installation, include:
        -   **.NET Desktop Development workload**
        -   **Windows Forms support**
-   Microsoft Windows Desktop Runtime - 8.0.0 (x64)

#### **2. Clone the Repository**
Download the source code:
`git clone https://github.com/ShayneEvans/BetterSnippingTool.git && cd BetterSnippingTool`

### **3. Open the Project**

1.  Locate the solution file (e.g., `BetterSnippingTool.sln`).
2.  Open it with Visual Studio.
### **4. Install Dependencies**
1.  In Visual Studio, go to **Tools > NuGet Package Manager > Manage NuGet Packages for Solution**.
2.  Restore the required dependencies by clicking the **Restore** button or running:
- **Dependency**: `WindowsAPICodePack` version `8.0.4`


### **Install FFmpeg (Minimal Build with GIF Support)**

Follow the steps below to install a minimal FFmpeg build with GIF support.

#### **Step-by-Step Guide**

1.  **Install MSYS2**  
    Download and install MSYS2 from [msys2.org](https://www.msys2.org/).
    
2.  **Install Build Tools**  
    Open the MSYS2 terminal and run the following command:  
    `pacman -S base-devel mingw-w64-x86_64-toolchain git yasm pkg-config`
    
3.  **Download the FFmpeg Source**  
    Clone the FFmpeg repository and navigate to the directory:  
    `git clone https://git.ffmpeg.org/ffmpeg.git ffmpeg`  
    `cd ffmpeg`
    
4.  **Install Required Libraries**  
    Install the necessary libraries (`libpng` and `zlib`):  
    `pacman -S mingw-w64-x86_64-libpng mingw-w64-x86_64-zlib`
    
5.  **Configure FFmpeg with Minimal Settings**  
    Run the following command to configure FFmpeg with only the necessary options for GIF support:  
    `./configure --disable-everything --enable-demuxer=image2 --enable-decoder=png --enable-muxer=gif --enable-encoder=gif --enable-encoder=png --enable-filter=palettegen --enable-filter=paletteuse --enable-filter=scale --extra-cflags="-I/mingw64/include" --extra-ldflags="-L/mingw64/lib"`
    
    -   If you encounter the error `gcc is unable to create an executable file`, install additional MinGW libraries with the following command:  
        `pacman -S mingw-w64-x86_64-gcc mingw-w64-x86_64-pkg-config mingw-w64-x86_64-zlib`
    -   If this doesn't work it is possible GCC isn't installed. This can be checked by doing `gcc --version`, if it doesn't work it's not installed. Execute this 	command to install gcc: `pacman -S mingw-w64-x86_64-gcc`. if `gcc --version` doesn't work after this make sure it appears in your PATH. The MSYS2 MinGW 	environment should automatically set this up, but you can verify by checking: `echo $PATH`. If the output does not include `/mingw64/bin` then add it 		manually with `export PATH=$PATH:/mingw64/bin`. After this hopefully `gcc --version` should work.
    -   Another potential error will be `nasm not found or too old. Please install/pdate nasm or use --disablex86asm for a build without hand-optimzied assembly` 	this can be remedied with `pacman -S nasm`.
6.  **Compile FFmpeg**  
    Run the following command to compile FFmpeg:  
    `make -j4`
    
7.  **Gather Required Files**  
    After the build completes, you will need the following files:
    
    -   `ffmpeg.exe` **\ffmpeg**
    -   `libbz2-1.dll` **C:\msys64\mingw64\bin**
    -   `libiconv-2.dll` **C:\msys64\mingw64\bin**
    -   `liblzma-5.dll` **C:\msys64\mingw64\bin**
    -   `libwinpthread-1.dll` **C:\msys64\mingw64\bin**
    -   `zlib1.dll` **C:\msys64\mingw64\bin**
8.  **Cleanup**  
    Remove unnecessary files and keep only the required binaries and DLLs by running:  
    `find . -maxdepth 1 ! -name '.' ! -name 'ffmpeg.exe' ! -name 'libbz2-1.dll' ! -name 'libiconv-2.dll' ! -name 'liblzma-5.dll' ! -name 'libwinpthread-1.dll' ! -name 'zlib1.dll' -exec rm -rf {} +`

### **5. Build the Project**

1.  In Visual Studio, go to **Build > Build Solution** or press `Ctrl+Shift+B`.
2.  The build will generate an `.exe` file in one of the following directories:
    -   For Debug builds: `bin\Debug`
    -   For Release builds: `bin\Release`
### **6. Run the Application**

1.  Navigate to the output folder (`bin\Debug` or `bin\Release`).
2.  Locate the `.exe` file (e.g., `BetterSnippingTool.exe`).
3.  Double-click to run the program.

### **7. Troubleshooting**

-   If you encounter issues during the build:
    -   Ensure all prerequisites are installed.
    -   Check the target framework compatibility on your system.
-   Feel free to open an issue in the repository for further assistance.

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
