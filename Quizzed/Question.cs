using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Quizzed
{
    class Question
    {
        public int time;
        public string question;
        public string answer;

        public Question()
        {
        }

        public Question(int t, string q, string a)
        {
            time = t;
            question = q;
            answer = a;
        }
    }

    class MultipleChoice : Question
    {
        public string answera;
        public string answerb;
        public string answerc;
        public string answerd;

        public MultipleChoice()
        {

        }

        public MultipleChoice(int t, string q, string a, string aa, string ab, string ac, string ad)
        {
            time = t;
            question = q;
            answer = a;
            answera = aa;
            answerb = ab;
            answerc = ac;
            answerd = ad;
        }
    }
}