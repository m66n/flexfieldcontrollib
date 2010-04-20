using System;
using System.Globalization;
using System.Net;
using System.Windows.Forms;

using FlexFieldControlLib;


namespace FlexControls
{
   class IPAddressControl : FlexFieldControl
   {
      public new int FieldCount
      {
         get { return base.FieldCount; }
      }

      public IPAddress IPAddress
      {
         get
         {
            return new IPAddress( GetAddressBytes() );
         }
         set
         {
            SetAddressBytes( value.GetAddressBytes() );
         }
      }

      public byte[] GetAddressBytes()
      {
         byte[] bytes = new byte[FieldCount];

         for ( int index = 0; index < FieldCount; ++index )
         {
            bytes[index] = (byte)GetValue( index );
         }

         return bytes;
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1720", Justification = "Prefer to use bytes as a variable name." )]
      public void SetAddressBytes( byte[] bytes )
      {
         Clear();

         if ( bytes == null )
         {
            return;
         }

         int length = Math.Min( FieldCount, bytes.Length );

         for ( int i = 0; i < length; ++i )
         {
            SetFieldText( i, bytes[ i ].ToString( CultureInfo.InvariantCulture ) );
         }
      }

      public IPAddressControl()
      {
         base.FieldCount = 4;

         SetSeparatorText( "." );
         SetSeparatorText( 0, String.Empty );
         SetSeparatorText( FieldCount, String.Empty );

         SetRange( 0, 255 );

         KeyEventArgs e = new KeyEventArgs( Keys.OemPeriod );
         AddCedeFocusKey( e );
         e = new KeyEventArgs( Keys.Decimal );
         AddCedeFocusKey( e );

         Size = MinimumSize;
      }
   }
}
