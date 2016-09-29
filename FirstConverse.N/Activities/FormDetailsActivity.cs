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
using Android.Support.V7.App;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using FirstConverse.Shared.Services;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using FirstConverse.Shared.BindingModel;
using FirstConverse.Shared;
using Newtonsoft.Json;

namespace FirstConverse.N.Droid
{
    [Activity(Label = "...", Theme = "@style/Theme.FC_Design")]
    public class FormDetailsActivity : AppCompatActivity
    {
        public bool ReadOnly = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.viewFormDetail);
            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ReadOnly = this.Intent.GetStringExtra("type") == "Response";
            this.FindViewById<Button>(Resource.Id.btnFormDetailsSubmit).Visibility = ReadOnly ? ViewStates.Invisible : ViewStates.Visible;

            this.FindViewById<Button>(Resource.Id.btnFormDetailsSubmit).Click += async (o,e)=>
             {
                 ProgressDialog waitDialog = new ProgressDialog(this);
                 waitDialog.SetMessage("Sending Reply...");
                 waitDialog.SetCancelable(false);
                 waitDialog.Show();
                 var data = ConversationDetail.Result.MessageTemplate.Controls;
                 
                 LinearLayout root = FindViewById<LinearLayout>(Resource.Id.formsLayout);
                 
                 for (int index = 0; index < root.ChildCount; index++)
                 {
                     View c = root.GetChildAt(index);
                     if (c.GetType() == typeof(EditText))
                     {
                         EditText formTxt = ((EditText)c);
                         string tag = formTxt.GetTag(Resource.Id.tag_textBox).ToString();
                         for (int i = 0; i < data.Count; i++)
                         {
                             AbstractControlInfo control = data[i];
                             if (tag == control.RowIndex + control.Name)
                             {
                                 ((TextBoxInfo)control).Text = formTxt.Text;
                                 break;
                             }
                         }
                     }
                     else if (c.GetType() == typeof(Switch))
                     {
                         Switch formTxt = ((Switch)c);
                         string tag = formTxt.GetTag(Resource.Id.tag_checkbox).ToString();
                         for (int i = 0; i < data.Count; i++)
                         {
                             AbstractControlInfo control = data[i];
                             if (tag == control.RowIndex + control.Name)
                             {
                                 ((CheckBoxInfo)data[i]).Checked = formTxt.Checked;
                                 break;
                             }
                         }
                     }
                     else if (c.GetType() == typeof(Spinner))
                     {
                         Spinner formTxt = ((Spinner)c);
                         string tag = formTxt.GetTag(Resource.Id.tag_combobox).ToString();
                         for (int i = 0; i < data.Count; i++)
                         {
                             AbstractControlInfo control = data[i];
                             if (tag == control.RowIndex + control.Name)
                             {
                                 ((ComboBoxInfo)control).SelectedItem = new Item() { Id = formTxt.SelectedItemId, Text = formTxt.SelectedItem.ToString() };
                                 break;
                             }
                         }
                     }
                     
                 }

                 FormResponseBindingModel response = new FormResponseBindingModel() { Controls = ConversationDetail.Result.MessageTemplate.Controls };
                 string result = await SubmitFormResponse(response);
                 waitDialog.Hide();
                 if (result == "SUCCESS")
                 {
                     var prefs = GetSharedPreferences(this.PackageName, FileCreationMode.Private);
                     var edit = prefs.Edit();
                     edit.PutInt("status_update_id", response.Id);
                     edit.Commit();
                     //UpdateDataInSharedPref(response.Id);
                     Toast.MakeText(this, "Form Response Sent!", ToastLength.Long).Show();
                     this.Finish();
                 }
                 else
                     Toast.MakeText(this, "Sorry, Please try again later.", ToastLength.Long).Show();
                 
             };
            LoadBackDrop();
            LoadData();
        }
        
        private async Task<string> SubmitFormResponse(FormResponseBindingModel response)
        {
            return await RestClient.SubmitFormResponse(this.Intent.GetStringExtra("auth_token"), this.Intent.GetIntExtra("MsgId", 0), response);
        }

        MessageDetailsResponse ConversationDetail;
        private async void LoadData()
        {
            ConversationDetail = await LoadConversationDetails();
            
            LinearLayout root = FindViewById<LinearLayout>(Resource.Id.formsLayout);

            TextView lable; EditText textBox; Spinner comboBox;
            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.form_collapsing_toolbar);
            collapsingToolBar.Title = ConversationDetail.Result.Subject != null && ConversationDetail.Result.Subject.Length > 20 ? ConversationDetail.Result.Subject.Substring(0, 20) + "..." : ConversationDetail.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblFormDetailSubject).Text = ConversationDetail.Result.Subject;
            FindViewById<TextView>(Resource.Id.lblFormDetailBody).Text = ConversationDetail.Result.Body;
            FindViewById<TextView>(Resource.Id.lblFormDetailSender).Text = ConversationDetail.Result.Sender.FirstName + " " + ConversationDetail.Result.Sender.LastName;
            FindViewById<TextView>(Resource.Id.lblFormDetailDateTime).Text = ConversationDetail.Result.SentDate.ToString("mmm-dd-yyyy hh:MM");
            foreach (AbstractControlInfo control in ConversationDetail.Result.MessageTemplate.Controls)
            {

                lable = new TextView(this);
                lable.Text = control.Caption;
                
                lable.SetTextColor(Resources.GetColor(Resource.Color.black));
                root.AddView(lable);
                if (control.Type == "MultilineTextBox" || control.Type == "TextBox")
                {
                    if (!ReadOnly)
                    {
                        textBox = new EditText(this);
                        
                        textBox.Text = ((TextBoxInfo)control).Text;
                        if (control.Type == "MultilineTextBox")
                        {
                            textBox.InputType = Android.Text.InputTypes.TextFlagMultiLine;
                            textBox.SetSingleLine(false);
                            textBox.SetMinLines(2);
                            textBox.SetMaxLines(2);
                            textBox.SetLines(2);
                            int maxLength = 250;
                            if (((MultilineTextBoxInfo)control).MaxLength > 0)
                                maxLength = ((MultilineTextBoxInfo)control).MaxLength;
                            textBox.SetFilters(new Android.Text.IInputFilter[] { new Android.Text.InputFilterLengthFilter(maxLength) });
                        }
                        else
                        {
                            int maxLength = 100;
                            if (((TextBoxInfo)control).MaxLength > 0)
                                maxLength = ((TextBoxInfo)control).MaxLength;
                            textBox.SetFilters(new Android.Text.IInputFilter[] { new Android.Text.InputFilterLengthFilter(maxLength) });
                            textBox.InputType = Android.Text.InputTypes.TextVariationEmailAddress;
                        }

                        textBox.SetTag(Resource.Id.tag_textBox, control.RowIndex + control.Name);
                        root.AddView(textBox);
                    }
                    else
                    {
                        lable = new TextView(this);
                        lable.Text = ((TextBoxInfo)control).Text;
                        lable.Gravity = GravityFlags.Left;
                        lable.SetPadding(30, 0, 0, 0);
                        lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));
                        root.AddView(lable);
                    }
                }
                else if (control.Type == "ComboBox")
                {
                    if (!ReadOnly)
                    {
                        ComboBoxInfo c = ((ComboBoxInfo)control);
                        comboBox = new Spinner(this);
                        
                        comboBox.SetTag(Resource.Id.tag_combobox, control.RowIndex + control.Name);
                        comboBox.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, c.Items.Select(k => k.Text).ToArray<string>());
                        comboBox.SetSelection(0);
                        root.AddView(comboBox);
                    }
                    else
                    {
                        lable = new TextView(this);
                        lable.Gravity = GravityFlags.End;
                        lable.Text = ((ComboBoxInfo)control).SelectedItem.Text;
                        lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));
                        root.AddView(lable);
                    }
                }
                else if (control.Type == "ListBox")
                {
                    ListBoxInfo c = ((ListBoxInfo)control);
                    lable = new TextView(this);
                    string[] arr = c.Items.Select(e => e.Text).ToArray<string>();
                    bool[] sel = new bool[arr.Length];
                    if (c.SelectedItems == null)
                        c.SelectedItems = new List<Item>();
                    Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                    
                    dialog.SetTitle(c.Caption).SetMultiChoiceItems(arr, sel, new EventHandler<DialogMultiChoiceClickEventArgs>(
                        delegate (object sender, DialogMultiChoiceClickEventArgs args)
                        {
                            Item newItem = c.Items[args.Which];
                            if ((!args.IsChecked) && c.SelectedItems != null && c.SelectedItems.Contains(newItem))
                            { c.SelectedItems.Remove(newItem); sel[args.Which] = false; }
                            else if (args.IsChecked && (!c.SelectedItems.Contains(newItem)))
                            { c.SelectedItems.Add(newItem); sel[args.Which] = true; }
                        }));

                    dialog.SetPositiveButton("OK", new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs args)
                    {
                        lable.Text = "";
                        foreach (Item i in c.SelectedItems)
                            lable.Text += i.Text + ", ";
                    }));

                    //dialog.SetNegativeButton("CANCEL", new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs args)
                    //{  }));

                    //dialog.SetNeutralButton("CLEAR", new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs args) {
                    //    if (c.SelectedItems != null)
                    //        c.SelectedItems.Clear();
                    //    lable.Text = "Select " + c.Caption;
                        
                    //}));

                    dialog.Create();
                    
                    lable.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                    lable.Gravity = GravityFlags.End;
                    
                    if (!ReadOnly)
                    {
                        lable.Click += delegate
                        {
                            dialog.Show();
                        };
                        lable.Text = "Select " + c.Caption;
                    }
                    else
                    {
                        if (c.SelectedItems != null)
                            foreach (Item i in c.SelectedItems)
                                lable.Text += i.Text + ", ";
                    }
                    lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));

                    root.AddView(lable);
                }
                else if (control.Type == "DateTimeBox")
                {
                    lable = new TextView(this);
                    
                    lable.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                    lable.Gravity = GravityFlags.End;
                    lable.SetTag(Resource.Id.tag_datecontrol, control.RowIndex + control.Name);
                    if (!ReadOnly)
                    {
                        lable.Click += delegate (object sender, EventArgs args)
                        {
                            TextView txt = (TextView)sender;
                            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime selectedDate)
                            {
                                txt.Text = selectedDate.ToLongDateString();
                                ((DateTimeBoxInfo)control).Value = selectedDate;
                            });
                            frag.Show(FragmentManager, DatePickerFragment.TAG);
                        };
                        if (((DateTimeBoxInfo)control).Value.Year > 1900)
                            lable.Text = ((DateTimeBoxInfo)control).Value.ToLongDateString();
                        else
                            lable.Text = "Choose Date";
                    }
                    else
                        lable.Text = ((DateTimeBoxInfo)control).Value.ToLongDateString();
                    lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));
                    
                    root.AddView(lable);
                }
                else if (control.Type == "CheckBox")
                {
                    if (!ReadOnly)
                    {
                        Switch toggle = new Switch(this);
                        //toggle.Enabled = !ReadOnly;
                        toggle.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));
                        if (Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.Kitkat)
                        {
                            toggle.TextOn = "YES"; toggle.TextOff = "NO"; toggle.ShowText = true;
                        }
                        toggle.SetTag(Resource.Id.tag_checkbox, control.RowIndex + control.Name);
                        toggle.Checked = ((CheckBoxInfo)control).Checked;
                        root.AddView(toggle);
                    }
                    else
                    {
                        lable = new TextView(this);
                        lable.Gravity = GravityFlags.End;
                        lable.Text = ((CheckBoxInfo)control).Checked ? "Yes" : "No";
                        lable.SetTextColor(Resources.GetColor(Resource.Color.dark_blue));
                        root.AddView(lable);
                    }
                }
                if (ReadOnly)
                {
                    lable = new TextView(this);
                    lable.SetHeight(1);
                    lable.SetWidth(LinearLayout.LayoutParams.MatchParent);
                    lable.SetBackgroundColor(Resources.GetColor(Resource.Color.my_teal));
                    root.AddView(lable);
                }
            }
            
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
        public async Task<MessageDetailsResponse> LoadConversationDetails()
        {
            return await RestClient.GetMessageDetails(this.Intent.GetStringExtra("auth_token"), this.Intent.GetIntExtra("MsgId", 0));
        }
    }
}