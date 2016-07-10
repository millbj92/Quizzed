using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace Quizzed
{
    class ExtendedButton : Button
    {
        public string mAnswer;
        public ExtendedButton(Context context /*IAttributeSet attributeSet*/) : base( context /*attributeSet*/ )
    {
            Click += (s, e) =>
            {
                if (Command == null) return;
                if (!Command.CanExecute(CommandParameter)) return;
                Command.Execute(CommandParameter);
            };
        }

        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
}