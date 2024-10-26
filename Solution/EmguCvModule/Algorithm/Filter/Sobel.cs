using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public partial class Sobel : EmguCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private int _xorder;

        [ObservableProperty]
        private int _yorder;

        [ObservableProperty]
        private int _ksize;

        [ObservableProperty]
        private double _scale;

        [ObservableProperty]
        private double _delta;

        [ObservableProperty]
        private BorderType _borderType;

        #endregion

        public Sobel()
            : base()
        {
            _xorder = 1;
            _yorder = 0;
            _ksize = 3;
            _scale = 1.0;
            _delta = 0;
            _borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Sobel(
                     srcMat, dstMat,
                    DepthType.Cv8U, Xorder, Yorder,
                     Ksize, Scale, Delta, BorderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Binarization]{ex.Message}");
            }
        }
    }
}