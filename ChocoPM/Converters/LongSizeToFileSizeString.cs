using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChocoPM.Converters
{
    public class LongSizeToFileSizeString : IValueConverter
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize  (long fileSize, [MarshalAs ( UnmanagedType.LPTStr )] StringBuilder buffer, int bufferSize );

        /// <summary>
        /// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
        /// </summary>
        /// <param name="filelength">The numeric value to be converted.</param>
        /// <returns>the converted string</returns>
        public static string StrFormatByteSize (long filesize) {
             StringBuilder sb = new StringBuilder( 16 );
             StrFormatByteSize( filesize, sb, sb.Capacity );
             return sb.ToString();
        }
 
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is long))
                return "";
            return StrFormatByteSize((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
