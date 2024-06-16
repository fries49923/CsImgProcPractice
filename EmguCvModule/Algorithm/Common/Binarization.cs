using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public partial class Binarization : EmguCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private double _threshold;

        [ObservableProperty]
        private ThresholdType _thresholdType;

        #endregion

        public Binarization()
            : base()
        {
            _threshold = 100;
            _thresholdType = ThresholdType.Binary;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Threshold(srcMat, dstMat, Threshold, 255, ThresholdType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Binarization]{ex.Message}");
            }
        }
    }
}
