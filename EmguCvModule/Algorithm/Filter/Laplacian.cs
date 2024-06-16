using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public partial class Laplacian : EmguCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private int _ksize;

        [ObservableProperty]
        private double _scale;

        [ObservableProperty]
        private double _delta;

        [ObservableProperty]
        private BorderType _borderType;

        #endregion

        public Laplacian()
            : base()
        {
            _ksize = 1;
            _scale = 1.0;
            _delta = 0.0;
            _borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Laplacian(
                    srcMat, dstMat,
                    DepthType.Default, Ksize, Scale, Delta, BorderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Laplacian]{ex.Message}");
            }
        }
    }
}
