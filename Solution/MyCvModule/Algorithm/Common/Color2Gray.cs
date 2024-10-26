using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyCvModule
{
    public partial class Color2Gray : MyCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private ProcessType _processType;

        #endregion

        public Color2Gray()
            : base()
        {
            _processType = ProcessType.Parallel;
        }

        protected override void Calculate()
        {
            try
            {
                if (ProcessType is ProcessType.Pointer)
                {
                    Calculate_Pointer();
                }
                else
                {
                    Calculate_Parallel();
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
                if (srcBitmapSource is null)
                {
                    throw new Exception("srcBitmapSource is null");
                }

                if (srcImg is null)
                {
                    throw new Exception("srcImg is null");
                }

                int width = srcBitmapSource.PixelWidth;
                int height = srcBitmapSource.PixelHeight;
                int channel = (srcBitmapSource.Format.BitsPerPixel + 7) / 8;
                int stride = width * channel;
                byte[] pixel = new byte[height * stride];

                // Does not handle gray
                if (channel < 3
                    || channel > 4)
                {
                    return;
                }

                byte[] newPixel = new byte[height * width];

                srcImg.CopyPixels(pixel, stride, 0);

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x += channel)
                    {
                        int idx = x + y * stride;
                        byte gray = (byte)(0.299 * pixel[idx + 2] + 0.587 * pixel[idx + 1] + 0.114 * pixel[idx]);

                        newPixel[idx / channel] = gray;
                    }
                });

                var newImg =
                    new WriteableBitmap(width, height, srcBitmapSource.DpiX, srcBitmapSource.DpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256);

                try
                {
                    newImg.Lock();
                    newImg.WritePixels(new Int32Rect(0, 0, width, height), newPixel, width, 0);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    newImg.Unlock();
                }

                srcImg = newImg;
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
                if (srcBitmapSource is null)
                {
                    throw new Exception("srcBitmapSource is null");
                }

                if (srcImg is null)
                {
                    throw new Exception("srcImg is null");
                }

                int width = srcBitmapSource.PixelWidth;
                int height = srcBitmapSource.PixelHeight;
                int channel = (srcBitmapSource.Format.BitsPerPixel + 7) / 8;
                int stride = width * channel;
                int bufferStride = srcImg.BackBufferStride;

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

                var newImg =
                    new WriteableBitmap(width, height, srcBitmapSource.DpiX, srcBitmapSource.DpiY, PixelFormats.Indexed8, BitmapPalettes.Gray256);

                byte[] newPixel = new byte[height * width];

                try
                {
                    srcImg.Lock();

                    unsafe
                    {
                        byte* ptr = (byte*)srcImg.BackBuffer.ToPointer();
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
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    srcImg.Unlock();
                }

                try
                {
                    newImg.Lock();
                    newImg.WritePixels(new Int32Rect(0, 0, width, height), newPixel, width, 0);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    newImg.Unlock();
                }

                srcImg = newImg;
            }
            catch (Exception ex)
            {
                throw new Exception($"[Pointer]{ex.Message}");
            }
        }
    }
}
