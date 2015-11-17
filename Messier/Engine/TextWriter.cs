using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDIGraphics = System.Drawing.Graphics;

namespace Messier.Engine
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    /// <summary>
    /// Converts strings into renderable textures
    /// </summary>
    public class TextWriter : IDisposable
    {
        static PrivateFontCollection fonts = new PrivateFontCollection();

        public static void AddFont(Stream font)
        {
            byte[] fnt = new byte[font.Length];
            font.Read(fnt, 0, fnt.Length);
            unsafe
            {
                fixed(byte *fontData = fnt)
                {
                    fonts.AddMemoryFont((IntPtr)fontData, fnt.Length);
                }
            }
        }

        public static void AddFont(string path)
        {
            fonts.AddFontFile(path);
        }

        public static TextWriter CreateWriter(string family, FontStyle style)
        {
            return new TextWriter(family, style);
        }


        string familyName;
        private TextWriter(string family, FontStyle s)
        {
            familyName = family + " ";

            if (s.HasFlag(FontStyle.Regular)) familyName += "Regular";
            if (s.HasFlag(FontStyle.Bold)) familyName += "Bold";
            if (s.HasFlag(FontStyle.Italic)) familyName += "Italic";
            if (s.HasFlag(FontStyle.Underline)) familyName += "Underline";
            if (s.HasFlag(FontStyle.Strikeout)) familyName += "Strikeout";
        }

        public BitmapTextureSource Write(string s, float size, Color fg)
        {
            Bitmap tmp = new Bitmap(1, 1);
            StringFormat fmt = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            GDIGraphics tmpG = GDIGraphics.FromImage(tmp);
            Font fnt = new Font(familyName, size);
            SizeF si = tmpG.MeasureString(s, fnt);
            tmpG.Dispose();
            tmp.Dispose();

            Bitmap bmp = new Bitmap((int)si.Width, (int)si.Height);
            bmp.MakeTransparent();

            tmpG = GDIGraphics.FromImage(bmp);
            tmpG.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            tmpG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

            tmpG.DrawString(s, fnt, new SolidBrush(fg), 0, 0);
            tmpG.Flush();
            tmpG.Dispose();

            return new BitmapTextureSource(bmp, 0);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TextWriter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
