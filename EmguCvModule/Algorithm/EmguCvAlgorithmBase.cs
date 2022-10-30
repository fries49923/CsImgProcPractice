using Emgu.CV;
using System;

namespace EmguCvModule
{
    public abstract class EmguCvAlgorithmBase : BaseModule.AlgorithmBase
    {
        protected Mat srcMat;
        protected Mat dstMat;

        public EmguCvAlgorithmBase()
            : base()
        {
            this.srcMat = null;
            this.dstMat = null;
        }

        protected override void ConvertImage()
        {
            try
            {
                this.srcMat = this.srcBitmapSource.ToMat();
                this.dstMat = new Mat();
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvAlgorithmBase][ConvertImage]{ex.Message}");
            }
        }

        protected override void RevertImage()
        {
            try
            {
                this.srcBitmapSource = this.dstMat.ToBitmapSource();
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvAlgorithmBase][RevertImage]{ex.Message}");
            }
        }

        protected override void ClearData()
        {
            this.srcMat?.Dispose();
            this.srcMat = null;

            this.dstMat?.Dispose();
            this.dstMat = null;

            base.ClearData();
        }
    }
}
