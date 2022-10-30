using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyCvModule
{
    public class Color2Gray : MyCvAlgorithmBase
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

        public Color2Gray()
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
                throw new Exception($"[MyCvModule][Color2Gray]{ex.Message}");
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

                // Does not handle gray
                if (channel < 3
                    || channel > 4)
                {
                    return;
                }

                byte[] newPixel = new byte[height * width];

                this.srcImg.CopyPixels(pixel, stride, 0);

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x += channel)
                    {
                        int idx = x + y * stride;
                        byte gray = (byte)(0.299 * pixel[idx + 2] + 0.587 * pixel[idx + 1] + 0.114 * pixel[idx]);

                        newPixel[idx / channel] = gray;
                    }
                });

                WriteableBitmap newImg =
                    new WriteableBitmap(width, height, this.srcBitmapSource.DpiX, this.srcBitmapSource.DpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256);

                try
                {
                    newImg.Lock();
                    newImg.WritePixels(new Int32Rect(0, 0, width, height), newPixel, width, 0);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    newImg.Unlock();
                }

                this.srcImg = newImg;
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

                // Does not handle gray
                if (channel < 3
                    || channel > 4)
                {
                    return;
                }

                int offset = 0;
                if (stride != bufferStride)
                {
                    offset = bufferStride - stride;
                }

                WriteableBitmap newImg =
                    new WriteableBitmap(width, height, this.srcBitmapSource.DpiX, this.srcBitmapSource.DpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256);

                byte[] newPixel = new byte[height * width];

                try
                {
                    this.srcImg.Lock();

                    unsafe
                    {
                        byte* ptr = (byte*)this.srcImg.BackBuffer.ToPointer();
                        byte* ptrHead = ptr;
                        byte* ptrEnd = ptr + bufferStride * height;

                        fixed (byte* p = newPixel)
                        {
                            byte* nPtr = p;
                            while (ptr != ptrEnd)
                            {
                                *nPtr = (byte)(0.299 * *(ptr + 2) + 0.587 * *(ptr + 1) + 0.114 * *ptr);

                                nPtr++;
                                ptr += channel;

                                if (offset > 0)
                                {
                                    int len = (int)(ptr - ptrHead);
                                    if (len % stride == 0)
                                    {
                                        ptr += offset;
                                        ptrHead = ptr;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    this.srcImg.Unlock();
                }

                try
                {
                    newImg.Lock();
                    newImg.WritePixels(new Int32Rect(0, 0, width, height), newPixel, width, 0);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    newImg.Unlock();
                }

                this.srcImg = newImg;
            }
            catch (Exception ex)
            {
                throw new Exception($"[Pointer]{ex.Message}");
            }
        }
    }
}
