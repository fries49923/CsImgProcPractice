using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace EmguCvModule
{
    public partial class GaussianBlur : EmguCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private int _ksize;

        [ObservableProperty]
        private double _sigmaX;

        [ObservableProperty]
        private double _sigmaY;

        [ObservableProperty]
        private BorderType _borderType;

        #endregion

        public GaussianBlur()
            : base()
        {
            _ksize = 3;
            _sigmaX = 1.0;
            _sigmaY = 0.0;
            _borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                var size = new Size(Ksize, Ksize);
                CvInvoke.GaussianBlur(
                    srcMat, dstMat,
                    size, SigmaX, SigmaY, BorderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][GaussianBlur]{ex.Message}");
            }
        }
    }
}
