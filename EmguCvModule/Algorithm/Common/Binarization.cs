using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public class Binarization : EmguCvAlgorithmBase
    {
        #region Property

        private double threshold;
        public double Threshold
        {
            get => this.threshold;
            set
            {
                this.threshold = value;
                this.OnPropertyChanged();
            }
        }

        private ThresholdType thresholdType;
        public ThresholdType ThresholdType
        {
            get => this.thresholdType;
            set
            {
                this.thresholdType = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public Binarization()
            : base()
        {
            this.threshold = 100;
            this.thresholdType = ThresholdType.Binary;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Threshold(this.srcMat, this.dstMat, this.threshold, 255, this.thresholdType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Binarization]{ex.Message}");
            }
        }
    }
}
