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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace FlexFieldControlLib
{
   /// <summary>
   /// An abstract base for a numeric fielded control.
   /// </summary>
   [DesignerAttribute( typeof( FlexFieldControlDesigner ) )]
   public abstract partial class FlexFieldControl : UserControl
   {
      #region Public Events

      /// <summary>
      /// Raised when the text of any field changes.
      /// </summary>
      public event EventHandler<FieldChangedEventArgs> FieldChangedEvent;
      /// <summary>
      /// Raised when the text of any field is validated, such as when focus
      /// changes from one field to another.
      /// </summary>
      public event EventHandler<FieldValidatedEventArgs> FieldValidatedEvent;

      #endregion  // Public Events

      #region Public Properties

      /// <summary>
      /// Gets or sets a value indicating whether the control is automatically
      /// sized vertically according to the current font and border. Default is
      /// true.
      /// </summary>
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

      /// <summary>
      /// Returns the text baseline. Used by FlexFieldControlDesigner to 
      /// indicate the baseline when control is used in design mode.
      /// </summary>
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

      /// <summary>
      /// Gets whether every field in the control is blank.
      /// </summary>
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

      /// <summary>
      /// Gets or sets the border style for the control. Default is
      /// BorderStyle.Fixed3D.
      /// </summary>
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
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets or sets the number of fields in the control. Default is 3;
      /// minimum is 1. Setting this value resets every field and separator
      /// to its default state.
      /// </summary>
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
               Size = MinimumSize;
            }
         }
      }

      /// <summary>
      /// Gets the minimum size for the control.
      /// </summary>
      public new Size MinimumSize
      {
         get
         {
            return CalculateMinimumSize();
         }
      }

      /// <summary>
      /// Gets or sets a value indicating whether the contents of the control
      /// can be changed. 
      /// </summary>
      public bool ReadOnly
      {
         get
         {
            return _readOnly;
         }
         set
         {
            _readOnly = value;

            foreach ( FieldControl fc in _fieldControls )
            {
               fc.ReadOnly = _readOnly;
            }

            foreach ( SeparatorControl sc in _separatorControls )
            {
               sc.ReadOnly = _readOnly;
            }

            Invalidate();
         }
      }

      /// <summary>
      /// Gets or sets the text of the control.
      /// </summary>
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
            Parse( value );
         }
      }

      #endregion  // Public Properties

      #region Public Methods

      /// <summary>
      /// Adds a KeyEventArgs to every field that indicates when a field should
      /// cede focus to the next field in the control. By default, each field
      /// has a single cede focus key -- the [Space] key. 
      /// </summary>
      /// <param name="e"><see cref="System.Windows.Forms.Keys">KeyCode</see>
      /// indicates which keyboard key will cede focus.</param>
      public void AddCedeFocusKey( KeyEventArgs e )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.AddCedeFocusKey( e );
         }
      }

      /// <summary>
      /// Adds a KeyEventArgs to a specific field that indicates when a field
      /// should cede focus to the next field in the control. By default, each
      /// field has a single cede focus key -- the [Space] key. 
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="e"></param>
      /// <returns></returns>
      public bool AddCedeFocusKey( int fieldIndex, KeyEventArgs e )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].AddCedeFocusKey( e );
         }

         return false;
      }

      /// <summary>
      /// Clears all content from the fields in the control.
      /// </summary>
      public void Clear()
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.Clear();
         }
      }

      /// <summary>
      /// Removes every cede focus key from every field in the control.
      /// </summary>
      public void ClearCedeFocusKeys()
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ClearCedeFocusKeys();
         }
      }

      /// <summary>
      /// Removes every cede focus key from a specific field in the control.
      /// </summary>
      /// <param name="fieldIndex"></param>
      public void ClearCedeFocusKeys( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ClearCedeFocusKeys();
         }
      }

      /// <summary>
      /// Gets the high inclusive boundary of allowed values for a specific
      /// field. The default value varies based on ValueFormat and MaxLength,
      /// but it is always the maximum value that can be represented by the
      /// field.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <returns></returns>
      public int GetRangeHigh( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].RangeHigh;
         }

         return 0;
      }

      /// <summary>
      /// Gets the low inclusive boundary of allowed values for a specific
      /// field. Default value is 0.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <returns></returns>
      public int GetRangeLow( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].RangeLow;
         }

         return 0;
      }

      /// <summary>
      /// Gets the value of a specific field. If the field is blank, its value
      /// is the same as its low range value.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <returns></returns>
      public int GetValue( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].Value;
         }

         return 0;
      }

      /// <summary>
      /// Gets an array of values for the entire control. If any field is blank,
      /// its value is the same as its low range value.
      /// </summary>
      /// <returns></returns>
      public int[] GetValues()
      {
         int[] values = new int[FieldCount];

         for ( int index = 0; index < FieldCount; ++index )
         {
            values[index] = _fieldControls[index].Value;
         }

         return values;
      }

      /// <summary>
      /// Determines if a specific field has input focus. True indicates that
      /// the field has focus.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <returns></returns>
      public bool HasFocus( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].Focused;
         }

         return false;
      }

      /// <summary>
      /// Determines if every field in the control is blank. True indicates that
      /// every field in the control is blank.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <returns></returns>
      public bool IsBlank( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            return _fieldControls[fieldIndex].Blank;
         }

         return false;
      }

      /// <summary>
      /// Removes every cede focus key from every field, and adds the default
      /// cede focus key -- the [Space] key -- to every field.
      /// </summary>
      public void ResetCedeFocusKeys()
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ResetCedeFocusKeys();
         }
      }

      /// <summary>
      /// Removes every cede focus key from a specific field, and adds the
      /// default cede focus key -- the [Space] key -- to the field.
      /// </summary>
      /// <param name="fieldIndex"></param>
      public void ResetCedeFocusKeys( int fieldIndex )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ResetCedeFocusKeys();
         }
      }

      /// <summary>
      /// Sets the character casing for every field in the control.
      /// </summary>
      /// <param name="casing"></param>
      public void SetCasing( CharacterCasing casing )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.CharacterCasing = casing;
            fc.Size = fc.MinimumSize;
         }
      }

      /// <summary>
      /// Sets the character casing for a specific field in the control.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="casing"></param>
      public void SetCasing( int fieldIndex, CharacterCasing casing )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].CharacterCasing = casing;
            _fieldControls[fieldIndex].Size = _fieldControls[fieldIndex].MinimumSize;
         }
      }

      /// <summary>
      /// Sets the maximum length for every field in the control. Default value
      /// is 3.
      /// </summary>
      /// <param name="maxLength"></param>
      public void SetMaxLength( int maxLength )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.MaxLength = maxLength;
         }
      }

      /// <summary>
      /// Sets the maximum length for a specific field in the control. Default
      /// value is 3.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="maxLength"></param>
      public void SetMaxLength( int fieldIndex, int maxLength )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].MaxLength = maxLength;
         }
      }

      /// <summary>
      /// Toggles whether the value for every field is displayed with leading
      /// zeros.
      /// </summary>
      /// <param name="leadingZeros"><code>true</code> indicates that the value
      /// is displayed with leading zeros.</param>
      public void SetLeadingZeros( bool leadingZeros )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.LeadingZeros = leadingZeros;
         }
      }

      /// <summary>
      /// Toggles whether the value for a specific field is displayed with
      /// leading zeros.
      /// </summary>
      /// <param name="fieldIndex">Zero-based index for field.</param>
      /// <param name="leadingZeros"><c>true</c> indicates value should be
      /// displayed with leading zeros.</param>
      public void SetLeadingZeros( int fieldIndex, bool leadingZeros )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].LeadingZeros = leadingZeros;
         }
      }

      /// <summary>
      /// Sets the low and high range for every field.
      /// </summary>
      /// <param name="low"></param>
      /// <param name="high"></param>
      public void SetRange( int low, int high )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.RangeLow = low;
            fc.RangeHigh = high;
         }
      }

      /// <summary>
      /// Sets the low and high range for a specific field.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="low"></param>
      /// <param name="high"></param>
      public void SetRange( int fieldIndex, int low, int high )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].RangeLow = low;
            _fieldControls[fieldIndex].RangeHigh = high;
         }
      }

      /// <summary>
      /// Sets the text for every separator.
      /// </summary>
      /// <param name="text"></param>
      public void SetSeparatorText( string text )
      {
         foreach ( SeparatorControl sc in _separatorControls )
         {
            sc.Text = text;
         }

         Size = MinimumSize;
      }

      /// <summary>
      /// Sets the text for a specific separator.
      /// </summary>
      /// <param name="separatorIndex"></param>
      /// <param name="text"></param>
      public void SetSeparatorText( int separatorIndex, string text )
      {
         if ( IsValidSeparatorIndex( separatorIndex ) )
         {
            _separatorControls[separatorIndex].Text = text;
            Size = MinimumSize;
         }
      }

      /// <summary>
      /// Sets the value for every field.
      /// </summary>
      /// <param name="value"></param>
      public void SetValue( int value )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.Value = value;
         }
      }

      /// <summary>
      /// Sets the value for a specific field.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="value"></param>
      public void SetValue( int fieldIndex, int value )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].Value = value;
         }
      }

      /// <summary>
      /// Sets the value format for every field.
      /// </summary>
      /// <param name="format"></param>
      public void SetValueFormat( ValueFormat format )
      {
         foreach ( FieldControl fc in _fieldControls )
         {
            fc.ValueFormat = format;
         }
      }

      /// <summary>
      /// Sets the value format for a specific field.
      /// </summary>
      /// <param name="fieldIndex"></param>
      /// <param name="format"></param>
      public void SetValueFormat( int fieldIndex, ValueFormat format )
      {
         if ( IsValidFieldIndex( fieldIndex ) )
         {
            _fieldControls[fieldIndex].ValueFormat = format;
         }
      }

      /// <summary>
      /// Converts the text of every separator and every field to a single
      /// string.
      /// </summary>
      /// <returns></returns>
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

      /// <summary>
      /// The constructor.
      /// </summary>
      protected FlexFieldControl()
      {
         InitializeComponent();

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

      #endregion   // Constructor

      #region Protected Properties

      /// <summary>
      /// Gets the default size for the control.
      /// </summary>
      protected override Size DefaultSize
      {
         get
         {
            return CalculateMinimumSize();
         }
      }

      #endregion  // Protected Properties

      #region Protected Methods

      /// <summary>
      /// Raises the BackColorChanged event.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnBackColorChanged( EventArgs e )
      {
         base.OnBackColorChanged( e );

         _backColorChanged = true;
      }

      /// <summary>
      /// Adjusts the size of the control when font is changed.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnFontChanged( EventArgs e )
      {
         Point origin = Location;
         base.OnFontChanged( e );
         Location = origin;
         Size = MinimumSize;
         AdjustSize();
      }

      /// <summary>
      /// Sets focus to the first field when the control gets focus.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnGotFocus( EventArgs e )
      {
         base.OnGotFocus( e );
         _fieldControls[0].TakeFocus( Direction.Forward, Selection.All, Action.None );
      }

      /// <summary>
      /// Sets the cursor to I-beam when mouse is over control.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnMouseEnter( EventArgs e )
      {
         base.OnMouseEnter( e );
         Cursor = Cursors.IBeam;
      }

      /// <summary>
      /// Clears the background and fills it with background color. If control
      /// has a border, draws it.
      /// </summary>
      /// <param name="e"></param>
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

      /// <summary>
      /// Ensures that any size change of control is constrained by allowed
      /// range.
      /// </summary>
      /// <param name="e"></param>
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

         base.BackColor = SystemColors.Window;

         _backColorChanged = false;

         for ( int index = 0; index < FieldCount; ++index )
         {
            FieldControl fc = new FieldControl();

            fc.CedeFocusEvent += new EventHandler<CedeFocusEventArgs>( OnFocusCeded );
            fc.FieldChangedEvent += new EventHandler<FieldChangedEventArgs>( OnFieldChanged );
            fc.FieldId = index;
            fc.FieldSizeChangedEvent += new EventHandler<EventArgs>( OnFieldSizeChanged );
            fc.FieldValidatedEvent += new EventHandler<FieldValidatedEventArgs>( OnFieldValidated );
            fc.Name = Properties.Resources.FieldControlName + index.ToString( CultureInfo.InvariantCulture );
            fc.Parent = this;
            fc.ReadOnly = ReadOnly;

            _fieldControls.Add( fc );

            Controls.Add( fc );
         }

         for ( int index = 0; index < ( FieldCount + 1 ); ++index )
         {
            SeparatorControl sc = new SeparatorControl();

            sc.Name = Properties.Resources.SeparatorControlName + index.ToString( CultureInfo.InvariantCulture );
            sc.Parent = this;
            sc.ReadOnly = ReadOnly;
            sc.SeparatorSizeChangedEvent += new EventHandler<EventArgs>( OnSeparatorSizeChanged );

            _separatorControls.Add( sc );

            Controls.Add( sc );
         }

         for ( int index = 0; index < _separatorControls.Count; ++index )
         {
            string text;

            if ( index == 0 )
            {
               text = "<";
            }
            else if ( index == ( _separatorControls.Count - 1 ) )
            {
               text = ">";
            }
            else
            {
               text = "><";
            }

            _separatorControls[index].Text = text;
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

      private void OnFieldValidated( object sender, FieldValidatedEventArgs e )
      {
         if ( FieldValidatedEvent != null )
         {
            FieldValidatedEvent( this, e );
         }
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

      private void Parse( string text )
      {
         if ( text == null )
         {
            return;
         }

         StringBuilder sb = new StringBuilder();

         for ( int index = 0; index < FieldCount; ++index )
         {
            sb.Append( _separatorControls[index].RegExString );

            sb.Append( "(" );
            sb.Append( _fieldControls[index].RegExString );
            sb.Append( ")" );
         }

         sb.Append( _separatorControls[FieldCount].RegExString );

         Regex regex = new Regex( sb.ToString() );

         Match match = regex.Match( text );

         if ( match.Success )
         {
            if ( match.Groups.Count == ( FieldCount + 1 ) )
            {
               for ( int index = 0; index < FieldCount; ++index )
               {
                  _fieldControls[index].Text = match.Groups[index + 1].Value;
               }
            }
         }
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

      private bool _readOnly;

      private bool _backColorChanged;

      #endregion  Private Data
   }
}