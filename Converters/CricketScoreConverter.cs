using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DartsApp.Converters
{
    public class CricketScoreConverter : IValueConverter, IMarkupExtension
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is List<int> playerScores && parameter is int number)
            {
                if (playerScores[number] == 0) return "";
                if (playerScores[number] == 1) return "/";
                if (playerScores[number] == 2) return "X";
                if (playerScores[number] > 2) return "O";
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
