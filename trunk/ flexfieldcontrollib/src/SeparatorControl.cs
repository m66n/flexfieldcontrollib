// Copyright (c) 2010 Michael Chapman
// http://flexfieldcontrollib.googlecode.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FlexFieldControlLib
{
   internal class SeparatorControl : Control
   {
      public event EventHandler<SeparatorMouseEventArgs> SeparatorMouseEvent;
      public event EventHandler<EventArgs> SeparatorSizeChangedEvent;

      public Point MidPoint
      {
         get
         {
            Point midPoint = Location;
            midPoint.Offset( Width / 2, Height / 2 );
            return midPoint;
         }
      }

      public new Size MinimumSize
      {
         get { return CalculateMinimumSize(); }
      }

      public bool ReadOnly
      {
         get { return _readOnly; }
         set
         {
            _readOnly = value;
            Invalidate();
         }
      }

      public string RegExString
      {
         get
         {
            StringBuilder sb = new StringBuilder();

            foreach ( char c in Text )
            {
               sb.Append( "[" );
               sb.Append( Regex.Escape( c.ToString() ) );
               sb.Append( "]" );
            }

            return sb.ToString();
         }
      }

      public int SeparatorIndex
      {
         get { return _separatorIndex; }
         set { _separatorIndex = value; }
      }

      public override string Text
      {
         get { return base.Text; }
         set
         {
            base.Text = value;
            Size = MinimumSize;
         }
      }

      public override string ToString()
      {
         return Text;
      }

      public SeparatorControl()
      {
         SetStyle( ControlStyles.AllPaintingInWmPaint, true );
         SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
         SetStyle( ControlStyles.UserPaint, true );

         SetStyle( ControlStyles.FixedHeight, true );
         SetStyle( ControlStyles.FixedWidth, true );

         BackColor = SystemColors.Window;

         _stringFormat = StringFormat.GenericTypographic;
         _stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

         Size = MinimumSize;
         TabStop = false;
      }

      protected override void OnFontChanged( EventArgs e )
      {
         base.OnFontChanged( e );
         Size = MinimumSize;
      }

      protected override void OnMouseDown( MouseEventArgs e )
      {
         base.OnMouseDown( e );

         if ( SeparatorMouseEvent != null )
         {
            SeparatorMouseEventArgs args = new SeparatorMouseEventArgs();

            args.Location = PointToScreen( e.Location );
            args.SeparatorIndex = SeparatorIndex;

            SeparatorMouseEvent( this, args );
         }
      }

      protected override void OnPaint( PaintEventArgs e )
      {
         base.OnPaint( e );

         Color backColor = BackColor;

         if ( !_backColorChanged )
         {
            if ( !Enabled || ReadOnly )
            {
               backColor = SystemColors.Control;
            }
         }

         Color foreColor = ForeColor;

         if ( !Enabled )
         {
            foreColor = SystemColors.GrayText;
         }
         else if ( ReadOnly )
         {
            if ( !_backColorChanged )
            {
               foreColor = SystemColors.WindowText;
            }
         }

         using ( SolidBrush backgroundBrush = new SolidBrush( backColor ) )
         {
            e.Graphics.FillRectangle( backgroundBrush, ClientRectangle );
         }
         
         using ( SolidBrush foreBrush = new SolidBrush( foreColor ) )
         {
            float x = (float)ClientRectangle.Width / 2F - _sizeText.Width / 2F;
            e.Graphics.DrawString( Text, Font, foreBrush,
               new RectangleF( x, 0F, _sizeText.Width, _sizeText.Height ), _stringFormat );
         }
      }

      protected override void OnParentBackColorChanged( EventArgs e )
      {
         base.OnParentBackColorChanged( e );

         BackColor = Parent.BackColor;

         _backColorChanged = true;
      }

      protected override void OnSizeChanged( EventArgs e )
      {
         base.OnSizeChanged( e );

         if ( SeparatorSizeChangedEvent != null )
         {
            SeparatorSizeChangedEvent( this, EventArgs.Empty );
         }
      }

      private Size CalculateMinimumSize()
      {
         using ( Graphics g = Graphics.FromHwnd( Handle ) )
         {
            _sizeText = g.MeasureString( Text, Font, -1, _stringFormat );
         }

         // MeasureString() cuts off the bottom pixel for descenders no matter
         // which StringFormatFlags are chosen
         //
         _sizeText.Height += 1F;

         return _sizeText.ToSize();
      }

      private int _separatorIndex;

      private bool _readOnly;
      private bool _backColorChanged;

      private StringFormat _stringFormat;
      private SizeF _sizeText;
   }
}
