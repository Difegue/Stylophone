using SkiaSharp;
using System;

namespace ColorThiefDotNet
{
    public class QuantizedColor
    {
        public QuantizedColor(SKColor color, int population)
        {
            Color = color;
            Population = population;
            IsDark = CalculateYiqLuma(color) < 128;
        }

        public SKColor Color { get; private set; }
        public int Population { get; private set; }
        public bool IsDark { get; private set; }

        public int CalculateYiqLuma(SKColor color)
        {
            return Convert.ToInt32(Math.Round((299 * color.Red + 587 * color.Green + 114 * color.Blue) / 1000f));
        }
    }
}