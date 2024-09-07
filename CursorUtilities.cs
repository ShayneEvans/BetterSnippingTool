using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CursorUtilities
{
    public static Cursor LoadCustomCursor(string filePath)
    {
        Bitmap bitmap = new Bitmap(filePath);
        IntPtr ptr = bitmap.GetHicon();
        return new Cursor(ptr);
    }

    public static Cursor CreatePaintCursor(Color penColor, int penSize)
    {
        int cursorWidth = penSize;
        int cursorHeight = penSize;

        using (Bitmap bitmap = new Bitmap(cursorWidth, cursorHeight))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);

                int cursorRadius = penSize / 2;
                int cursorX = cursorRadius;
                int cursorY = cursorRadius;

                using (SolidBrush dotBrush = new SolidBrush(penColor))
                {
                    g.FillEllipse(dotBrush, cursorX - cursorRadius, cursorY - cursorRadius, penSize, penSize);
                }
            }
            // Convert the bitmap to a cursor
            IntPtr hCursor = bitmap.GetHicon();
            return new Cursor(hCursor);
        }
    }
}