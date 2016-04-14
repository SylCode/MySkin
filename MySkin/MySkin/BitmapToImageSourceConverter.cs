using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using System.Drawing;

namespace MySkin
{
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Bitmap)) return null;
            return (WriteableBitmap)(Bitmap)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is WriteableBitmap)) return null;
            return (Bitmap)(WriteableBitmap)value;
        }
    }
}
