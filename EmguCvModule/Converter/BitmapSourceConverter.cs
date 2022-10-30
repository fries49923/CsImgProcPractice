//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EmguCvModule
{
    //public static class BitmapSourceExtension
    public static class BitmapSourceConverter
    {
        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The pointer to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSourceOld(this IInputArray image)
        {
            using (InputArray ia = image.GetInputArray())
            using (Mat m = ia.GetMat())
            using (System.Drawing.Bitmap source = m.ToBitmap())
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this IInputArray image)
        {
            using (InputArray ia = image.GetInputArray())
            using (Mat m = ia.GetMat())
            using (System.Drawing.Bitmap source = m.ToBitmap())
            {
                PixelFormat pixelFormat;
                BitmapPalette palette = null;
                if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    pixelFormat = PixelFormats.Bgra32;
                }
                else if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                {
                    pixelFormat = PixelFormats.Bgr32;
                }
                else if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    pixelFormat = PixelFormats.Bgr24;
                }
                else if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    pixelFormat = PixelFormats.Indexed8;
                    palette = BitmapPalettes.Gray256;
                }
                else
                {
                    throw new Exception(string.Format("Conversion from Bitmap of format {0} is not supported.", source.PixelFormat));
                }

                System.Drawing.Imaging.BitmapData bitmapData =
                    source.LockBits(
                        new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        source.PixelFormat);

                BitmapSource bitmapSource =
                    BitmapSource.Create(
                        bitmapData.Width,
                        bitmapData.Height,
                        source.HorizontalResolution,
                        source.VerticalResolution,
                        pixelFormat,
                        palette,
                        bitmapData.Scan0,
                        bitmapData.Stride * bitmapData.Height,
                        bitmapData.Stride);

                source.UnlockBits(bitmapData);

                return bitmapSource;
            }
        }

        /// <summary>
        /// Convert a BitmapSource into a Mat
        /// </summary>
        /// <param name="source">The Bitmap source</param>
        /// <param name="image">The resulting Mat</param>
        /// <exception cref="Exception">Will throw exception when we cannot handle the BitmapSource's PixelFormat</exception>
        public static void ToMat(this BitmapSource source, Mat image)
        {
            if (source.Format == PixelFormats.Bgra32
                || source.Format == PixelFormats.Bgr32)
            {
                image.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
                source.CopyPixels(Int32Rect.Empty, image.DataPointer, image.Step * image.Rows, image.Step);
            }
            else if (source.Format == PixelFormats.Bgr24)
            {
                image.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 3);
                source.CopyPixels(Int32Rect.Empty, image.DataPointer, image.Step * image.Rows, image.Step);
            }
            else if (source.Format == PixelFormats.Indexed8
                || source.Format == PixelFormats.Gray8)
            {
                image.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 1);
                source.CopyPixels(Int32Rect.Empty, image.DataPointer, image.Step * image.Rows, image.Step);
            }
            else
            {
                throw new Exception(string.Format("Conversion from BitmapSource of format {0} is not supported.", source.Format));
            }

        }

        /// <summary>
        /// Convert a BitmapSource into a Mat
        /// </summary>
        /// <param name="source">The Bitmap source</param>
        /// <returns>The resulting Mat</returns>
        public static Mat ToMat(this BitmapSource source)
        {
            Mat result = new Mat();
            source.ToMat(result);
            return result;
        }
    }
}
