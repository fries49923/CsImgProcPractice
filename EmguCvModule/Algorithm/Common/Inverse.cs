using Emgu.CV;
using System;

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
                CvInvoke.BitwiseNot(this.srcMat, this.dstMat);
            }
            catch (Exception ex)
            {
                throw new Exception($"[EmguCvModule][Inverse]{ex.Message}");
            }
        }
    }
}
