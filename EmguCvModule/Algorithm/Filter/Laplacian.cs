using Emgu.CV;
using Emgu.CV.CvEnum;
using System;

namespace EmguCvModule
{
    public class Laplacian : EmguCvAlgorithmBase
    {
        #region Property

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

        public Laplacian()
            : base()
        {
            this.ksize = 1;
            this.scale = 1.0;
            this.delta = 0.0;
            this.borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.Laplacian(this.srcMat, this.dstMat, DepthType.Default, this.ksize, this.scale, this.delta, this.borderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Laplacian]{ex.Message}");
            }
        }
    }
}
