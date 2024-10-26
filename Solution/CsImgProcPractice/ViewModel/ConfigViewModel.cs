using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;

namespace CsImgProcPractice
{
    public partial class ConfigWindowViewModel : ObservableObject
    {
        #region Property

        [ObservableProperty]
        private AppThemes _theme;

        #endregion

        public ConfigWindowViewModel()
        {
            _theme = default;
        }

        private string GetLastThemeName()
        {
            ResourceDictionary? res =
                Application.Current.Resources.MergedDictionaries.LastOrDefault(
                obj => obj.Source is not null
                    && obj.Source.ToString().Contains("Theme/"));

            if (res is null)
            {
                return "";
            }

            var themeFileName =
                Path.GetFileNameWithoutExtension(
                    res.Source.ToString());

            return themeFileName.Replace("Theme", "");
        }

        [RelayCommand]
        private void WindowLoad()
        {
            string themeName = GetLastThemeName();

            if (Enum.TryParse(themeName, out AppThemes th))
            {
                Theme = th;
            }
        }

        [RelayCommand]
        private void ThemeChange()
        {
            ResourceDictionary? res =
                Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                obj => obj.Source is not null
                    && obj.Source.ToString().EndsWith($"Theme/{Theme}Theme.xaml"));

            if (res is null)
            {
                return;
            }

            Application.Current.Resources.MergedDictionaries.Remove(res);
            Application.Current.Resources.MergedDictionaries.Add(res);
        }
    }
}
