//Using platform invoking to copy bitmap, memory efficient.
using System.Runtime.InteropServices;

namespace BetterSnippingTool.Interop
{
    public static class PInvoke_Clipboard
    {
        //A handle (pointer) to a bitmap (HBITMAP)
        const int CF_BITMAP = 2;
        const int CF_HDROP = 15;

        [DllImport(dllName: "user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWnd);

        [DllImport(dllName: "user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport(dllName: "user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport(dllName: "user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        //Function used to set a bitmap to the clipboard utilizing Win32 API
        public static void SetBitmapToClipboard(Bitmap bitmap)
        {
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hdc = IntPtr.Zero;
            IntPtr hdcMem = IntPtr.Zero;

            try
            {
                //Create a compatible DC
                hdc = GetDC(IntPtr.Zero);
                hdcMem = CreateCompatibleDC(hdc);

                // Create a compatible bitmap
                hBitmap = CreateCompatibleBitmap(hdc, bitmap.Width, bitmap.Height);
                IntPtr oldBitmap = SelectObject(hdcMem, hBitmap);

                // Draw the bitmap onto the compatible DC
                using (Graphics g = Graphics.FromHdc(hdcMem))
                {
                    g.DrawImage(bitmap, 0, 0);
                }

                SelectObject(hdcMem, oldBitmap);

                // Open the clipboard and clear it
                if (!OpenClipboard(IntPtr.Zero))
                    throw new InvalidOperationException("Failed to open clipboard.");

                if (!EmptyClipboard())
                    throw new InvalidOperationException("Failed to empty clipboard.");

                // Set the bitmap data to the clipboard
                IntPtr result = SetClipboardData(CF_BITMAP, hBitmap);
                if (result == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to set clipboard data.");

                // Release DC
                ReleaseDC(IntPtr.Zero, hdc);
            }
            finally
            {
                // Clean up GDI objects
                if (hdc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hdc);
                if (hdcMem != IntPtr.Zero) DeleteObject(hdcMem);
                if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);

                // Close the clipboard
                CloseClipboard();
            }
        }
    }
}