using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace BaseModule
{
    public abstract class AlgorithmBase : INotifyPropertyChanged
    {
        protected BitmapSource srcBitmapSource;

        public AlgorithmBase()
        {
            this.srcBitmapSource = null;
        }

        public void Execute(BitmapSource srcImg, ref BitmapSource dstImg)
        {
            try
            {
                this.srcBitmapSource = srcImg;

                this.ConvertImage();
                this.Calculate();
                this.RevertImage();

                dstImg = this.srcBitmapSource;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.ClearData();
            }
        }

        protected abstract void ConvertImage();

        protected abstract void Calculate();

        protected abstract void RevertImage();

        protected virtual void ClearData()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                return;
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
