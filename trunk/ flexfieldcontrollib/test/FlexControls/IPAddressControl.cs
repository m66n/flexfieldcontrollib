using System;
using System.Windows.Forms;

using FlexFieldControlLib;


namespace FlexControls
{
   class IPAddressControl : FlexFieldControl
   {
      public IPAddressControl()
      {
         FieldCount = 4;

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
