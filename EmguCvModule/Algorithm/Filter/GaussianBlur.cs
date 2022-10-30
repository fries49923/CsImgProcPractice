using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public class GaussianBlur : EmguCvAlgorithmBase
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

        private double sigmaX;
        public double SigmaX
        {
            get => this.sigmaX;
            set
            {
                this.sigmaX = value;
                this.OnPropertyChanged();
            }
        }

        private double sigmaY;
        public double SigmaY
        {
            get => this.sigmaY;
            set
            {
                this.sigmaY = value;
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

        public GaussianBlur()
            : base()
        {
            this.ksize = 3;
            this.sigmaX = 1.0;
            this.sigmaY = 0.0;
            this.borderType = BorderType.Default;
        }

        protected override void Calculate()
        {
            try
            {
                Size size = new Size(this.ksize, this.ksize);
                CvInvoke.GaussianBlur(this.srcMat, this.dstMat, size, this.sigmaX, this.sigmaY, this.borderType);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][GaussianBlur]{ex.Message}");
            }
        }
    }
}
