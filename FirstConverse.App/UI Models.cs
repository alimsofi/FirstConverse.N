using FirstConverse.Shared.BindingModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace FirstConverse.Shared
{

    //public class ResponseConversationHeaders
    //{
    //    public string Status { get; set; }
    //    public int Count { get; set; }
    //    public List<ResponseHeaderItems> Results { get; set; }
    //}
    //public class ResponseHeaderItems
    //{
    //    public int Id { get; set; }
    //    public int RecipientCount { get; set; }
    //    public int ResponseCount { get; set; }
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public ImportanceLevel ImportanceLevel { get; set; }
    //    public string Subject { get; set; }
    //    public string Body { get; set; }
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public MessageState State { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    public TemplateType TemplateType { get; set; }
    //    public UserViewModel CreatedBy { get; set; }
    //    public UserViewModel UpdatedBy { get; set; }
    //}

    public class ResponseHeadersViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultStatus Status { get; set; }
        public int Count { get; set; }
        public ResponseConversationHeaders Forms { get; set; }
        public ResponseConversationHeaders TextMessages { get; set; }
        public ResponseConversationHeaders Surveys { get; set; }
        public ResponseConversationHeaders MeetingInvites { get; set; }
        public ResponseConversationHeaders PermissionRequests { get; set; }
    }

    public class ResponseConversationHeaders
    {
        public int Count { get; set; }
        public List<MessageHeadersViewModel> Items { get; set; }
    }

    public class MessageHeadersViewModel
    {
        public int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ImportanceLevel ImportanceLevel { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageState? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TemplateType TemplateType { get; set; }
        public bool IsViewed { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ConversationType ConversationType { get; set; }
        public UserViewModel Sender { get; set; }
        public string From { get; set; }
    }
    public enum ResultStatus
    {
        Success = 1,
        Failure = 0
    }
    public enum MessageState
    {
        Open = 1,
        Closed = 2
    }

    public enum ImportanceLevel
    {
        Normal,
        Low,
        High
    }

    public enum TemplateType
    {
        Text = 1,
        Form = 2,
        MeetingInvite = 3,
        Survey = 4,
        PermissionRequest = 5
    }
    public enum ConversationType
    {
        Request = 1,
        Response = 2
    }

    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }


    public class MessageDetailsResponse
    {
        public string Status { get; set; }
        public ConversationViewModel Result { get; set; }
    }

    public class ConversationViewModel
    {
        public string Body { get; set; }
        public UserViewModel Sender { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MessageId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ImportanceLevel ImportanceLevel { get; set; }
        public AbstractTemplateViewModel MessageTemplate { get; set; }
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ConversationStatus Status { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
    }

    public class AbstractTemplateViewModel : AbstractModel
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TemplateStatus Status { get; set; }
        public DateTime? PublishedDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TemplateType Type { get; set; }
        public List<AbstractControlInfo> Controls { get; set; }
        public List<QuestionAnswer> QuestionAnswers { get; set; }
    }

    #region Form Controls Definition
    public class AbstractControlBase : AbstractModel
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public int RowIndex { get; set; }
    }
    public class AbstractControl : AbstractControlBase
    {
        public IList<Item> Items { get; set; }
        public IList<Item> SelectedItems { get; set; }
        public int SelectedIndex { get; set; }
        public Item SelectedItem { get; set; }

        public string SelectedText
        {
            get
            {
                if (SelectedItem != null)
                {
                    return SelectedItem.Text;
                }
                else
                { return null; }
            }
        }
        public long SelectedValue
        {
            get
            {
                if (SelectedItem != null)
                {
                    return SelectedItem.Id;
                }
                else
                {
                    return -1;
                }
            }
        }
        public DateTime? Value { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
    }
    public class Item { public string Text { get; set; } public long Id { get; set; } }
    public class ComboBox : AbstractControlBase
    {
        public int SelectedIndex { get; set; }
        public List<Item> Items { get; set; }
        public Item SelectedItem { get; set; }

        public string SelectedText
        {
            get
            {
                if (SelectedItem != null)
                {
                    return SelectedItem.Text;
                }
                else
                { return null; }
            }
        }
        public long SelectedValue
        {
            get
            {
                if (SelectedItem != null)
                {
                    return SelectedItem.Id;
                }
                else
                {
                    return -1;
                }
            }
        }



    }
    public class DateTimeBox : AbstractControlBase
    {
        public DateTime? Value { get; set; }
    }
    public class CheckBox : AbstractControlBase
    {
        public bool Checked { get; set; }
    }

    public class TextBox : AbstractControlBase
    {
        public string Text { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }



    }

    public class ListBox : AbstractControlBase
    {
        public IList<Item> Items { get; set; }
        public IList<Item> SelectedItems { get; set; }



    }

    public class FormResponseBindingModel : ResponseBindingModel
    {
        public List<AbstractControlInfo> Controls { get; set; }
        [JsonIgnore]
        public override string Content
        {
            get
            {
                string json = JsonConvert.SerializeObject(new { Controls = Controls }, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.None,
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                return json;
            }
            set
            {
                var content = JsonConvert.DeserializeObject<FormResponseBindingModel>(value);
                Controls = content.Controls;
            }
        }
    }
    public class SurveyResponseBindingModel : ResponseBindingModel
    {
        public IList<QuestionAnswerBindingModel> QuestionAnswer { get; set; }
        [JsonIgnore]
        public override string Content
        {
            get
            {
                string json = JsonConvert.SerializeObject(new { QuestionAnswer = QuestionAnswer }, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.None,
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                return json;
            }
            set
            {
                var userResponse = JsonConvert.DeserializeObject<SurveyResponseBindingModel>(value);
                QuestionAnswer = userResponse.QuestionAnswer;
            }
        }
    }
    public class QuestionAnswerBindingModel
    {
        public long QuestionId { get; set; }
        public long AnswerId { get; set; }
        public string OtherAnswer { get; set; }
    }

    #endregion
    public enum TemplateStatus
    {
        Draft,
        Published,
    }
    public enum ConversationStatus
    {
        Sent = 1,
        Closed = 2,
        Cancelled = 3,
        Recalled = 4
    }
    public class AbstractModel
    {
        public int Id { get; set; }
    }
    public class NotificationData
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("body")]
        public string Message { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        //[JsonProperty("id")]
        //public long MessageId { get; set; }
    }
    public class QuestionAnswer
    {
        public Item Question { get; set; }
        public List<Item> Answers { get; set; }
        public List<Item> SelectedAnswers { get; set; }
    }
}
