using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Utils
{
    public class WPFElementHelper
    {
        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        public enum Themes
        {
            Light,
            Dark
        }
        public static void SetTheme(Themes desiredTheme)
        {
            switch (desiredTheme)
            {
                case Themes.Light:
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Blue"), ThemeManager.GetAppTheme("BaseLight"));
                    return;
                case Themes.Dark:
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Blue"), ThemeManager.GetAppTheme("BaseDark"));
                    return;
                default:
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Red"), ThemeManager.GetAppTheme("BaseLight"));
                    return;
            }
            
            
        }
    }
}