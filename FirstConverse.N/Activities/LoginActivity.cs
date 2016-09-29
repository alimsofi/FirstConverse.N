using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;

using FirstConverse.Shared.Services;
using FirstConverse.Shared.Models;
using Android.Views.Animations;
using Android.Support.Design.Widget;
using FirstConverse.Shared;
using Newtonsoft.Json;
using Gcm.Client;

namespace FirstConverse.N.Droid
{
	[Activity (Label = "FC-DEV", Theme = "@style/Theme.FC_Design", Icon = "@drawable/icon")]
	public class LoginActivity : AppCompatActivity
	{

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            //LayoutInflater.Factory = new RobotoTextFactory();

            SetContentView(Resource.Layout.viewLogin);

            FindViewById<Button>(Resource.Id.btnSignup).Click += NavLabels_Click;
            //FindViewById<TextView>(Resource.Id.lblForgotPassword).Click += NavLabels_Click;

            FindViewById<EditText>(Resource.Id.txtLoginUserName).Text = "rafeeq.recipient@gmail.com";
            FindViewById<EditText>(Resource.Id.txtLoginPassword).Text = "P@mp0sh2016";


            FindViewById<Button>(Resource.Id.btnLogin).Click += LoginActivity_Click;
        }

        private async void LoginActivity_Click(object sender, EventArgs e)
        {

            if (FindViewById<EditText>(Resource.Id.txtLoginUserName).Text == string.Empty || FindViewById<EditText>(Resource.Id.txtLoginPassword).Text == string.Empty)
            {
                Snackbar.Make((View)sender, "Enter Your Username & Password", Snackbar.LengthLong).Show();
                return;
            }
            ProgressDialog waitDialog = new ProgressDialog(this);
            waitDialog.SetMessage("Authenticating...");
            waitDialog.SetCancelable(false);
            waitDialog.Show();

            AuthResponse response = await RestClient.Authenticate(FindViewById<EditText>(Resource.Id.txtLoginUserName).Text, FindViewById<EditText>(Resource.Id.txtLoginPassword).Text);
            waitDialog.Hide();
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
            if (response.ErrorResponse == null)
            {
                prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
                var edit = prefs.Edit();

                // Store duration (seconds) for validity of token instead
                edit.PutString("auth_token", response.UserInfo.AccessToken);
                edit.PutString("user_name", response.UserInfo.UserName);
                DateTime tokenExpires = DateTime.Now.AddSeconds(response.UserInfo.ValidityInSeconds);
                edit.PutLong("token_expires", tokenExpires.Ticks);
                edit.Commit();
                var actv = new Intent(this, typeof(MainActivity));
                actv.PutExtra("auth_token", response.UserInfo.AccessToken);
                RegisterForPushNotification();
                LoadDataFromService(response.UserInfo.AccessToken, response.UserInfo.UserName);
                Finish();
                StartActivity(actv);
                //OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_exit);
            }
            else if (response.ErrorResponse != null && response.ErrorResponse.ErrorCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Snackbar.Make((View)sender, "Invalid UserId Or Password", Snackbar.LengthLong).SetAction("OKAY", v =>
                {
                    FindViewById<TextView>(Resource.Id.txtLoginPassword).Text = "";
                }).Show();
            }
            else
            {
                // todo: log response.errorresponse
                Snackbar.Make((View)sender, "SignIn Error. Please try later", Snackbar.LengthLong).SetAction("RETRY", v =>
                {
                    LoginActivity_Click(sender, e);
                }).Show();
            }
        }

        
        
        private void NavLabels_Click(object sender, EventArgs e)
        {
            TextView label = (TextView)sender;
            Animation ani = AnimationUtils.LoadAnimation(ApplicationContext, Resource.Animation.fade_in);
            label.StartAnimation(ani);
            StartActivity(new Intent(this, typeof(RegisterActivity)));
            OverridePendingTransition(Resource.Animation.in_from_bottom, Resource.Animation.out_from_bottom);
        }
        public ResponseHeadersViewModel ConversationList { get; set; }
        public void LoadDataFromService(string token, string userName)
        {
            // get data from shared preferences
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);

            try
            {
                string jsonData = prefs.GetString("list_data_" + userName, string.Empty);
                if (string.IsNullOrEmpty(jsonData))
                {
                    // get data from server when force refresh or on push notification
                    ProgressDialog waitDialog = new ProgressDialog(this);
                    waitDialog.SetMessage("Please Wait...");
                    waitDialog.SetCancelable(false);
                    waitDialog.Show();

                    ConversationList = RestClient.GetConversations(token);

                    waitDialog.Hide();
                    if (ConversationList == null)
                        ConversationList = new ResponseHeadersViewModel();

                    var edit = prefs.Edit();
                    edit.PutString("list_data_" + userName, JsonConvert.SerializeObject(ConversationList));
                    edit.Commit();
                }
            }
            catch { }

        }
        private void RegisterForPushNotification()
        {
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
            bool registered = prefs.GetBoolean("push_reg", false);

            if (!registered)
            {
                //Register with GCM
                GcmClient.Register(this, GcmBroadcastReceiver.SENDER_IDS);
                var edit = prefs.Edit();
                edit.PutBoolean("push_reg", true);
            }
        }
    }
}


