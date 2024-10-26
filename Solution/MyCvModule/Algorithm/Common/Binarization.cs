using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace MyCvModule
{
    public partial class Binarization : MyCvAlgorithmBase
    {
        #region Property

        private int _threshold;
        public int Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                if (_threshold > 255)
                {
                    _threshold = 255;
                }
                else if (_threshold < 0)
                {
                    _threshold = 0;
                }

                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private ProcessType _processType;

        #endregion

        public Binarization()
            : base()
        {
            _threshold = 100;
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
                throw new Exception($"[MyCvModule][Binarization]{ex.Message}");
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

                // Does not handle alpha
                int step = (channel > 3) ? 3 : channel;

                srcImg.CopyPixels(pixel, stride, 0);

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x += channel)
                    {
                        int idx = x + y * stride;
                        for (int i = 0; i < step; i++)
                        {
                            if (pixel[idx + i] > (byte)Threshold)
                            {
                                pixel[idx + i] = 255;
                            }
                        }
                    }
                });

                try
                {
                    srcImg.Lock();
                    srcImg.WritePixels(new Int32Rect(0, 0, width, height), pixel, stride, 0);
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
                int bufferStride = srcImg.BackBufferStride;

                try
                {
                    srcImg.Lock();

                    unsafe
                    {
                        byte* ptr = (byte*)srcImg.BackBuffer.ToPointer();
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

                            if (*ptr > (byte)Threshold)
                            {
                                *ptr = 255;
                            }

                            ptr++;
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
