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
                        (BaseModule.AlgorithmBase?)Activator.CreateInstance(type);

                    if (plugin is null)
                        continue;

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
                if (result is true)
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
            return string.IsNullOrEmpty(FileName) is false;
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
                if (File.Exists(fileName) is false)
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
                if (File.Exists(FileName) is false)
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

                ImageProcessCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                throw new Exception($"[LoadImageFromFile]{ex.Message}");
            }
        }

        private bool CanAlgorithmChanged(object obj)
        {
            ImageProcessCommand.NotifyCanExecuteChanged();

            return obj is not null;
        }

        [RelayCommand(CanExecute = nameof(CanAlgorithmChanged))]
        private void AlgorithmChanged(object obj)
        {
            try
            {
                ParaControl.Clear();

                var p = new PropControlCreator(obj);
                var ctl = p.GetPropControls();
                if (ctl is null)
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

        private bool CanImageProcess(object obj)
        {
            return obj is not null && Img is not null;
        }

        [RelayCommand(CanExecute = nameof(CanImageProcess))]
        private void ImageProcess(object obj)
        {
            try
            {
               BitmapSource image = Img;
               var algorithm = (BaseModule.AlgorithmBase)obj;

                int startTime = Environment.TickCount;
                algorithm.Execute(image, ref image);
                TactTime = Environment.TickCount - startTime + " ms";
                Img = image;

                HistoryImgs.Add(
                    new ImageData { AlgName = obj.ToString(), Img = image });

                UpdateCanExecuteChanged();
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

        private bool CanSaveImage()
        {
            return Img is not null;
        }

        [RelayCommand(CanExecute = nameof(CanSaveImage))]
        private void SaveImage()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp|TIF Image|*.tif;*.tiff|PNG Image|*.png",
                    FileName = $"Imgae_{DateTime.Now:yyyyMMddHHmmss}"
                };

                bool? result = dlg.ShowDialog();
                if (result is false)
                {
                    return;
                }

                string fileName = dlg.FileName;
                string ext = Path.GetExtension(fileName);

                BitmapEncoder encoder = ext.ToLower() switch
                {
                    ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                    ".tif" or ".tiff" => new TiffBitmapEncoder(),
                    ".png" => new PngBitmapEncoder(),
                    _ => new BmpBitmapEncoder(),
                };

                encoder.Frames.Add(BitmapFrame.Create(Img));
                using var fileStream = new FileStream(fileName, FileMode.Create);
                encoder.Save(fileStream);
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

            if (obj is null)
            {
                return true;
            }

            var str = $"{obj}";

            return str.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
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

        private bool CanHistoryRestore(object obj)
        {
            return obj is not null;
        }

        //[RelayCommand(CanExecute = nameof(CanHistoryRestore))]
        [RelayCommand]
        private void HistoryRestore(object obj)
        {
            if (obj is not ImageData data)
            {
                return;
            }

            Img = data.Img;
            UpdateCanExecuteChanged();
        }

        private bool CanHistoryRemove(object obj)
        {
            return obj is not null;
        }

        //[RelayCommand(CanExecute = nameof(CanHistoryRemove))]
        [RelayCommand]
        private void HistoryRemove(object obj)
        {
            if (obj is not ImageData data)
            {
                return;
            }

            HistoryImgs.Remove(data);
            UpdateCanExecuteChanged();
            GC.Collect();
        }

        private bool CanHistoryRemoveAll()
        {
            return HistoryImgs is not null && HistoryImgs.Count > 0;
        }

        //[RelayCommand(CanExecute = nameof(CanHistoryRemoveAll))]
        [RelayCommand]
        private void HistoryRemoveAll()
        {
            HistoryImgs.Clear();
            UpdateCanExecuteChanged();
            GC.Collect();
        }

        private void UpdateCanExecuteChanged()
        {
            //HistoryRestoreCommand.NotifyCanExecuteChanged();
            //HistoryRemoveCommand.NotifyCanExecuteChanged();
            //HistoryRemoveAllCommand.NotifyCanExecuteChanged();
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
