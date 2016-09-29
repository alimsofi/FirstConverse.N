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
namespace FirstConverse.N.Droid
{
    /// <summary>
    /// to be used for text messages listing
    /// </summary>
    public class FragmentText : SupportFragment
    {
        List<MessageHeadersViewModel> _values = new List<MessageHeadersViewModel>();
        public FragmentText(ResponseConversationHeaders DataItems)
        {
            if (DataItems != null && DataItems.Items != null)
                _values = DataItems.Items;
            else
                _values = new List<MessageHeadersViewModel>();
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            RecyclerView recyclerView = inflater.Inflate(Resource.Layout.fragmentList, container, false) as RecyclerView;

            SetUpRecyclerView(recyclerView);

            return recyclerView;
        }
        private void SetUpRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            recyclerView.SetAdapter(new SimpleStringRecyclerViewAdapter(recyclerView.Context, _values, Activity.Resources));

            recyclerView.SetItemClickListener((rv, position, view) =>
            {
                //An item has been clicked
                Context context = view.Context;
                MessageHeadersViewModel t = _values[position];
                Intent intent = new Intent(context, typeof(MessageDetailActivity));
                intent.PutExtra("MsgId", t.Id);
                intent.PutExtra("auth_token", this.Activity.Intent.GetStringExtra("auth_token"));
                intent.PutExtra("type", t.TemplateType.ToString());
                context.StartActivity(intent);
            });
        }

        public class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
        {
            private readonly TypedValue mTypedValue = new TypedValue();
            private int mBackground;
            private List<MessageHeadersViewModel> mValues;
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

                simpleHolder.mFormAction.Text = "OPEN";
                simpleHolder.mHead.Text = item.Subject.Length > 20 ? item.Subject.Substring(0, 20) + "..." : item.Subject;
                //if (item.Status == MessageState.Open)
                    simpleHolder.mHead.SetTypeface(null, TypefaceStyle.Bold);
                simpleHolder.mPriority.Text = item.ImportanceLevel.ToString();
                simpleHolder.mSender.Text = item.Sender.FirstName + " " + item.Sender.LastName;
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
                View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.fragmentListItemText, parent, false);
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