using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

using FlexFieldControlLib;

namespace FlexControls
{
   public class IPAddressPortControl : FlexFieldControl
   {
      public IPAddressPortControl()
      {
         base.FieldCount = 5;

         SetSeparatorText( 0, String.Empty );
         SetSeparatorText( 1, "." );
         SetSeparatorText( 2, "." );
         SetSeparatorText( 3, "." );
         SetSeparatorText( 4, " : " );
         SetSeparatorText( FieldCount, String.Empty );

         for ( int i = 0; i < 4; ++i )
         {
            SetRange( i, 0, 255 );
         }

         SetMaxLength( 4, 5 );
         SetRange( 4, 0, 65535 );

         KeyEventArgs e = new KeyEventArgs( Keys.OemPeriod );
         AddCedeFocusKey( 0, e );
         AddCedeFocusKey( 1, e );
         AddCedeFocusKey( 2, e );
         e = new KeyEventArgs( Keys.Decimal );
         AddCedeFocusKey( 0, e );
         AddCedeFocusKey( 1, e );
         AddCedeFocusKey( 2, e );

         e = new KeyEventArgs( Keys.OemSemicolon );
         AddCedeFocusKey( 3, e );

         Size = MinimumSize;
      }
   }
}