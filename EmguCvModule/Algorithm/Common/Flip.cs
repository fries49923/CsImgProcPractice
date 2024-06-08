using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public class Flip : EmguCvAlgorithmBase
    {
        #region Property

        private FlipType flipType;
        public FlipType FlipType
        {
            get => this.flipType;
            set
            {
                this.flipType = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public Flip()
            : base()
        {
            this.flipType = FlipType.Vertical;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Flip(this.srcMat, this.dstMat, this.flipType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Flip]{ex.Message}");
            }
        }
    }
}
