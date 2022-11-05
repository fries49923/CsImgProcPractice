using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsImgProcPractice
{
    public class ConfigViewModel : ViewModelBase
    {
        #region Property

        private AppThemes theme;
        public AppThemes Theme
        {
            get => this.theme;

            set
            {
                this.theme = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Command

        public ICommand WindowLoadCommand
        { get; private set; }

        public ICommand ThemeChangeCommand
        { get; private set; }

        #endregion

        public ConfigViewModel()
        {
            this.theme = default;

            this.WindowLoadCommand = new RelayCommand((obj) => WindowLoad_Execute());
            this.ThemeChangeCommand = new RelayCommand((obj) => ThemeChange_Execute());
        }

        private string GetLastThemeName()
        {
            ResourceDictionary res = Application.Current.Resources.MergedDictionaries.LastOrDefault(
                obj => obj.Source != null && obj.Source.ToString().Contains("Theme/"));

            if (res == null)
            {
                return "";
            }

            string themeFileName = Path.GetFileNameWithoutExtension(res.Source.ToString());

            return themeFileName.Replace("Theme", "");
        }

        private void WindowLoad_Execute()
        {
            string themeName = this.GetLastThemeName();

            if (Enum.TryParse(themeName, out AppThemes th))
            {
                this.Theme = th;
            }
        }

        private void ThemeChange_Execute()
        {
            ResourceDictionary res = Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                obj => obj.Source != null && obj.Source.ToString().EndsWith($"Theme/{this.Theme}Theme.xaml"));

            if (res == null)
            {
                return;
            }

            Application.Current.Resources.MergedDictionaries.Remove(res);
            Application.Current.Resources.MergedDictionaries.Add(res);
        }
    }
}
