
using Android.App;
using Android.OS;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System;
using Android.Widget;
using System.Linq;
using Android.Views;
using FirstConverse.Shared;
using System.Threading.Tasks;
using FirstConverse.Shared.Services;
using FirstConverse.Shared.BindingModel;
using System.Collections.Generic;
using Android.Content;

namespace FirstConverse.N.Droid
{
    [Activity(Label = "...", Theme = "@style/Theme.FC_Design")]
    public class SurveyDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "cheese_name";
        private bool ReadOnly;
        private MessageDetailsResponse surveyListData { get; set; }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.viewSurveyDetail);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            ReadOnly = this.Intent.GetStringExtra("type") == "Response";

            FindViewById<Button>(Resource.Id.btnSurveyDetailsSubmit).Click += SurveyDetailActivity_Click;
            LoadSurveyData();


            LoadBackDrop();
        }

        private async void SurveyDetailActivity_Click(object sender, EventArgs e)
        {
            ProgressDialog waitDialog = new ProgressDialog(this);
            waitDialog.SetMessage("Sending Reply...");
            waitDialog.SetCancelable(false);
            waitDialog.Show();
            SurveyResponseBindingModel surveyResponse = new SurveyResponseBindingModel();
            IList<QuestionAnswerBindingModel> selectedAnswers = new List<QuestionAnswerBindingModel>();
            surveyResponse.Id = surveyListData.Result.MessageId;
            
            foreach (QuestionAnswer itemResp in surveyListData.Result.MessageTemplate.QuestionAnswers)
            {
                if(itemResp.SelectedAnswers== null || itemResp.SelectedAnswers.Count==0|| itemResp.SelectedAnswers[0].Text=="")
                {
                    waitDialog.Hide();
                    View anchor = sender as View;
                    Snackbar.Make(anchor, "You must reply to all questions", Snackbar.LengthLong).SetAction("OKAY", v =>
                    { }).Show();
                    return;
                }
                selectedAnswers.Add(new QuestionAnswerBindingModel()
                {
                    AnswerId = itemResp.SelectedAnswers[0].Id,
                    QuestionId = itemResp.Question.Id
                });
            }
            surveyResponse.QuestionAnswer = selectedAnswers;
            string result = await SubmitSurveyResponse(surveyResponse);
            waitDialog.Hide();
            if (result == "SUCCESS")
            {
                //View anchor = sender as View;
                //Snackbar  sn = Snackbar.Make(anchor, "Your Reply is Submitted!", Snackbar.LengthLong);
                //sn.Show();
                //await Task.Delay(2000);
                //UpdateDataInSharedPref(response.Id);
                Toast.MakeText(this, "Your Reply is Submitted!", ToastLength.Long).Show();
                this.Finish();
            }
            else
            {
                // todo:  log result error
                Toast.MakeText(this, "Sorry, Please try again later.", ToastLength.Long).Show();
            }

        }
        private async Task<string> SubmitSurveyResponse(SurveyResponseBindingModel response)
        {
            return await RestClient.SubmitSurveyResponse(this.Intent.GetStringExtra("auth_token"), this.Intent.GetIntExtra("MsgId", 0), response);
        }
        private async void LoadSurveyData()
        {
            surveyListData = await LoadSurveyDetails();

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.survey_collapsing_toolbar);
            collapsingToolBar.Title = surveyListData.Result.Subject != null && surveyListData.Result.Subject.Length > 20 ? surveyListData.Result.Subject.Substring(0, 20) + "..." : surveyListData.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblSurveyDetailSubject).Text = surveyListData.Result.Subject == null ? "" : surveyListData.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblSurveyDetailBody).Text = surveyListData.Result.Body == null ? "" : surveyListData.Result.Body;
            FindViewById<TextView>(Resource.Id.lblSurveyDetailSender).Text = surveyListData.Result.Sender.FirstName + " " + surveyListData.Result.Sender.LastName;
            FindViewById<TextView>(Resource.Id.lblSurveyDetailDateTime).Text = surveyListData.Result.SentDate.ToString("mmm-dd-yyyy hh:MM");

            LinearLayout root = FindViewById<LinearLayout>(Resource.Id.surveyLayout);
            TextView lable;

            foreach (QuestionAnswer control in surveyListData.Result.MessageTemplate.QuestionAnswers)
            {
                lable = new TextView(this);
                lable.Text = control.Question.Text;

                lable.SetTextColor(Resources.GetColor(Resource.Color.black));
                root.AddView(lable);

                //  create view for answers

                ListBoxInfo c = new ListBoxInfo();
                c.Items = control.Answers;
                c.SelectedItems = control.SelectedAnswers;
                lable = new TextView(this);
                string[] arr = c.Items.Select(e => e.Text).ToArray<string>();
                bool[] sel = new bool[arr.Length];
                if (c.SelectedItems == null)
                    c.SelectedItems = new List<Item>();
                Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                control.SelectedAnswers = new List<Item>();
                int checkedItem = -1;
                //if (control.SelectedAnswers != null && control.SelectedAnswers.Count > 0)
                //    checkedItem = (int)control.SelectedAnswers[0].Id;
                dialog.SetTitle(c.Caption).SetSingleChoiceItems(arr, checkedItem, new EventHandler<DialogClickEventArgs>(
                    delegate (object sender, DialogClickEventArgs args)
                    {
                        control.SelectedAnswers = new List<Item>();
                        control.SelectedAnswers.Add(new Item() { Text = c.Items[args.Which].Text, Id = c.Items[args.Which].Id });
                        lable.Text = control.SelectedAnswers[0].Text;
                        checkedItem = (int)c.Items[args.Which].Id;
                        CloseDialog();
                    }));

                //dialog.SetPositiveButton("OKAY", new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs args)
                //{
                //    lable.Text = control.SelectedAnswers[0].Text;
                //}));

                dialog.Create();

                lable.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                lable.Gravity = GravityFlags.End;

                if (!ReadOnly)
                {
                    
                    lable.Click += delegate
                    {
                        _dialog = dialog.Show();
                        
                    };
                    lable.Text = "Tap & Choose Answer" + c.Caption;
                }
                else
                {
                    if (control.SelectedAnswers != null)
                        lable.Text = control.SelectedAnswers[0].Text;
                }
                lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));

                root.AddView(lable);

            }
        }
        Android.App.AlertDialog _dialog;

        private void CloseDialog()
        {
            if (_dialog != null)
                _dialog.Dismiss();
        }
        public async Task<MessageDetailsResponse> LoadSurveyDetails()
        {
            return await RestClient.GetMessageDetails(this.Intent.GetStringExtra("auth_token"), this.Intent.GetIntExtra("MsgId", 0));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.contextMenu, menu);
            return true;
        }

        private void LoadBackDrop()
        {
            ImageView imageView = FindViewById<ImageView>(Resource.Id.backdrop);
            imageView.SetImageResource(Cheeses.RandomCheeseDrawable);
        }
    }
}