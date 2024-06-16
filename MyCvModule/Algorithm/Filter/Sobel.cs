using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace MyCvModule
{
    // TODO: How to speed up??
    public partial class Sobel : MyCvAlgorithmBase
    {
        #region Property

        [ObservableProperty]
        private DirectionType _directionType;

        [ObservableProperty]
        private ProcessType _processType;

        #endregion

        public Sobel()
            : base()
        {
            _directionType = DirectionType.Both;
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
                throw new Exception($"[MyCvModule][Sobel]{ex.Message}");
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

                srcImg.CopyPixels(pixel, stride, 0);

                int[] gx = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                int[] gy = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
                byte[] newPixel = new byte[height * stride];

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x += channel)
                    {
                        int idx = x + y * stride;
                        for (int i = 0; i < channel; i++)
                        {
                            if (i == 3)
                            {
                                // Does not handle alpha
                                newPixel[idx + i] = pixel[idx + i];
                            }
                            else if (x == 0 || y == 0 || x == stride - channel || y == height - 1)
                            {
                                // edge -> 0
                                newPixel[idx + i] = 0;
                            }
                            else
                            {
                                int sumX = 0;
                                int sumY = 0;
                                for (int yy = -1; yy <= 1; yy++)
                                {
                                    for (int xx = -1; xx <= 1; xx++)
                                    {
                                        int kernelIdx = (xx + 1) + (yy + 1) * 3;
                                        int sIdx = idx + i + (xx * channel) + (yy * stride);

                                        if (DirectionType is DirectionType.Vertical
                                           || DirectionType is DirectionType.Both)
                                        {
                                            sumX += gx[kernelIdx] * pixel[sIdx];
                                        }

                                        if (DirectionType is DirectionType.Horizontal
                                        || DirectionType is DirectionType.Both)
                                        {
                                            sumY += gy[kernelIdx] * pixel[sIdx];
                                        }
                                    }
                                }

                                int result = (int)Math.Sqrt(sumX * sumX + sumY * sumY);
                                if (result > 255)
                                {
                                    result = 255;
                                }

                                newPixel[idx + i] = (byte)result;
                            }
                        }
                    }
                });

                try
                {
                    srcImg.Lock();
                    srcImg.WritePixels(new Int32Rect(0, 0, width, height), newPixel, stride, 0);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    srcImg.Unlock();
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

                int offset = 0;
                if (stride != bufferStride)
                {
                    offset = bufferStride - stride;
                }

                int[] gx = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                int[] gy = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

                byte[] copyPixel = new byte[height * stride];
                srcImg.CopyPixels(copyPixel, stride, 0);

                try
                {
                    srcImg.Lock();

                    unsafe
                    {
                        byte* ptr = (byte*)srcImg.BackBuffer.ToPointer();
                        byte* ptrStart = ptr;
                        byte* ptrHead = ptr;
                        byte* ptrEnd = ptr + bufferStride * height;

                        fixed (byte* p = copyPixel)
                        {
                            byte* nPtr = p;
                            int len = 0;
                            while (ptr != ptrEnd)
                            {
                                int tmp = (int)(ptr - ptrStart);
                                if (channel == 4
                                    && (len + 1) % 4 == 0)
                                {
                                    // Does not handle alpha
                                    *ptr = *nPtr;
                                }
                                else if (tmp < stride
                                    || len % stride >= stride - channel
                                    || len % stride < channel
                                    || tmp > bufferStride * (height - 1))
                                {
                                    // edge -> 0
                                    *ptr = 0;
                                }
                                else
                                {
                                    // Convolution
                                    int sumX = 0;
                                    int sumY = 0;
                                    for (int y = -1; y <= 1; y++)
                                    {
                                        for (int x = -1; x <= 1; x++)
                                        {
                                            int kernelIdx = (x + 1) + (y + 1) * 3;
                                            byte* data = nPtr + (x * channel) + (y * stride);

                                            if (DirectionType is DirectionType.Vertical
                                               || DirectionType is DirectionType.Both)
                                            {
                                                sumX += gx[kernelIdx] * *data;
                                            }

                                            if (DirectionType is DirectionType.Horizontal
                                            || DirectionType is DirectionType.Both)
                                            {
                                                sumY += gy[kernelIdx] * *data;
                                            }
                                        }
                                    }

                                    int result = (int)Math.Sqrt(sumX * sumX + sumY * sumY);
                                    if (result > 255)
                                    {
                                        result = 255;
                                    }

                                    *ptr = (byte)result;
                                }

                                nPtr++;
                                ptr++;

                                len = (int)(ptr - ptrHead);
                                if (len % stride == 0)
                                {
                                    ptr += offset;
                                    ptrHead = ptr;
                                    len = 0;
                                }
                            }
                        }
                    }
                    srcImg.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    srcImg.Unlock();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[Pointer]{ex.Message}");
            }
        }
    }
}
