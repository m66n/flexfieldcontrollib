// Copyright (c) 2007 Michael Chapman
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace FlexFieldControlLib
{
   [DesignerAttribute( typeof( FlexFieldControlDesigner ) )]
   public abstract partial class FlexFieldControl : UserControl
   {
      #region Public Events

      public event EventHandler<FieldChangedEventArgs> FieldChangedEvent;

      #endregion  // Public Events

      #region Public Properties

      [Browsable( true )]
      public bool AutoHeight
      {
         get
         {
            return _autoHeight;
         }
         set
         {
            if ( _autoHeight != value )
            {
               _autoHeight = value;
               if ( _autoHeight )
               {
                  AdjustSize();
               }
            }
         }
      }

      public int Baseline
      {
         get
         {
            NativeMethods.TEXTMETRIC textMetric = GetTextMetrics( Handle, Font );

            int offset = textMetric.tmAscent + 1;

            switch ( BorderStyle )
            {
               case BorderStyle.Fixed3D:
                  offset += Fixed3DOffset.Height;
                  break;
               case BorderStyle.FixedSingle:
                  offset += FixedSingleOffset.Height;
                  break;
            }

            return offset;
         }
      }

      public bool Blank
      {
         get
         {
            foreach ( FieldControl fc in _fieldControls )
            {
               if ( !fc.Blank )
               {
                  return false;
               }
            }

            return true;
         }
      }

      public new BorderStyle BorderStyle
      {
         get
         {
            return _borderStyle;
         }
         set
         {
            if ( _borderStyle != value )
            {
               _borderStyle = value;
               AdjustSize();
            }
         }
      }

      protected override Size DefaultSize
      {
         get
         {
            return CalculateMinimumSize();
         }
      }

      public int FieldCount
      {
         get
         {
            return _fieldCount;
         }
         set
         {
            if ( value < 1 )
            {
               value = 1;
            }

            if ( value != _fieldCount )
            {
               _fieldCount = value;
               InitializeControls();
            }
         }
      }

      public new Size MinimumSize
      {
         get
         {
            return CalculateMinimumSize();
         }
      }

      public override string Text
      {
         get
         {
            StringBuilder sb = new StringBuilder();

            for ( int index = 0; index < FieldCount; ++index )
            {
               sb.Append( _separatorControls[index].Text );
               sb.Append( _fieldControls[index].Text );
            }

            sb.Append( _separatorControls[FieldCount].Text );

            return sb.ToString();

         }
         set
         {
            base.Text = value;
         }
      }

      #endregion  // Public Properties

      #region Public Methods

      public void AddCedeFocusKey( KeyEventArgs e )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.AddCedeFocusKey( e );
         }
      }

      public bool AddCedeFocusKey( int fieldIndex, KeyEventArgs e )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].AddCedeFocusKey( e );
         }

         return false;
      }

      public void ClearCedeFocusKeys()
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ClearCedeFocusKeys();
         }
      }

      public void ClearCedeFocusKeys( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ClearCedeFocusKeys();
         }
      }

      public bool IsFieldBlank( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].Blank;
         }

         return false;
      }

      public void ResetCedeFocusKeys()
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ResetCedeFocusKeys();
         }
      }

      public void ResetCedeFocusKeys( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ResetCedeFocusKeys();
         }
      }

      public void SetCasing( CharacterCasing casing )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.CharacterCasing = casing;
            fc.Size = fc.MinimumSize;
         }
      }

      public void SetCasing( int fieldIndex, CharacterCasing casing )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].CharacterCasing = casing;
            _fieldControls[fieldIndex].Size = _fieldControls[fieldIndex].MinimumSize;
         }
      }

      public void SetMaxLength( int maxLength )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.MaxLength = maxLength;
         }
      }

      public void SetMaxLength( int fieldIndex, int maxLength )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].MaxLength = maxLength;
         }
      }

      public void SetLeadingZeroes( bool leadingZeroes )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.LeadingZeroes = leadingZeroes;
         }
      }

      public void SetLeadingZeroes( int fieldIndex, bool leadingZeroes )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].LeadingZeroes = leadingZeroes;
         }
      }

      public void SetMinimizeWidth( bool minimizeWidth )
      {
         foreach ( SeparatorControl sc in _separatorControls )
         {
            sc.MinimizeWidth = minimizeWidth;
         }
      }

      public void SetMinimizeWidth( int separatorIndex, bool minimizeWidth )
      {
         if ( IsValidSeparatorIndex( separatorIndex ) )
         {
            _separatorControls[separatorIndex].MinimizeWidth = minimizeWidth;
         }
      }

      public void SetRange( int low, int high )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.RangeLow = low;
            fc.RangeHigh = high;
         }
      }

      public void SetRange( int fieldIndex, int low, int high )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].RangeLow = low;
            _fieldControls[fieldIndex].RangeHigh = high;
         }
      }

      public void SetSeparatorText( string text )
      {
         foreach ( SeparatorControl sc in _separatorControls )
         {
            sc.Text = text;
         }
      }

      public void SetSeparatorText( int separatorIndex, string text )
      {
         if ( IsValidSeparatorIndex( separatorIndex ) )
         {
            _separatorControls[separatorIndex].Text = text;
         }
      }

      public void SetValueFormat( ValueFormat format )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ValueFormat = format;
         }
      }

      public void SetValueFormat( int fieldIndex, ValueFormat format )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ValueFormat = format;
         }
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();

         for ( int index = 0; index < FieldCount; ++index )
         {
            sb.Append( _separatorControls[index].ToString() );
            sb.Append( _fieldControls[index].ToString() );
         }

         sb.Append( _separatorControls[FieldCount].ToString() );

         return sb.ToString();
      }

      #endregion  // Public Methods

      #region Constructor

      protected FlexFieldControl()
      {
         InitializeComponent();

         base.BackColor = SystemColors.Window;

         InitializeControls();

         SetStyle( ControlStyles.AllPaintingInWmPaint, true );
         SetStyle( ControlStyles.ContainerControl, true );
         SetStyle( ControlStyles.OptimizedDoubleBuffer, true );
         SetStyle( ControlStyles.ResizeRedraw, true );
         SetStyle( ControlStyles.Selectable, true );
         SetStyle( ControlStyles.UserPaint, true );

         Size = MinimumSize;

         LayoutControls();
      }

      #endregion     // Constructor

      #region Protected Methods

      protected override void OnFontChanged( EventArgs e )
      {
         Point origin = Location;
         base.OnFontChanged( e );
         Location = origin;
         Size = MinimumSize;
         AdjustSize();
      }

      protected override void OnGotFocus( EventArgs e )
      {
         base.OnGotFocus( e );
         _fieldControls[0].TakeFocus( Direction.Forward, Selection.All, Action.None );
      }

      protected override void OnMouseEnter( EventArgs e )
      {
         base.OnMouseEnter( e );
         Cursor = Cursors.IBeam;
      }

      protected override void OnPaint( PaintEventArgs e )
      {
         base.OnPaint( e );

         Color backColor = BackColor;

         e.Graphics.FillRectangle( new SolidBrush( backColor ), ClientRectangle );

         Rectangle rectBorder = new Rectangle( ClientRectangle.Left, ClientRectangle.Top,
            ClientRectangle.Width - 1, ClientRectangle.Height - 1 );

         switch ( BorderStyle )
         {
            case BorderStyle.Fixed3D:

               if ( Application.RenderWithVisualStyles )
               {
                  ControlPaint.DrawVisualStyleBorder( e.Graphics, rectBorder );
               }
               else
               {
                  ControlPaint.DrawBorder3D( e.Graphics, ClientRectangle, Border3DStyle.Sunken );
               }
               break;

            case BorderStyle.FixedSingle:

               ControlPaint.DrawBorder( e.Graphics, ClientRectangle,
                  SystemColors.WindowFrame, ButtonBorderStyle.Solid );
               break;
         }
      }

      protected override void OnSizeChanged( EventArgs e )
      {
         base.OnSizeChanged( e );

         AdjustSize();
      }

      #endregion     // Protected Methods

      #region Private Methods

      private void AdjustSize()
      {
         Size minSize = MinimumSize;

         int newWidth = minSize.Width;
         int newHeight = minSize.Height;

         if ( Width > newWidth )
         {
            newWidth = Width;
         }

         if ( ( Height > newHeight ) && !AutoHeight )
         {
            newHeight = Height;
         }

         Size = new Size( newWidth, newHeight );

         LayoutControls();
      }

      private Size CalculateMinimumSize()
      {
         Size minimumSize = new Size();

         foreach ( FieldControl fc in _fieldControls )
         {
            minimumSize.Width += fc.Width;
            minimumSize.Height = Math.Max( minimumSize.Height, fc.Height );
         }

         foreach ( SeparatorControl sc in _separatorControls )
         {
            minimumSize.Width += sc.Width;
            minimumSize.Height = Math.Max( minimumSize.Height, sc.Height );
         }

         switch ( BorderStyle )
         {
            case BorderStyle.Fixed3D:
               minimumSize.Width += ( 2 * Fixed3DOffset.Width );
               minimumSize.Height = GetSuggestedHeight();
               break;

            case BorderStyle.FixedSingle:
               minimumSize.Width += ( 2 * FixedSingleOffset.Width );
               minimumSize.Height = GetSuggestedHeight();
               break;
         }

         return minimumSize;
      }

      private void Cleanup()
      {
         foreach ( SeparatorControl sc in _separatorControls )
         {
            Controls.Remove( sc );
         }

         foreach ( FieldControl fc in _fieldControls )
         {
            Controls.Remove( fc );
         }

         _separatorControls.Clear();
         _fieldControls.Clear();
      }

      private int GetSuggestedHeight()
      {
         _referenceTextBox.AutoSize = true;
         _referenceTextBox.BorderStyle = BorderStyle;
         _referenceTextBox.Font = Font;
         return _referenceTextBox.Height;
      }

      private static NativeMethods.TEXTMETRIC GetTextMetrics( IntPtr hwnd, Font font )
      {
         IntPtr hdc = NativeMethods.GetWindowDC( hwnd );

         NativeMethods.TEXTMETRIC textMetric;
         IntPtr hFont = font.ToHfont();

         try
         {
            IntPtr hFontPreviouse = NativeMethods.SelectObject( hdc, hFont );
            NativeMethods.GetTextMetrics( hdc, out textMetric );
            NativeMethods.SelectObject( hdc, hFontPreviouse );
         }
         finally
         {
            NativeMethods.ReleaseDC( hwnd, hdc );
            NativeMethods.DeleteObject( hFont );
         }

         return textMetric;
      }

      private void InitializeControls()
      {
         Cleanup();

         for ( int index = 0; index < FieldCount; ++index )
         {
            FieldControl fc = new FieldControl();

            fc.CedeFocusEvent += new EventHandler<CedeFocusEventArgs>( OnFocusCeded );
            fc.FieldChangedEvent += new EventHandler<FieldChangedEventArgs>( OnFieldChanged );
            fc.FieldId = index;
            fc.FieldSizeChangedEvent += new EventHandler<EventArgs>( OnFieldSizeChanged );
            fc.Name = Properties.Resources.FieldControlName + index.ToString( CultureInfo.InvariantCulture );
            fc.Parent = this;

            _fieldControls.Add( fc );

            Controls.Add( fc );
         }

         for ( int index = 0; index < ( FieldCount + 1 ); ++index )
         {
            SeparatorControl sc = new SeparatorControl();

            sc.Name = Properties.Resources.SeparatorControlName + index.ToString( CultureInfo.InvariantCulture );
            sc.Parent = this;
            sc.SeparatorSizeChangedEvent += new EventHandler<EventArgs>( OnSeparatorSizeChanged );

            _separatorControls.Add( sc );

            Controls.Add( sc );
         }
      }

      private bool IsValidFieldIndex( int fieldIndex )
      {
         if ( fieldIndex >= 0 && fieldIndex < _fieldControls.Count )
         {
            return true;
         }

         return false;
      }

      private bool IsValidSeparatorIndex( int separatorIndex )
      {
         Debug.Assert( false );
         if ( separatorIndex >= 0 && separatorIndex < _separatorControls.Count )
         {
            return true;
         }


         return false;
      }

      private void LayoutControls()
      {
         SuspendLayout();

         int difference = Size.Width - MinimumSize.Width;

         Debug.Assert( difference >= 0 );

         int offsetCount = 2 * FieldCount;

         int div = difference / offsetCount;
         int mod = difference % offsetCount;

         int[] offsets = new int[offsetCount];

         for ( int index = 0; index < offsetCount; ++index )
         {
            offsets[index] = div;

            if ( index < mod )
            {
               ++offsets[index];
            }
         }

         int x = 0;
         int y = 0;

         switch ( BorderStyle )
         {
            case BorderStyle.Fixed3D:
               x = Fixed3DOffset.Width;
               y = Fixed3DOffset.Height;
               break;

            case BorderStyle.FixedSingle:
               x = FixedSingleOffset.Width;
               y = FixedSingleOffset.Height;
               break;
         }

         for ( int index = 0; index < FieldCount; ++index )
         {
            _separatorControls[index].Location = new Point( x, y );
            x += _separatorControls[index].Width;

            x += offsets[2 * index];

            _fieldControls[index].Location = new Point( x, y );
            x += _fieldControls[index].Width;

            x += offsets[2 * ( FieldCount - index ) - 1];
         }

         _separatorControls[FieldCount].Location = new Point( x, y );

         ResumeLayout( false );
      }

      private void OnFieldChanged( object sender, FieldChangedEventArgs e )
      {
         if ( FieldChangedEvent != null )
         {
            FieldChangedEvent( this, e );
         }

         OnTextChanged( EventArgs.Empty );
      }

      private void OnFieldSizeChanged( object sender, EventArgs e )
      {
         AdjustSize();
         Invalidate();
      }

      private void OnFocusCeded( object sender, CedeFocusEventArgs e )
      {
         switch ( e.Action )
         {
            case Action.Home:

               _fieldControls[0].TakeFocus( e.Direction, e.Selection, e.Action );
               return;

            case Action.End:

               _fieldControls[FieldCount - 1].TakeFocus( e.Direction, e.Selection, e.Action );
               return;

            case Action.Trim:

               if ( e.FieldId == 0 )
               {
                  return;
               }

               _fieldControls[e.FieldId - 1].TakeFocus( e.Direction, e.Selection, e.Action );
               return;
         }

         if ( ( e.Direction == Direction.Reverse && e.FieldId == 0 ) ||
              ( e.Direction == Direction.Forward && e.FieldId == ( FieldCount - 1 ) ) )
         {
            return;
         }

         int fieldId = e.FieldId;

         if ( e.Direction == Direction.Forward )
         {
            ++fieldId;
         }
         else
         {
            --fieldId;
         }

         _fieldControls[fieldId].TakeFocus( e.Direction, e.Selection, e.Action );
      }

      private void OnSeparatorSizeChanged( object sender, EventArgs e )
      {
         AdjustSize();
         Invalidate();
      }

      #endregion     // Private Methods

      #region Private Data

      private TextBox _referenceTextBox = new TextBox();

      private int _fieldCount = 3;

      private bool _autoHeight = true;

      private List<FieldControl> _fieldControls = new List<FieldControl>();
      private List<SeparatorControl> _separatorControls = new List<SeparatorControl>();

      private Size Fixed3DOffset = new Size( 3, 3 );
      private Size FixedSingleOffset = new Size( 2, 2 );

      private BorderStyle _borderStyle = BorderStyle.Fixed3D;

      #endregion  Private Data
   }
}