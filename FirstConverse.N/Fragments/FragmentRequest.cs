using Android.OS;
using Android.Views;
using FirstConverse.Shared;
using System.Collections.Generic;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace FirstConverse.N.Droid
{
    /// <summary>
    /// to be used for  permission requests and meeting invtes.
    /// </summary>
    public class FragmentRequest : SupportFragment

    {
        List<MessageHeadersViewModel> _values = new List<MessageHeadersViewModel>();
        public FragmentRequest(ResponseConversationHeaders DataItems)
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
            View view = inflater.Inflate(Resource.Layout.viewSurveyDetail, container, false);

            return view;
        }
    }
}