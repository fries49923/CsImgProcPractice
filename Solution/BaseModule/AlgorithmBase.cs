using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace BaseModule
{
    public abstract class AlgorithmBase : ObservableObject
    {
        protected BitmapSource? srcBitmapSource;

        public AlgorithmBase()
        {
            srcBitmapSource = null;
        }

        public void Execute(BitmapSource srcImg, ref BitmapSource dstImg)
        {
            try
            {
                srcBitmapSource = srcImg;

                ConvertImage();
                Calculate();
                RevertImage();

                dstImg = srcBitmapSource;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ClearData();
            }
        }

        protected abstract void ConvertImage();

        protected abstract void Calculate();

        protected abstract void RevertImage();

        protected virtual void ClearData()
        {

        }
    }
}
