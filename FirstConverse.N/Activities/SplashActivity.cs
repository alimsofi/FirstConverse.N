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
using System.Threading.Tasks;
using Gcm.Client;
using FirstConverse.Shared.Services;
using Newtonsoft.Json;
using FirstConverse.Shared;

namespace FirstConverse.N.Droid
{ 
    [Activity (Theme = "@style/MyTheme.Splash", NoHistory = true, MainLauncher = true, Icon = "@drawable/logo_mini")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);          
                
        }
        string token = "";
        protected override void OnResume()
        {
            base.OnResume();
            var startUpActivity = typeof(LoginActivity); 
            Task startupWork = new Task(() => {
                //Task.Delay(1000);  // Simulate a bit of startup work.

                var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);

                token = prefs.GetString("auth_token", string.Empty);
                long validity = prefs.GetLong("token_expires", 0);
                if ((!string.IsNullOrEmpty(token)) && (DateTime.Now.Ticks < validity))
                {
                    LoadDataFromService(token, prefs.GetString("user_name", string.Empty));
                    startUpActivity = typeof(MainActivity);
                }
            });

            startupWork.ContinueWith(t => {
                StartActivity(new Intent(Application.Context, startUpActivity).PutExtra("auth_token", token));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
        
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

                    var ConversationList = RestClient.GetConversations(token);

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
    }
}