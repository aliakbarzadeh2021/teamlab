using System.Drawing;
using System;

namespace ASC.Web.Controls
{
    public class ColorPickerColorChangedArgs : EventArgs
{
    public Color Color {get; set;}

    public ColorPickerColorChangedArgs()
    {

    }

    public ColorPickerColorChangedArgs(Color color)
    {
        Color = color;
    }
}
}
