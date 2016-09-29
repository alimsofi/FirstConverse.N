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
using Android.Views.Animations;
using FirstConverse.Shared;
using Android.Support.Design.Widget;

namespace FirstConverse.N.Droid
{
    [Activity(Label = "Register", Theme = "@style/Theme.FC_Design")]
    public class RegisterActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.viewRegister);
            // Create your application here
            FindViewById<Button>(Resource.Id.btnRegister).Click += RegisterButton_Click;
            //FindViewById<TextView>(Resource.Id.lblLogin).Click += NavLabels_Click;
            //FindViewById<TextView>(Resource.Id.lblForgotPassword).Click += NavLabels_Click;
        }

        private void NavLabels_Click(object sender, EventArgs e)
        {
            TextView label = (TextView)sender;
            Animation ani = AnimationUtils.LoadAnimation(ApplicationContext, Resource.Animation.fade_in);
            label.StartAnimation(ani);
            //StartActivity(new Intent(this, typeof(MainActivity)));
            Finish();
            OverridePendingTransition(Resource.Animation.out_from_bottom, Resource.Animation.out_to_top);
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            Register registerModel = new Register();
            registerModel.FirstName = FindViewById<TextView>(Resource.Id.txtFirstName).Text;
            registerModel.LastName = FindViewById<TextView>(Resource.Id.txtLastName).Text;
            registerModel.Email = FindViewById<TextView>(Resource.Id.txtEmailAddress).Text;
            registerModel.Password = FindViewById<TextView>(Resource.Id.txtPassword).Text;
            registerModel.ConfirmPassword = FindViewById<TextView>(Resource.Id.txtConfirmPassword).Text;
            if (registerModel.FirstName == "" || registerModel.LastName == "" || registerModel.Email == "" || registerModel.Password == "" || registerModel.ConfirmPassword == "")
            {
                Snackbar.Make((View)sender, "All fields are Required", Snackbar.LengthLong).Show();
                return;
            }
            else if (registerModel.Password != registerModel.ConfirmPassword)
            {
                Snackbar.Make((View)sender, "Password and Confirm password do not match", Snackbar.LengthLong).Show();
                return;
            }
            else if (!ValidateEmail(registerModel.Email))
            {
                Snackbar.Make((View)sender, "Invalid Email", Snackbar.LengthLong).Show();
                return;
            }
            ProgressDialog waitDialog = new ProgressDialog(this);
            waitDialog.SetMessage("Registration In Progress...");
            waitDialog.SetCancelable(false);
            waitDialog.Show();
            string respMessage = await registerModel.Submit();
            //Toast msg = Toast.MakeText(this, respMessage, ToastLength.Short);
            waitDialog.Hide();
            //msg.Show();
            if (respMessage.Contains("Success"))
            {
                //StartActivity(new Intent(this, typeof(MainActivity)));
                Toast.MakeText(this, "Registration Successful, Wait for Activation Email", ToastLength.Long).Show();
                Finish();
                OverridePendingTransition(Resource.Animation.out_from_bottom, Resource.Animation.out_to_top);
            }
            else
                Snackbar.Make((View)sender, "Sorry, There was some problem, Please try later", Snackbar.LengthLong).SetAction("RETRY",v=> {
                    RegisterButton_Click(sender, e);
                }).Show();
        }
        private bool ValidateEmail(string email)
        {
            if (email.Length < 8)
                return false;
            if (email.Substring(2).IndexOf("@") == -1)
                return false;
            if (email.Substring(5).IndexOf(".") == -1)
                return false;
            return true;
        }
    }
}