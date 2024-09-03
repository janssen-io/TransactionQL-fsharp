using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Linq;

namespace TransactionQL.DesktopApp.Controls
{
    public static class ColourMixer
    {
        public static FuncMultiValueConverter<Color?, Color> MixOpacity { get; } =
            new FuncMultiValueConverter<Color?, Color>(cs =>
            {
                Color[] colours = cs.OfType<Color>().ToArray();
                return colours.Length != 2
                    ? throw new InvalidCastException($"Cannot mix the opacity of {colours.Length} colour(s).")
                    : new Color(colours[1].A, colours[0].R, colours[0].G, colours[0].B);
            });
    }
}