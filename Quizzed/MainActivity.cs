using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using System.Json;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Quizzed
{
    [Activity(Label = "Quizzed", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        LinearLayout layout;
        TextView PointsText;
        TextView debugText;
        TextView countdownText;
        Timer countdownTimer;
        Timer continueTimer;

        int countdownSeconds = 0;
        int continueSeconds = 0;
        int points = 0;
        bool canClick = true;
        Question currentQuestion;
        

        List<View> viewsToRemove;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            debugText = FindViewById<TextView>(Resource.Id.debugText);
            countdownText = FindViewById<TextView>(Resource.Id.countdownText);
            PointsText = FindViewById<TextView>(Resource.Id.pointstext);
            layout = (LinearLayout)FindViewById(Resource.Id.mLayout);
            viewsToRemove = new List<View>();
            

            button.Click += delegate { GetPHP(); };

        }


        /// <summary>
        /// Misc game logic
        /// </summary>
        #region MiscFunctions

        public void GetPHP()
        {
            if (viewsToRemove.Count > 0)
                ClearViews();

            canClick = true;
            var json = "";
            try
            {
                WebClient wc = new WebClient();
                json = wc.DownloadString("http://73.104.32.120/get_question.php");
            }
            catch(WebException ex)
            {
                Console.Write(ex.Response);
            }
            
            
            

            debugText.Text = json.ToString();

            JsonValue mValue = JsonObject.Parse(json);
            JsonObject result = mValue as JsonObject;

            MakeQuestion(result);
        }

        void CheckAnswer(string answer)
        {
            if (!canClick)
                return;

            canClick = false;
            if (answer == currentQuestion.answer)
            {
                debugText.Text = "Correct!";
                countdownTimer.Stop();
                AddPoint();
                ContinueTimer();
            }

        }

        void AddPoint()
        {
            points++;

            PointsText.Text = points.ToString();
        }
        #endregion


        /// <summary>
        /// Logic for making the GUI
        /// </summary>
        #region GUI
        void ClearViews()
        {
            List<View> temp = new List<View>();
            RunOnUiThread(() =>
            {
                foreach (View v in viewsToRemove)
                {
                    layout.Invalidate();
                    layout.RemoveView(v);
                    temp.Add(v);
                }

                foreach (View v in temp)
                {
                    viewsToRemove.Remove(v);
                    viewsToRemove.TrimExcess();
                }
            });
            
        }

        void MakeQuestion(JsonObject obj)
        {
            int time = int.Parse(obj["time"]);
            string type = obj["type"];
            string question = obj["question"];
            string answer = obj["answer"];

            Question q = new Question(time, question, answer);

            if (type == "multiplechoice")
            {
                debugText.Text = "Multiple.";
                string a, b, c, d;
                a = obj["a"];
                b = obj["b"];
                c = obj["c"];
                d = obj["d"];

                MultipleChoice mc = new MultipleChoice(time, question, answer, a, b, c, d);
                currentQuestion = mc;
                BuildMCUI(mc);
            }
            else if(type == "truefalse")
            {
                debugText.Text = "truefalse";
                currentQuestion = q;
                BuildTrueFalseUI(q);
            }
            else
            {
                debugText.Text = "numeric";
                currentQuestion = q;
                BuildNumericUI(q);
            }
        }

        void BuildMCUI(MultipleChoice mc)
        {
            debugText.Text = mc.question;
            string[] answers = new string[4];
            if (mc.answera != "na") 
            answers[0] = mc.answera;
            if (mc.answerb != "na") 
            answers[1] = mc.answerb;
            if (mc.answerc != "na") 
            answers[2] = mc.answerc;
            if (mc.answerd != "na") 
            answers[3] = mc.answerd;

            foreach(string s in answers)
            {
                RunOnUiThread(() =>
                { 
                    ExtendedButton btn = new ExtendedButton(this);
                    btn.mAnswer = s;
                    btn.SetText(s, TextView.BufferType.Normal);
                    btn.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                                ViewGroup.LayoutParams.WrapContent);

                    btn.Click += delegate { CheckAnswer(btn.mAnswer); };
                    viewsToRemove.Add(btn);

                    layout.AddView(btn);
                });
            }

            SetTimer(mc.time);
        }

        void BuildTrueFalseUI(Question q)
        {
            debugText.Text = q.question;
            RunOnUiThread(() =>
            {
                //True Button
                ExtendedButton trueButton = new ExtendedButton(this);
                trueButton.mAnswer = "True";
                trueButton.SetText("True", TextView.BufferType.Normal);
                trueButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                                ViewGroup.LayoutParams.WrapContent);
                trueButton.Click += delegate { CheckAnswer(trueButton.mAnswer); };

                //False Button
                ExtendedButton falseButton = new ExtendedButton(this);
                falseButton.mAnswer = "False";
                falseButton.SetText("False", TextView.BufferType.Normal);
                falseButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                                ViewGroup.LayoutParams.WrapContent);
                falseButton.Click += delegate { CheckAnswer(falseButton.mAnswer); };

                viewsToRemove.Add(trueButton);
                viewsToRemove.Add(falseButton);
                layout.AddView(trueButton);
                layout.AddView(falseButton);
            });

            SetTimer(q.time);
        }

        void BuildNumericUI(Question q)
        {
            debugText.Text = q.question;

            RunOnUiThread(() =>
            {
                EditText numberText = new EditText(this);
                numberText.InputType = Android.Text.InputTypes.NumberFlagDecimal;

                Button mButton = new Button(this);
                mButton.SetText("Submit", TextView.BufferType.Normal);
                mButton.Click += delegate { CheckAnswer(numberText.Text.ToString()); };

                viewsToRemove.Add(numberText);
                viewsToRemove.Add(mButton);
                layout.AddView(numberText);
                layout.AddView(mButton);
            });

            SetTimer(q.time);
        }
        #endregion

        /// <summary>
        /// Timer for getting new question.
        /// </summary>
        #region ContinueTimer
        public void ContinueTimer()
        {
            continueTimer = new Timer();
            continueTimer.Interval = 1000;
            continueTimer.Elapsed += ContinueOnTick;
            continueSeconds = 3;
            RunOnUiThread(() =>
            {
                countdownText.Text = continueSeconds.ToString() + " Seconds Until Next Question";
            });

            continueTimer.Enabled = true;
        }

        public void ContinueOnTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            continueSeconds--;
            RunOnUiThread(() =>
            {
                countdownText.Text = continueSeconds.ToString() + " Seconds Until Next Question";
            });

            //Update visual representation here
            //Remember to do it on UI thread

            if (continueSeconds == 0)
            {
                RunOnUiThread(() => {
                    DoContinue();
                }); 
            }
        }

        void DoContinue()
        {
            continueTimer.Stop();
            GetPHP();
        }
        #endregion

        /// <summary>
        /// Timer for the question.
        /// </summary>
        /// <param name="seconds"></param>
        #region Question Timer
        public void SetTimer(int seconds)
        {
            //debugText.Text = "Setting.";
            countdownTimer = new Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Elapsed += OnTimedEvent;
            countdownSeconds = seconds;
            RunOnUiThread(() =>
            {
                countdownText.Text = seconds.ToString() + " Seconds";
            });
            

            countdownTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            countdownSeconds--;
            RunOnUiThread(() =>
            {
                countdownText.Text = countdownSeconds.ToString() + " Seconds";
            });

            //Update visual representation here
            //Remember to do it on UI thread

            if (countdownSeconds == 0)
            {
                countdownTimer.Stop();
            }
        }
        #endregion
    }
}

