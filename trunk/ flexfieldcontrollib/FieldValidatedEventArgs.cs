using System;

namespace FlexFieldControlLib
{
   public class FieldValidatedEventArgs : EventArgs
   {
      public int FieldId
      {
         get
         {
            return _fieldId;
         }
         set
         {
            _fieldId = value;
         }
      }

      public string Text
      {
         get
         {
            return _text;
         }
         set
         {
            _text = value;
         }
      }

      private int _fieldId;
      private string _text;
   }
}
