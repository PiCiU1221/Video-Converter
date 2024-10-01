using Microsoft.UI.Xaml.Data;
using System;
using YoutubeConverter.Enums;

namespace YoutubeConverter.Converters
{
    /// <summary>
    /// Converts enum values to a more user friendly ones, that we then
    /// display in the UI.
    /// </summary>
    internal class ConversionTypeToFriendlyNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ConversionType conversionType)
            {
                switch (conversionType)
                {
                    case ConversionType.YoutubeVideo:
                        return "YouTube Wideo";

                    case ConversionType.YoutubeAudio:
                        return "YouTube Audio";

                    case ConversionType.TVP:
                        return "vod.tvp.pl";

                    default:
                        return value.ToString();
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue)
            {
                switch (stringValue)
                {
                    case "YouTube Wideo":
                        return ConversionType.YoutubeVideo;

                    case "YouTube Audio":
                        return ConversionType.YoutubeAudio;

                    case "vod.tvp.pl":
                        return ConversionType.TVP;
                }
            }
            return ConversionType.YoutubeVideo;
        }
    }
}