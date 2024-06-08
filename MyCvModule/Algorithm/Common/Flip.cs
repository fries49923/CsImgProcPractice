using System.Windows;

namespace MyCvModule
{
    public class Flip : MyCvAlgorithmBase
    {
        #region Property

        private DirectionType directionType;
        public DirectionType DirectionType
        {
            get => this.directionType;
            set
            {
                this.directionType = value;
                this.OnPropertyChanged();
            }
        }

        private ProcessType processType;
        public ProcessType ProcessType
        {
            get => this.processType;
            set
            {
                this.processType = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public Flip()
            : base()
        {
            this.directionType = DirectionType.Horizontal;
            this.processType = ProcessType.Parallel;
        }

        protected override void Calculate()
        {
            try
            {
                if (this.processType == ProcessType.Pointer)
                {
                    this.Calculate_Pointer();
                }
                else
                {
                    this.Calculate_Parallel();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[MyCvModule][Flip]{ex.Message}");
            }
        }

        private void Calculate_Parallel()
        {
            try
            {
                int width = this.srcBitmapSource.PixelWidth;
                int height = this.srcBitmapSource.PixelHeight;
                int channel = (this.srcBitmapSource.Format.BitsPerPixel + 7) / 8;
                int stride = width * channel;
                byte[] pixel = new byte[height * stride];

                this.srcImg.CopyPixels(pixel, stride, 0);

                if (this.directionType == DirectionType.Horizontal
                    || this.directionType == DirectionType.Both)
                {
                    Parallel.For(0, height, y =>
                    {
                        for (int x = 0; x < (width / 2) * channel; x += channel)
                        {
                            int idxL = x + y * stride;
                            int idxR = (stride - x - 1) + y * stride - (channel - 1);
                            for (int ch = 0; ch < channel; ch++)
                            {
                                // Swap use tuple
                                (pixel[idxR + ch], pixel[idxL + ch]) =
                                    (pixel[idxL + ch], pixel[idxR + ch]);
                            }
                        }
                    });
                }

                if (this.directionType == DirectionType.Vertical
                    || this.directionType == DirectionType.Both)
                {
                    Parallel.For(0, height / 2, y =>
                    {
                        for (int x = 0; x < stride; x += channel)
                        {
                            int idxT = x + y * stride;
                            int idxB = x + (height - y - 1) * stride;
                            for (int ch = 0; ch < channel; ch++)
                            {
                                // Swap use tuple
                                (pixel[idxB + ch], pixel[idxT + ch]) =
                                    (pixel[idxT + ch], pixel[idxB + ch]);
                            }
                        }
                    });
                }

                try
                {
                    this.srcImg.Lock();
                    this.srcImg.WritePixels(new Int32Rect(0, 0, width, height), pixel, stride, 0);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    this.srcImg.Unlock();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[Parallel]{ex.Message}");
            }
        }

        private void Calculate_Pointer()
        {
            try
            {
                int width = this.srcBitmapSource.PixelWidth;
                int height = this.srcBitmapSource.PixelHeight;
                int channel = (this.srcBitmapSource.Format.BitsPerPixel + 7) / 8;
                int stride = width * channel;
                int bufferStride = this.srcImg.BackBufferStride;

                int offset = 0;
                if (stride != bufferStride)
                {
                    offset = bufferStride - stride;
                }

                try
                {
                    this.srcImg.Lock();

                    if (this.directionType == DirectionType.Horizontal
                        || this.directionType == DirectionType.Both)
                    {
                        unsafe
                        {
                            byte* ptr = (byte*)this.srcImg.BackBuffer.ToPointer();

                            byte* ptrL = ptr;
                            byte* ptrR = ptrL + (stride - 1) - (channel - 1);
                            byte* ptrEnd = ptr + bufferStride * height;

                            while (ptrL != ptrEnd)
                            {
                                for (int ch = 0; ch < channel; ch++)
                                {
                                    (*ptrR, *ptrL) =
                                        (*ptrL, *ptrR);

                                    ptrL++;

                                    if (ch == channel - 1)
                                    {
                                        ptrR -= (2 * channel - 1);
                                    }
                                    else
                                    {
                                        ptrR++;
                                    }
                                }

                                int len = (int)(ptrR - ptrL);
                                if (len < channel)
                                {
                                    ptrL += ((width / 2) + (width % 2)) * channel + offset;
                                    ptrR = ptrL + (stride - 1) - (channel - 1);
                                }
                            }
                        }
                    }

                    if (this.directionType == DirectionType.Vertical
                        || this.directionType == DirectionType.Both)
                    {
                        unsafe
                        {
                            byte* ptr = (byte*)this.srcImg.BackBuffer.ToPointer();

                            byte* ptrT = ptr;
                            byte* ptrB = ptr + bufferStride * (height - 1);
                            byte* ptrHead = ptr;
                            byte* ptrEnd = ptr + bufferStride * (height / 2);
                            while (ptrT != ptrEnd)
                            {
                                (*ptrB, *ptrT) =
                                    (*ptrT, *ptrB);

                                ptrT++;
                                ptrB++;

                                int len = (int)(ptrT - ptrHead);
                                if (len % stride == 0)
                                {
                                    ptrT += offset;
                                    ptrHead = ptrT;

                                    int h = (int)((ptrT - ptr) / bufferStride + 1);
                                    ptrB = ptr + bufferStride * (height - h);
                                }
                            }

                        }
                    }

                    this.srcImg.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    this.srcImg.Unlock();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[Pointer]{ex.Message}");
            }
        }
    }
}
