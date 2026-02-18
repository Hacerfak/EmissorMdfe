using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EmissorMdfe.UI.Converters;

public class IndexConverter : IValueConverter
{
    // Da ViewModel (Código 1, 2...) para a Tela (Índice 0, 1...)
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valorInteiro)
        {
            return valorInteiro - 1;
        }
        return 0;
    }

    // Da Tela (Índice 0, 1...) para a ViewModel (Código 1, 2...)
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int indiceInteiro)
        {
            return indiceInteiro + 1;
        }
        return 1;
    }
}