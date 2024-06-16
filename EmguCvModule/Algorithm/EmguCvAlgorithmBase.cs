using Emgu.CV;

namespace EmguCvModule
{
    public abstract class EmguCvAlgorithmBase : BaseModule.AlgorithmBase
    {
        protected Mat? srcMat;
        protected Mat? dstMat;

        public EmguCvAlgorithmBase()
            : base()
        {
            srcMat = null;
            dstMat = null;
        }

        protected override void ConvertImage()
        {
            try
            {
                if (srcBitmapSource is null)
                {
                    throw new Exception("srcBitmapSource is null.");
                }

                srcMat = srcBitmapSource.ToMat();
                dstMat = new Mat();
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
                if (dstMat is null)
                {
                    throw new Exception("dstMat is null.");
                }

                srcBitmapSource = dstMat.ToBitmapSource();
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvAlgorithmBase][RevertImage]{ex.Message}");
            }
        }

        protected override void ClearData()
        {
            srcMat?.Dispose();
            srcMat = null;

            dstMat?.Dispose();
            dstMat = null;

            base.ClearData();
        }
    }
}
