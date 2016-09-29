using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using SupportFragment = Android.Support.V4.App.Fragment;
using System.Collections.Generic;
using Android.Graphics;
using Android.Util;
using Android.Content;
using Android.Content.Res;
using Android.Widget;
using FirstConverse.Shared;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Android.Support.Design.Widget;

namespace FirstConverse.N.Droid
{
    /// <summary>
    ///  to be used for forms listing
    /// </summary>
    public class FragmentForms : SupportFragment
    {
        List<MessageHeadersViewModel> _values = new List<MessageHeadersViewModel>();
        
        public FragmentForms(ResponseConversationHeaders DataItems)
        {
            
            if (DataItems != null && DataItems.Items != null)
                _values = DataItems.Items;
            else
                _values = new List<MessageHeadersViewModel>();
        }


        private void FragmentForms_RefreshFormsView(List<MessageHeadersViewModel> NewList)
        {
            // do refresh here.
            for (int x = NewList.Count - 1; x >= 0; x--)
                ((SimpleStringRecyclerViewAdapter)recyclerView.GetAdapter()).mValues.Insert(0, NewList[0]);
            
            recyclerView.GetAdapter().NotifyItemInserted(0);
            recyclerView.SmoothScrollToPosition(0);
        }
        public void UpdateStatusFlag(int messageId)
        {
            
            // Update Cache
            var prefs = this.Context.GetSharedPreferences(this.Context.PackageName, FileCreationMode.Private);
            string jsonData = prefs.GetString("list_data_" + prefs.GetString("user_name", string.Empty), string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                var ConversationList = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                if (ConversationList != null)
                {
                    var current = from row in ConversationList.Forms.Items where row.Id == messageId select row;
                    if (current != null)
                        current.Single<MessageHeadersViewModel>().ConversationType = ConversationType.Response;
                    var EditPref = prefs.Edit();
                    EditPref.PutString("list_data_" + prefs.GetString("user_name", string.Empty), JsonConvert.SerializeObject(ConversationList));
                    EditPref.Commit();
                }
            }
            // Update View
            int index = 0;
            for (; index < ((SimpleStringRecyclerViewAdapter)recyclerView.GetAdapter()).mValues.Count; index++)
            {
                if (((SimpleStringRecyclerViewAdapter)recyclerView.GetAdapter()).mValues[index].Id == messageId)
                {
                    ((SimpleStringRecyclerViewAdapter)recyclerView.GetAdapter()).mValues[index].ConversationType = ConversationType.Response;
                    break;
                }
            }
            recyclerView.GetAdapter().NotifyItemChanged(index);
        }
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ((MainActivity)this.Activity).RefreshFormsView -= FragmentForms_RefreshFormsView;
            ((MainActivity)this.Activity).RefreshFormsView += FragmentForms_RefreshFormsView;

            // Create your fragment here
            //FloatingActionButton fab = this.Activity.FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += (o, e) =>
            //{

                // make call to service 

                //
                //FragmentForms_RefreshFormsView(new List<MessageHeadersViewModel>() { _values[_values.Count - 1] });

                //((SimpleStringRecyclerViewAdapter)recyclerView.GetAdapter()).mValues.Insert(0, new MessageHeadersViewModel()
                //{
                //    Body = "hello new",
                //    ConversationType = ConversationType.Request,
                //    EndDate = DateTime.Now,
                //    From = "test frm",
                //    Id = 999,
                //    ImportanceLevel = ImportanceLevel.High,
                //    IsViewed = false,
                //    Sender = new UserViewModel() { Email = "mail@mail.com", FirstName = "alim", LastName = "sofi" },
                //    StartDate = DateTime.Now,
                //    Status = MessageState.Open,
                //    Subject = "tes subject",
                //    TemplateType = TemplateType.Form

                //});
                
                
            //};
        }
        RecyclerView recyclerView;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            recyclerView = inflater.Inflate(Resource.Layout.fragmentList, container, false) as RecyclerView;

            SetUpRecyclerView(recyclerView);

            return recyclerView;
        }
        // no longer used now
        private void RefreshListFromCache()
        {
            var prefs = this.Context.GetSharedPreferences(this.Context.PackageName, FileCreationMode.Private);
            string jsonData = prefs.GetString("list_data_" + prefs.GetString("user_name", string.Empty), string.Empty);
            if (!string.IsNullOrEmpty(jsonData))
            {
                var ConversationList = JsonConvert.DeserializeObject<ResponseHeadersViewModel>(jsonData);
                if (ConversationList != null)
                    _values = ConversationList.Forms.Items;
            }
        }
        private void SetUpRecyclerView(RecyclerView recyclerView)
        {
            //RefreshListFromCache();
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            recyclerView.SetAdapter(new SimpleStringRecyclerViewAdapter(recyclerView.Context, _values, Activity.Resources));

            recyclerView.SetItemClickListener((rv, position, view) =>
            {
                //An item has been clicked
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(FormDetailsActivity));
                MessageHeadersViewModel t = _values[position];
                //intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, _values.ToArray()[position].Id);
                intent.PutExtra("MsgId", t.Id);
                intent.PutExtra("auth_token", this.Activity.Intent.GetStringExtra("auth_token"));
                intent.PutExtra("type", t.ConversationType.ToString());
                context.StartActivity(intent);
            });
        }

        public class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
        {
            private readonly TypedValue mTypedValue = new TypedValue();
            private int mBackground;
            public List<MessageHeadersViewModel> mValues;
            Resources mResource;
            private Dictionary<int, int> mCalculatedSizes;

            public SimpleStringRecyclerViewAdapter(Context context, List<MessageHeadersViewModel> items, Resources res)
            {
                context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
                mBackground = mTypedValue.ResourceId;
                mValues = items;
                mResource = res;

                mCalculatedSizes = new Dictionary<int, int>();
            }

            public override int ItemCount
            {
                get
                {
                    return mValues.Count;
                }
            }
            public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var simpleHolder = holder as SimpleViewHolder;
                MessageHeadersViewModel item = mValues[position];

                simpleHolder.mFormAction.Text = item.ConversationType == ConversationType.Request ? "Reply" : "View";
                simpleHolder.mHead.Text = item.Subject != null && item.Subject.Length > 20 ? item.Subject.Substring(0, 20) + "..." : item.Subject;
                if (item.ConversationType == ConversationType.Request)
                    simpleHolder.mHead.SetTypeface(null, TypefaceStyle.Bold);
                simpleHolder.mPriority.Text = item.ImportanceLevel.ToString();
                simpleHolder.mSender.Text = item.Sender.FirstName + " "+ item.Sender.LastName;
                simpleHolder.mStartDate.Text = item.StartDate == null ? DateTime.Now.ToString("MMM-dd-yyyy HH:mm") : item.StartDate.ToString();
                simpleHolder.mSubHead.Text = item.Subject != null && item.Subject.Length > 50 ? item.Subject.Substring(0, 50) + "..." : item.Subject;
                int drawableID = Cheeses.RandomCheeseDrawable;
                
                BitmapFactory.Options options = new BitmapFactory.Options();

                if (mCalculatedSizes.ContainsKey(drawableID))
                {
                    options.InSampleSize = mCalculatedSizes[drawableID];
                }

                else
                {
                    options.InJustDecodeBounds = true;

                    BitmapFactory.DecodeResource(mResource, drawableID, options);

                    options.InSampleSize = Cheeses.CalculateInSampleSize(options, 100, 100);
                    options.InJustDecodeBounds = false;

                    mCalculatedSizes.Add(drawableID, options.InSampleSize);
                }


                var bitMap = await BitmapFactory.DecodeResourceAsync(mResource, drawableID, options);

                simpleHolder.mImageView.SetImageBitmap(bitMap);
                
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.fragmentListItemForms, parent, false);
                view.SetBackgroundResource(mBackground);

                return new SimpleViewHolder(view);
            }
        }

        public class SimpleViewHolder : RecyclerView.ViewHolder
        {
            public string mBoundString;
            public readonly View mView;
            public readonly ImageView mImageView;
            public readonly ImageView mRightArrow;
            public readonly TextView mHead;
            public readonly TextView mFormAction;
            public readonly TextView mSubHead;
            public readonly TextView mPriority;
            public readonly TextView mSender;
            public readonly TextView mStartDate;

            public SimpleViewHolder(View view) : base(view)
            {
                mView = view;
                mRightArrow = view.FindViewById<ImageView>(Resource.Id.right_arrow);
                mImageView = view.FindViewById<ImageView>(Resource.Id.avatar);
                mHead = view.FindViewById<TextView>(Resource.Id.lblHead);
                mFormAction = view.FindViewById<TextView>(Resource.Id.lblFormAction);
                mSubHead = view.FindViewById<TextView>(Resource.Id.lblSubHead);
                mPriority = view.FindViewById<TextView>(Resource.Id.lblPriority);
                mSender = view.FindViewById<TextView>(Resource.Id.lblSender);
                mStartDate = view.FindViewById<TextView>(Resource.Id.lblStartDate);
            }
            // NOT REQUIRED
            public override string ToString()
            {
                return base.ToString() + " '" + mHead.Text;
            }
        }
    }
}