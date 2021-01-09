using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfAsync
{
    class IntToKoleckaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)value;
            string kolecko;

            switch(i)
            {
                case 0: return ":-)";
                case 1: kolecko = "kolečko"; break;
                case 2: case 3: case 4: kolecko = "kolečka"; break;
                default: kolecko = "koleček"; break;
            }
            return $"Musím odvézt ještě {i} {kolecko} betonu :-(";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
