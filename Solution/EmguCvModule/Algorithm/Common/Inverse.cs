using Emgu.CV;

namespace EmguCvModule
{
    public class Inverse : EmguCvAlgorithmBase
    {
        public Inverse()
            : base()
        {

        }

        protected override void Calculate()
        {
            try
            {
                CvInvoke.BitwiseNot(srcMat, dstMat);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Inverse]{ex.Message}");
            }
        }
    }
}
