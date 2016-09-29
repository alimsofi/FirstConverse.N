using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FirstConverse.N.Droid
{
    public static class Cheeses
    {
        private static Random RANDOM = new Random();
        
        public static int RandomCheeseDrawable
        {
            get
            {
                switch (RANDOM.Next(5))
                {
                    default:
                    case 0:
                        return Resource.Drawable.material_back_drop_1;
                    case 1:
                        return Resource.Drawable.material_back_drop_2;
                    case 2:
                        return Resource.Drawable.material_back_drop_3;
                    case 3:
                        return Resource.Drawable.material_back_drop_4;
                    case 4:
                        return Resource.Drawable.material_back_drop_5;
                }
            }
        }        
        public static List<string> CheeseStrings
        {
            get
            {
                return new List<string>() { };
            }        
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                // Calculate ratios of height and width to requested height and
                // width
                int heightRatio = height / reqHeight;
                int widthRatio = width / reqWidth;

                // Choose the smallest ratio as inSampleSize value, this will
                // guarantee
                // a final image with both dimensions larger than or equal to the
                // requested height and width.
                inSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
            }

            return inSampleSize;
        }
    }
}