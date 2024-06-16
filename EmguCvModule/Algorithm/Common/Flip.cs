using CommunityToolkit.Mvvm.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public partial class Flip : EmguCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private FlipType _flipType;

        #endregion

        public Flip()
            : base()
        {
            _flipType = FlipType.Vertical;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Flip(srcMat, dstMat, FlipType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Flip]{ex.Message}");
            }
        }
    }
}
