using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CsImgProcPractice
{
    public class MainViewModel : ViewModelBase
    {
        #region Property

        private string fileName;
        public string FileName
        {
            get => this.fileName;

            set
            {
                this.fileName = value;
                this.OnPropertyChanged();
            }
        }

        private string searchText;
        public string SearchText
        {
            get => this.searchText;

            set
            {
                this.searchText = value;
                this.OnPropertyChanged();
            }
        }

        private string tactTime;
        public string TactTime
        {
            get => this.tactTime;

            set
            {
                this.tactTime = value;
                this.OnPropertyChanged();
            }
        }

        private BitmapSource img;
        public BitmapSource Img
        {
            get => this.img;

            set
            {
                this.img = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<BaseModule.AlgorithmBase> Algorithms
        { get; private set; }

        public ObservableCollection<FrameworkElement> ParaControl
        { get; private set; }

        public ObservableCollection<ImageData> HistoryImgs
        { get; private set; }

        #endregion

        #region Command

        public ICommand WindowLoadCommand { get; private set; }

        public ICommand LoadImageCommand { get; private set; }

        public ICommand ReloadImageCommand { get; private set; }

        public ICommand DropImageFileCommand { get; private set; }

        public ICommand AlgorithmChangedCommand { get; private set; }

        public ICommand AlgorithmSearchCommand { get; private set; }

        public ICommand ImageProcessCommand { get; private set; }

        public ICommand SaveImageCommand { get; private set; }

        public ICommand HistoryRestoreCommand { get; private set; }

        public ICommand HistoryRemoveCommand { get; private set; }

        public ICommand HistoryRemoveAllCommand { get; private set; }

        public ICommand ConfigSettingCommand { get; private set; }

        #endregion

        #region Variable

        private int dailogFilterIndex; // This index is 1-based, not 0-based.
        private CollectionView collectionView;

        #endregion

        public MainViewModel()
        {
            this.fileName = "";
            this.searchText = "";
            this.tactTime = "";
            this.img = null;
            this.Algorithms = new ObservableCollection<BaseModule.AlgorithmBase>();
            this.ParaControl = new ObservableCollection<FrameworkElement>();
            this.HistoryImgs = new ObservableCollection<ImageData>();

            this.WindowLoadCommand = new RelayCommand((obj) => WindowLoad_Execute());
            this.LoadImageCommand = new RelayCommand((obj) => LoadImage_Execute());
            this.ReloadImageCommand = new RelayCommand(ReloadImage_Execute, (obj) => ReloadImage_CanExecute());
            this.DropImageFileCommand = new RelayCommand(DropImageFile_Execute, DropImageFile_CanExecute);
            this.AlgorithmChangedCommand = new RelayCommand(AlgorithmChanged_Execute, AlgorithmChanged_CanExecute);
            this.AlgorithmSearchCommand = new RelayCommand((obj) => AlgorithmSearch_Execute());
            this.ImageProcessCommand = new RelayCommand(ImageProcess_Execute, ImageProcess_CanExecute);
            this.SaveImageCommand = new RelayCommand((obj) => SaveImage_Execute(), (obj) => SaveImage_CanExecute());
            this.HistoryRestoreCommand = new RelayCommand(HistoryRestore_Execute, HistoryRestore_CanExecute);
            this.HistoryRemoveCommand = new RelayCommand(HistoryRemove_Execute, HistoryRemove_CanExecute);
            this.HistoryRemoveAllCommand = new RelayCommand((obj) => HistoryRemoveAll_Execute(), (obj) => HistoryRemoveAll_CanExecute());
            this.ConfigSettingCommand = new RelayCommand((obj) => ConfigSetting_Execute());

            this.dailogFilterIndex = 1; // This index is 1-based, not 0-based.
            this.collectionView = (CollectionView)CollectionViewSource.GetDefaultView(this.Algorithms);
            this.collectionView.Filter = AlgorithmFilter;
        }

        private void WindowLoad_Execute()
        {
            try
            {
                // Load assemblies
                AssemblyLoader loader = new AssemblyLoader(AppDomain.CurrentDomain.BaseDirectory);
                Assembly[] assemblies = loader.Load();

                // Find algorithm class
                DerivedFinder finder = new DerivedFinder(typeof(BaseModule.AlgorithmBase));
                Type[] types = finder.FindType(assemblies);

                // Sort by FullName
                IEnumerable<Type> pluginTypes = types.OrderBy(type => type.FullName);

                // Add algorithm class
                foreach (Type type in pluginTypes)
                {
                    BaseModule.AlgorithmBase plugin =
                        (BaseModule.AlgorithmBase)Activator.CreateInstance(type);
                    string typeName = plugin.ToString();
                    this.Algorithms.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[WindowLoad]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadImage_Execute()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Filter = "JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp|TIF Image|*.tif;*.tiff|PNG Image|*.png|All files|*.*";
                dlg.FilterIndex = this.dailogFilterIndex;

                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    this.FileName = dlg.FileName;
                    this.dailogFilterIndex = dlg.FilterIndex;
                    this.LoadImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[LoadImage_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ReloadImage_CanExecute()
        {
            if (string.IsNullOrEmpty(this.FileName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void ReloadImage_Execute(object obj)
        {
            try
            {
                this.LoadImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[ReloadImage_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool DropImageFile_CanExecute(object obj)
        {
            if (obj == null)
            { return false; }

            if (obj.GetType() != typeof(string))
            { return false; }

            return true;
        }

        private void DropImageFile_Execute(object obj)
        {
            try
            {
                string fileName = (string)obj;

                if (!File.Exists(fileName))
                {
                    return;
                }

                string ext = Path.GetExtension(fileName).ToLower();

                if (ext != ".jpg"
                    && ext != ".jpeg"
                    && ext != ".tif"
                    && ext != ".tiff"
                    && ext != ".png"
                    && ext != ".bmp")
                {
                    return;
                }

                this.FileName = fileName;
                this.LoadImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[DropImageFile_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadImage()
        {
            try
            {
                if (!File.Exists(this.FileName))
                {
                    return;
                }

                var bitmap = new BitmapImage();
                using (var stream = File.OpenRead(this.FileName))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                this.Img = bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"[LoadImage]{ex.Message}");
            }
        }

        private bool AlgorithmChanged_CanExecute(object obj)
        {
            return obj != null;
        }

        private void AlgorithmChanged_Execute(object obj)
        {
            try
            {
                this.ParaControl.Clear();

                PropControlCreator p = new PropControlCreator(obj);
                var ctl = p.GetPropControls();
                if (ctl == null)
                {
                    return;
                }

                this.ParaControl.Add(ctl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[AlgorithmChanged_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ImageProcess_CanExecute(object obj)
        {
            return obj != null && this.img != null;
        }

        private void ImageProcess_Execute(object obj)
        {
            try
            {
                BitmapSource image = this.Img;
                BaseModule.AlgorithmBase algorithm = (BaseModule.AlgorithmBase)obj;

                int startTime = Environment.TickCount;
                algorithm.Execute(image, ref image);
                this.TactTime = Environment.TickCount - startTime + " ms";
                this.Img = image;

                this.HistoryImgs.Add(
                    new ImageData { AlgName = obj.ToString(), Img = image });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[ImageProcess_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool SaveImage_CanExecute()
        {
            return this.img != null;
        }

        private void SaveImage_Execute()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp|TIF Image|*.tif;*.tiff|PNG Image|*.png";
                dlg.FileName = $"Imgae_{DateTime.Now:yyyyMMddHHmmss}";

                bool? result = dlg.ShowDialog();
                if (result != true)
                {
                    return;
                }

                string fileName = dlg.FileName;
                string ext = Path.GetExtension(fileName);
                BitmapEncoder encoder = null;
                switch (ext.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;

                    case ".tif":
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;

                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;

                    case ".bmp":
                    default:
                        encoder = new BmpBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(this.img));
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[SaveImage_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool AlgorithmFilter(object obj)
        {
            if (string.IsNullOrEmpty(this.SearchText))
            {
                return true;
            }
            else
            {
                return obj.ToString().IndexOf(this.SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private void AlgorithmSearch_Execute()
        {
            try
            {
                CollectionViewSource.GetDefaultView(this.Algorithms).Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"[AlgorithmSearch_Execute]{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool HistoryRestore_CanExecute(object obj)
        {
            return obj != null;
        }

        private void HistoryRestore_Execute(object obj)
        {
            ImageData data = obj as ImageData;

            this.Img = data.Img;
        }

        private bool HistoryRemove_CanExecute(object obj)
        {
            return obj != null;
        }

        private void HistoryRemove_Execute(object obj)
        {
            ImageData data = obj as ImageData;

            this.HistoryImgs.Remove(data);
            GC.Collect();
        }

        private bool HistoryRemoveAll_CanExecute()
        {
            return this.HistoryImgs != null && this.HistoryImgs.Count > 0;
        }

        private void HistoryRemoveAll_Execute()
        {
            this.HistoryImgs.Clear();
            GC.Collect();
        }

        private void ConfigSetting_Execute()
        {
            ConfigWindow window = new ConfigWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}
