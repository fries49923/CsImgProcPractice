using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public class Sobel : EmguCvAlgorithmBase
    {
        #region Property

        private int xorder;
        public int Xorder
        {
            get => this.xorder;
            set
            {
                this.xorder = value;
                this.OnPropertyChanged();
            }
        }

        private int yorder;
        public int Yorder
        {
            get => this.yorder;
            set
            {
                this.yorder = value;
                this.OnPropertyChanged();
            }
        }

        private int ksize;
        public int Ksize
        {
            get => this.ksize;
            set
            {
                this.ksize = value;
                this.OnPropertyChanged();
            }
        }

        private double scale;
        public double Scale
        {
            get => this.scale;
            set
            {
                this.scale = value;
                this.OnPropertyChanged();
            }
        }

        private double delta;
        public double Delta
        {
            get => this.delta;
            set
            {
                this.delta = value;
                this.OnPropertyChanged();
            }
        }

        private BorderType borderType;
        public BorderType BorderType
        {
            get => this.borderType;
            set
            {
                this.borderType = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public Sobel()
            : base()
        {
            this.xorder = 1;
            this.yorder = 0;
            this.ksize = 3;
            this.scale = 1.0;
            this.delta = 0;
            this.borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Sobel(this.srcMat, this.dstMat, DepthType.Cv8U, this.xorder, this.yorder, this.ksize, this.scale, this.delta, this.borderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Binarization]{ex.Message}");
            }
        }
    }
}