using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace CotpsBot.Converters
{
    [Preserve(AllMembers = true)]
    public class RunningToBtnColor: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (bool) value == false)
            {
                Application.Current.Resources.TryGetValue("PrimaryGreen", out var green);
                return (Color)green;
            }

            // true
            Application.Current.Resources.TryGetValue("PrimaryRed", out var red);
            return (Color)red;

        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}