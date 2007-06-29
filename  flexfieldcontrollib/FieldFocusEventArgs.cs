using System;


namespace FlexFieldControlLib
{
   internal class FieldFocusEventArgs : EventArgs
   {
      /*
      public int FieldIndex
      {
         get
         {
            return _fieldIndex;
         }
         set
         {
            _fieldIndex = value;
         }
      }
       * */

      public FocusEventType FocusEventType
      {
         get
         {
            return _focusEventType;
         }
         set
         {
            _focusEventType = value;
         }
      }

      //private int _fieldIndex;
      private FocusEventType _focusEventType;
   }

   internal enum FocusEventType
   {
      GotFocus,
      LostFocus
   }
}
