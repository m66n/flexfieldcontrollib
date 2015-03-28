using System;
using System.Windows.Forms;

using FlexFieldControlLib;


namespace FlexControls
{
  class MACAddressControl : FlexFieldControl
  {
    public MACAddressControl()
    {
      // the format of this control is 'FF:FF:FF:FF:FF:FF'

      // set FieldCount first
      //
      FieldCount = 6;

      // every field is 2 digits max
      //
      SetMaxLength(2);

      // every separator is ':'...
      //
      SetSeparatorText(":");

      // except for the first and last separators
      //
      SetSeparatorText(0, String.Empty);
      SetSeparatorText(FieldCount, String.Empty);

      // the value format is hexadecimal
      //
      SetValueFormat(ValueFormat.Hexadecimal);

      // use leading zeros for every field
      //
      SetLeadingZeros(true);

      // use uppercase only
      //
      SetCasing(CharacterCasing.Upper);

      // add ':' key to cede focus for every field
      //
      KeyEventArgs e = new KeyEventArgs(Keys.OemSemicolon);
      AddCedeFocusKey(e);

      // this should be the last thing
      //
      Size = MinimumSize;
    }
  }
}