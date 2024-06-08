using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyCvModule
{
    public abstract class MyCvAlgorithmBase : BaseModule.AlgorithmBase
    {
        protected WriteableBitmap? srcImg;

        public MyCvAlgorithmBase()
            : base()
        {
            srcImg = null;
        }

        protected override void ConvertImage()
        {
            try
            {
                // Check format
                PixelFormat format = srcBitmapSource.Format;
                if (format != PixelFormats.Bgra32
                    && format != PixelFormats.Bgr32
                    && format != PixelFormats.Bgr24
                    && format != PixelFormats.Indexed8
                    && format != PixelFormats.Gray8)
                {
                    throw new Exception($"PixelFormat {format} not support.");
                }

                srcImg = new WriteableBitmap(srcBitmapSource);
            }
            catch (Exception ex)
            {
                throw new Exception($"[MyCvAlgorithmBase][ConvertImage]{ex.Message}");
            }
        }

        protected override void RevertImage()
        {
            srcBitmapSource = srcImg;
        }

        protected override void ClearData()
        {
            GC.Collect();
            base.ClearData();
        }
    }
}
