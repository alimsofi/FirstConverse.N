
using Android.App;
using Android.OS;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System;
using Android.Widget;
//using Fragments.Helpers;
using Android.Views;
using FirstConverse.Shared;
using System.Threading.Tasks;
using FirstConverse.Shared.Services;

namespace FirstConverse.N.Droid
{
    [Activity(Label = "...", Theme = "@style/Theme.FC_Design")]
    public class MessageDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "cheese_name";
               
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.viewMessageDetail);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            //string cheeseName = Intent.GetStringExtra(EXTRA_NAME);

            var data = await LoadConversationDetails();

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            collapsingToolBar.Title = data.Result.Subject != null && data.Result.Subject.Length > 20 ? data.Result.Subject.Substring(0, 20) + "..." : data.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblMessageDetailSubject).Text = data.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblMessageDetailBody).Text = data.Result.Body;
            FindViewById<TextView>(Resource.Id.lblMessageDetailSender).Text = data.Result.Sender.FirstName + " " + data.Result.Sender.LastName;
            FindViewById<TextView>(Resource.Id.lblMessageDetailDateTime).Text = data.Result.SentDate.ToString("mmm-dd-yyyy hh:MM");
            LoadBackDrop();
        }
        public async Task<MessageDetailsResponse> LoadConversationDetails()
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