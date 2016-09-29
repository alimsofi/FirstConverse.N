using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using FirstConverse.Shared.Services;
using FirstConverse.Shared;
using System.Collections.Generic;
using Newtonsoft.Json;

//VERY VERY VERY IMPORTANT NOTE!!!!
// Your package name MUST NOT start with an uppercase letter.
// Android does not allow permissions to start with an upper case letter
// If it does you will get a very cryptic error in logcat and it will not be obvious why you are crying!
// So please, for the love of all that is kind on this earth, use a LOWERCASE first letter in your Package Name!!!!
namespace FirstConverse.N.Droid
{
    //You must subclass this!
    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new string[] { "@PACKAGE_NAME@" })]
    public class GcmBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        //The SENDER_ID is your Google API Console App Project ID.
        public static string[] SENDER_IDS = new string[] { "243975416020" };

        public const string TAG = "PushSharp-GCM";
    }

    [Service] //Must use the service tag
    public class PushHandlerService : GcmServiceBase
    {
        public PushHandlerService() : base(GcmBroadcastReceiver.SENDER_IDS) { }

        const string TAG = "GCM-Test";

        protected override void OnRegistered(Context context, string registrationId)
        {
            RegisterDevice(context, registrationId);
        }
        private async void RegisterDevice(Context context, string registrationId)
        {
            var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
            string token = prefs.GetString("auth_token", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                bool deviceRegistered = await RestClient.RegisterForPush(token, registrationId);
                if (deviceRegistered)
                {
                    var edit = prefs.Edit();
                    edit.PutBoolean("push_reg", true);
                    edit.Commit();
                }//CreateNotification("GCM Registered", "Device Registered for Notifications");
            }
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            //Log.Verbose(TAG, "GCM Unregistered: " + registrationId);
            //Remove from the web service
            //	var wc = new WebClient();
            //	var result = wc.UploadString("http://your.server.com/api/unregister/", "POST",
            //		"{ 'registrationId' : '" + lastRegistrationId + "' }");
            var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
            var edit = prefs.Edit();
            edit.PutBoolean("push_reg", false);
            edit.Commit();
            CreateNotification("GCM Unregistered...", "The device has been unregistered, Tap to View!", "DEFAULT");
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            string s = "";
            NotificationData notification = new NotificationData();
            //foreach (string key in intent.Extras.KeySet())
            //    s += key + "-";
            if (intent != null && intent.Extras != null)
            {
                notification.Title = intent.Extras.Get("gcm.notification.title").ToString();
                notification.Message = intent.Extras.Get("gcm.notification.body").ToString();
                notification.Icon = intent.Extras.Get("gcm.notification.icon").ToString().ToUpper();
                //Store the message 
                var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
                var edit = prefs.Edit();
                edit.PutString("last_msg", JsonConvert.SerializeObject(notification));
                edit.Commit();

                CreateNotification(notification.Title, notification.Message, notification.Icon);
            }
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            //Log.Warn(TAG, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            //Log.Error(TAG, "GCM Error: " + errorId);
        }

        void CreateNotification(string Title, string MsgContent, string Icon)
        {
            Dictionary<string, int> icons = new Dictionary<string, int>();
            icons.Add("DEFAULT", Resource.Drawable.noti_icon);
            icons.Add("INFO", Resource.Drawable.noti_icon);
            icons.Add("MESSAGE", Resource.Drawable.noti_icon);
            icons.Add("ALERT", Resource.Drawable.noti_icon);
            //Create notification
            if (!icons.ContainsKey(Icon))
                Icon = "DEFAULT";
            var NotificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            //Create an intent to show ui
            var uiIntent = new Intent(this, typeof(MainActivity));

            //Create the notification
            var lNotification = new Notification(icons[Icon], Title);

            //Auto cancel will remove the notification once the user touches it
            lNotification.Flags = NotificationFlags.AutoCancel;

            //Set the notification info
            //we use the pending intent, passing our ui intent over which will get called
            //when the notification is tapped.
            lNotification.SetLatestEventInfo(this, Title, MsgContent, PendingIntent.GetActivity(this, 0, uiIntent, 0));

            //Show the notification
            NotificationManager.Notify(1, lNotification);
        }
    }
}

