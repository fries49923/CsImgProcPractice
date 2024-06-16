using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EmguCvModule
{
    public class Color2Gray : EmguCvAlgorithmBase
    {
        public Color2Gray()
            : base()
        {

        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.CvtColor(srcMat, dstMat, ColorConversion.Bgr2Gray);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Color2Gray]{ex.Message}");
            }
        }
    }
}
