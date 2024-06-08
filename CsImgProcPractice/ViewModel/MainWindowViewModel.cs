using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CsImgProcPractice
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Variable

        private int dailogFilterIndex; // This index is 1-based, not 0-based.
        private CollectionView collectionView;

        #endregion

        #region Property

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadImageCommand))]
        private string _fileName;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _tactTime;

        [ObservableProperty]
        private BitmapSource? _img;

        public ObservableCollection<BaseModule.AlgorithmBase> Algorithms
        { get; private set; }

        public ObservableCollection<FrameworkElement> ParaControl
        { get; private set; }

        public ObservableCollection<ImageData> HistoryImgs
        { get; private set; }

        #endregion

        public MainWindowViewModel()
        {
            _fileName = "";
            _searchText = "";
            _tactTime = "";
            _img = null;
            Algorithms = new();
            ParaControl = new();
            HistoryImgs = new();

            dailogFilterIndex = 1; // This index is 1-based, not 0-based.
            collectionView = (CollectionView)CollectionViewSource.GetDefaultView(Algorithms);
            collectionView.Filter = AlgorithmFilter;
        }

        [RelayCommand]
        private void WindowLoad()
        {
            try
            {
                // Load assemblies
                var loader = new AssemblyLoader(AppDomain.CurrentDomain.BaseDirectory);
                var assemblies = loader.Load();

                // Find algorithm class
                var finder = new DerivedFinder(typeof(BaseModule.AlgorithmBase));
                var types = finder.FindType(assemblies);

                // Sort by FullName
                var pluginTypes = types.OrderBy(type => type.FullName);

                // Add algorithm class
                foreach (Type type in pluginTypes)
                {
                    var plugin =
                        (BaseModule.AlgorithmBase)Activator.CreateInstance(type);
                    string typeName = plugin.ToString();
                    Algorithms.Add(plugin);
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

        [RelayCommand]
        private void LoadImage()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp|TIF Image|*.tif;*.tiff|PNG Image|*.png|All files|*.*",
                    FilterIndex = dailogFilterIndex
                };

                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    FileName = dlg.FileName;
                    dailogFilterIndex = dlg.FilterIndex;
                    LoadImageFromFile();
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

        private bool CanReloadImage()
        {
            return !string.IsNullOrEmpty(FileName);
        }

        [RelayCommand(CanExecute = nameof(CanReloadImage))]
        private void ReloadImage()
        {
            try
            {
                LoadImageFromFile();
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

        [RelayCommand]
        private void DropImageFile(string fileName)
        {
            try
            {
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

                FileName = fileName;
                LoadImageFromFile();
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

        private void LoadImageFromFile()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    return;
                }

                var bitmap = new BitmapImage();
                using (var stream = File.OpenRead(FileName))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                Img = bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"[LoadImageFromFile]{ex.Message}");
            }
        }

        private bool CanAlgorithmChanged(object obj)
        {
            return obj != null;
        }

        [RelayCommand(CanExecute = nameof(CanAlgorithmChanged))]
        private void AlgorithmChanged(object obj)
        {
            try
            {
                ParaControl.Clear();

                PropControlCreator p = new PropControlCreator(obj);
                var ctl = p.GetPropControls();
                if (ctl == null)
                {
                    return;
                }

                ParaControl.Add(ctl);
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

        // TODO: Fix
        //private bool CanImageProcess(object obj)
        //{
        //    return obj != null && _img != null;
        //}

        //[RelayCommand(CanExecute = nameof(CanImageProcess))]
        [RelayCommand]
        private void ImageProcess(object obj)
        {
            try
            {
                if(obj is null || _img is null)
                {
                    return;
                }
                
                BitmapSource image = Img;
                BaseModule.AlgorithmBase algorithm = (BaseModule.AlgorithmBase)obj;

                int startTime = Environment.TickCount;
                algorithm.Execute(image, ref image);
                TactTime = Environment.TickCount - startTime + " ms";
                Img = image;

                HistoryImgs.Add(
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

        // TODO: Fix
        private bool CanSaveImage()
        {
            return _img != null;
        }

        [RelayCommand(CanExecute = nameof(CanSaveImage))]
        private void SaveImage()
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
                BitmapEncoder encoder;
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

                encoder.Frames.Add(BitmapFrame.Create(_img));
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
            if (string.IsNullOrEmpty(SearchText))
            {
                return true;
            }
            else
            {
                return obj.ToString().IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        [RelayCommand]
        private void AlgorithmSearch()
        {
            try
            {
                CollectionViewSource.GetDefaultView(Algorithms).Refresh();
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

        // TODO: Check
        private bool CanHistoryRestore(object obj)
        {
            return obj != null;
        }

        [RelayCommand(CanExecute = nameof(CanHistoryRestore))]
        private void HistoryRestore(object obj)
        {
            ImageData data = obj as ImageData;

            Img = data.Img;
        }

        // TODO: Check
        private bool CanHistoryRemove(object obj)
        {
            return obj != null;
        }

        [RelayCommand(CanExecute = nameof(CanHistoryRemove))]
        private void HistoryRemove(object obj)
        {
            ImageData data = obj as ImageData;

            HistoryImgs.Remove(data);
            GC.Collect();
        }

        // TODO: Check
        private bool CanHistoryRemoveAll()
        {
            return HistoryImgs != null && HistoryImgs.Count > 0;
        }

        [RelayCommand(CanExecute = nameof(CanHistoryRemoveAll))]
        private void HistoryRemoveAll()
        {
            HistoryImgs.Clear();
            GC.Collect();
        }

        [RelayCommand]
        private void ConfigSetting()
        {
            var window = new ConfigWindow
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
        }
    }
}
