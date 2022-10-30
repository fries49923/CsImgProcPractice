using System;
using System.Threading.Tasks;
using System.Windows;

namespace MyCvModule
{
    public class Inverse : MyCvAlgorithmBase
    {
        #region Property

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

        public Inverse()
            : base()
        {
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
                throw new Exception($"[MyCvModule][Inverse]{ex.Message}");
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

                // Does not handle alpha
                int step = (channel > 3) ? 3 : channel;

                this.srcImg.CopyPixels(pixel, stride, 0);

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x += channel)
                    {
                        int idx = x + y * stride;
                        for (int i = 0; i < step; i++)
                        {
                            pixel[idx + i] = (byte)(255 - pixel[idx + i]);
                        }
                    }
                });

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
                int bufferStride = this.srcImg.BackBufferStride;

                try
                {
                    this.srcImg.Lock();

                    unsafe
                    {
                        byte* ptr = (byte*)this.srcImg.BackBuffer.ToPointer();
                        byte* ptrStart = ptr;
                        byte* ptrEnd = ptr + bufferStride * height;

                        while (ptr != ptrEnd)
                        {
                            // Does not handle alpha
                            if (channel == 4
                                && (ptr - ptrStart + 1) % 4 == 0)
                            {
                                ptr++;
                                continue;
                            }

                            *ptr = (byte)(255 - *ptr);
                            ptr++;
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
