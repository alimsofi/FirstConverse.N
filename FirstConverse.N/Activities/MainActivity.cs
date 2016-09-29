using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using System.Collections.Generic;
using Java.Lang;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using Android.Gms.Common;
using Gcm.Client;
using FirstConverse.Shared.Services;
using FirstConverse.Shared;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
namespace FirstConverse.N.Droid
{
    [Activity(Label = "FirstConverse.N", MainLauncher = false, Icon = "@drawable/icon", Theme = "@style/Theme.FC_Design")]
    public class MainActivity : AppCompatActivity
    {
        private string Token { get; set; }
        private string UserName { get; set; }
        private ISharedPreferences SharedPref;
        private DrawerLayout mDrawerLayout;
        public ResponseHeadersViewModel ConversationList { get; set; }
        public delegate void RefreshListView(List<MessageHeadersViewModel> NewList);
        public event RefreshListView RefreshFormsView;
        
        //bool registered = false;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);

            SupportActionBar ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            ab.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }
            SharedPref = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
            Token = SharedPref.GetString("auth_token", string.Empty);
            UserName = SharedPref.GetString("user_name", string.Empty);

            var ConversationsList = LoadDataFromCache();

            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            SetUpViewPager(viewPager, ConversationsList);
            tabs.SetupWithViewPager(viewPager);

            #region tab selection color not used
            //tabs.GetTabAt(0).SetIcon(Resource.Drawable.ic_dashboard);
            //tabs.GetTabAt(0).Icon.SetColorFilter(Android.Graphics.Color.Pink, Android.Graphics.PorterDuff.Mode.Darken);
            //tabs.GetTabAt(1).SetIcon(Resource.Drawable.ic_event);
            //tabs.GetTabAt(1).Icon.SetColorFilter(Android.Graphics.Color.Pink, Android.Graphics.PorterDuff.Mode.Lighten);
            //tabs.GetTabAt(2).SetIcon(Resource.Drawable.ic_forum);
            #endregion

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += async (o, e) =>
            {
                View anchor = o as View;

                Snackbar.Make(anchor, "Refreshing, Please Wait...", Snackbar.LengthLong)
                        .SetAction("OKAY", v => { }).Show();
                await CheckForNewPush();
            };
            View navHeader = navigationView.GetHeaderView(0);
            if (navHeader != null)  
            {
               navHeader.FindViewById<TextView>(Resource.Id.lblNavUserName).Text = UserName;
               //  Want to show random header image on drawer?  Uncomment next line             
               //navHeader.FindViewById<ImageView>(Resource.Id.imgViewHeader).SetImageResource(Cheeses.RandomCheeseDrawable);
            }
        }

        //  Called from CreateView Event 
        private ResponseHeadersViewModel LoadDataFromCache()
        {
            if (ConversationList != null && ConversationList.Count > 0)
                return ConversationList;

            if (string.IsNullOrEmpty(Token))
            {
                LogoutFromService();
                return new ResponseHeadersViewModel();
            }
            
            string jsonData = SharedPref.GetString("list_data_" + UserName, string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                ConversationList = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                if (ConversationList != null)
                    return ConversationList;
            }
            return new ResponseHeadersViewModel();
        }

        #region Not used for now
        public ResponseHeadersViewModel LoadData(string token, bool refresh)
        {
            // get data from shared preferences
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);

            try
            {
                if (!refresh)
                {
                    string jsonData = prefs.GetString("list_data_" + prefs.GetString("user_name", string.Empty), string.Empty);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        ConversationList = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                        if (ConversationList != null)
                            return ConversationList;
                    }
                }
            }
            catch { }

            // TODO:  may never reach here..  we can remove this code 
            // get data from server when force refresh or on push notification
            ProgressDialog waitDialog = new ProgressDialog(this);
            waitDialog.SetMessage("Please Wait...");
            waitDialog.SetCancelable(false);
            waitDialog.Show();

            ConversationList = RestClient.GetConversations(token);

            waitDialog.Hide();
            if (ConversationList == null)
                return new ResponseHeadersViewModel();

            var edit = prefs.Edit();
            edit.PutString("list_data_" + prefs.GetString("user_name", string.Empty), JsonConvert.SerializeObject(ConversationList));
            edit.Commit();
            return ConversationList;
        }
        #endregion

        protected async override void OnResume()
        {
            base.OnResume();
            // TODO:  Use some flag to avoid too many refreshes

            if (SharedPref.GetInt("status_update_id", 0) != 0)
            {
                ((FragmentForms)formsFragment).UpdateStatusFlag(SharedPref.GetInt("status_update_id", 0));
                SharedPref.Edit().Remove("status_update_id").Commit();
            }

            await CheckForNewPush();
            // check for any status update after submit form or survey
            
        }

        // Called OnResume event to refresh new data
        private async Task CheckForNewPush()
        {

            // chek if push notification ids are available.. then fetch items by ids
            // for now get all conversations
            try
            {
                var NewConversationList = await RestClient.RefreshConversationsAsync(Token);
                // chek if any new ?
                if (NewConversationList.Count > ConversationList.Count)
                {
                    var EditPref = SharedPref.Edit();
                    EditPref.PutString("list_data_" + UserName, JsonConvert.SerializeObject(NewConversationList));
                    EditPref.Commit();
                    var NewFormItems = new List<MessageHeadersViewModel>();
                    for (int x = 0; x < NewConversationList.Forms.Count - ConversationList.Forms.Count; x++)
                        NewFormItems.Add(NewConversationList.Forms.Items[x]);
                    ConversationList = NewConversationList;
                    // Raise refresh view event on fragments
                    this.RefreshFormsView.Invoke(NewFormItems);
                }
            }
            catch(System.Exception ex)
            {
                if(ex.Message.StartsWith("ERROR"))
                {
                    View anchor = FindViewById<FloatingActionButton>(Resource.Id.fab) as View;
                    Snackbar.Make(anchor, "Something went wrong!", Snackbar.LengthLong)
                            .SetAction("RETRY", async v =>
                            {
                                await CheckForNewPush();
                            }).Show();
                }
            }
        }

        private void UpdateDataInSharedPref(int messageId)
        {
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
            string jsonData = prefs.GetString("list_data_" + prefs.GetString("user_name", string.Empty), string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                ResponseHeadersViewModel ConversationList = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                var s = (from a in ConversationList.Forms.Items where a.Id == messageId select a).FirstOrDefault<MessageHeadersViewModel>();
                s.ConversationType = ConversationType.Response;
                var edit = prefs.Edit();
                edit.PutString("list_data_" + prefs.GetString("user_name", string.Empty), JsonConvert.SerializeObject(ConversationList));
                edit.Commit();
            }
        }

        private async void LogoutFromService()
        {
            ProgressDialog waitDialog = new ProgressDialog(this);
            waitDialog.SetMessage("Logging Out...");
            waitDialog.SetCancelable(false);
            waitDialog.Show();
            string response = await RestClient.Logout(this.Intent.GetStringExtra("auth_token"));
            if (response == "DONE")
            {
                var actv = new Intent(this, typeof(LoginActivity));
                Finish();
                StartActivity(actv);
                OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_exit);
                var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
                var edit = prefs.Edit();
                edit.Remove("auth_token");
                edit.Remove("token_expires");
                edit.Commit();
            }
            waitDialog.Hide();
        }

        #region NOT used now
        private void ShowNotificationMsg()
        {
            var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
            string textLastMsg = prefs.GetString("last_msg", string.Empty);

            if (!string.IsNullOrEmpty(textLastMsg))
            {
                NotificationData notification = JsonConvert.DeserializeObject<NotificationData>(textLastMsg);
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle(notification.Title);
                alert.SetMessage(notification.Message);
                Dictionary<string, int> icons = new Dictionary<string, int>();
                icons.Add("DEFAULT", Resource.Drawable.noti_icon);
                icons.Add("INFO", Resource.Drawable.noti_icon);
                icons.Add("MESSAGE", Resource.Drawable.noti_icon);
                icons.Add("ALERT", Resource.Drawable.noti_icon);
                //Create notification
                if (!icons.ContainsKey(notification.Icon))
                    notification.Icon = "DEFAULT";
                alert.SetIcon(icons[notification.Icon]);
                // TODo: positive button can open the msg/form  and close to dismiss notification
                alert.SetPositiveButton("DISMISS", (senderAlert, args) => {
                    //Toast.MakeText(this, "Deleted!", ToastLength.Short).Show();
                    var edit = prefs.Edit();
                    edit.Remove("last_msg");
                    edit.Commit();
                    notificationAlert = null;
                });

                //alert.SetNegativeButton("Cancel", (senderAlert, args) => {
                //    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                //});

                notificationAlert = alert.Create();
            }
        }
        #endregion

        #region  Tab controller code
        Dialog notificationAlert;
        SupportFragment formsFragment;
        private void SetUpViewPager(ViewPager viewPager , ResponseHeadersViewModel conversationList)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            formsFragment = new FragmentForms(conversationList.Forms);
            adapter.AddFragment(formsFragment, "Forms");
            adapter.AddFragment(new FragmentText(conversationList.TextMessages), "Msgs");
            adapter.AddFragment(new FragmentSurvey(conversationList.Surveys), "Surveys");
            adapter.AddFragment(new FragmentRequest(conversationList.PermissionRequests), "Permission");
            viewPager.Adapter = adapter;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);                    
            }
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
                switch(e.MenuItem.ItemId)
                {
                    case Resource.Id.menuLogout:
                        LogoutFromService();
                        break;
                }
            };
        }

        
        public class TabAdapter : FragmentPagerAdapter
        {
            public List<SupportFragment> Fragments { get; set; }
            public List<string> FragmentNames { get; set; }

            public TabAdapter (SupportFragmentManager sfm) : base (sfm)
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }

            public void AddFragment(SupportFragment fragment, string name)
            {
                Fragments.Add(fragment);
                FragmentNames.Add(name);
            }

            public override int Count
            {
                get
                {
                    return Fragments.Count;
                }
            }

            public override SupportFragment GetItem(int position)
            {
                return Fragments[position];
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                int[] imageResId = {
                    Resource.Drawable.ic_assignment_white_24dp,
                    Resource.Drawable.ic_mail_outline_white_24dp,
                    Resource.Drawable.ic_playlist_add_check_white_36dp,
                    Resource.Drawable.ic_today_white_24dp
                };
                string[] tabTitles = { "FORMS", "MESSAGES", "SURVEYS", "PERMISSIONS" };
                // Generate title based on item position
                Drawable image = Android.App.Application.Context.GetDrawable(imageResId[position]);
                image.SetBounds(0, 0, image.IntrinsicWidth, image.IntrinsicHeight);
                // Replace blank spaces with image icon
                SpannableString sb = new SpannableString("  " + tabTitles[position]);
                ImageSpan imageSpan = new ImageSpan(image, SpanAlign.Bottom);
                sb.SetSpan(imageSpan, 0, 1, SpanTypes.ExclusiveExclusive);
                return sb;
            }
            
        }
        #endregion
        
    }
}

