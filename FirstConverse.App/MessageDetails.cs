using FC.Framework.WebApi.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
namespace FirstConverse.Shared.BindingModel
{
    public class MessageDetailsResponseObject : ResponseBindingModel
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
    public class ResponseBindingModel
    {
        public int Id { get; set; }
        [JsonIgnore]
        public virtual string Content { get; set; }
    }
    [JsonConverter(typeof(ControlInfoConverter))]
    public abstract class AbstractControlInfo
    {
        private class ControlInfoConverter : JsonCreationConverter<AbstractControlInfo>
        {
            protected override AbstractControlInfo Create(Type objectType, JObject jObject)
            {
                switch (jObject.Value<string>("Type"))
                {
                    case "TextBox":
                        return new TextBoxInfo();
                    case "ComboBox":
                        return new ComboBoxInfo();
                    case "CheckBox":
                        return new CheckBoxInfo();
                    case "DateTimeBox":
                        return new DateTimeBoxInfo();
                    case "ListBox":
                        return new ListBoxInfo();
                    case "MultilineTextBox":
                        return new MultilineTextBoxInfo();
                    default:
                        return null;
                }
            }
        }
        public int RowIndex { get; set; }
        public bool Required { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public string Name { get; set; } //Is unique within current form context
    }
    public class TextBoxInfo : AbstractControlInfo
    {
        public string Text { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
    }
    public class ComboBoxInfo : AbstractControlInfo
    {
        public List<Item> Items { get; set; }
        public Item SelectedItem { get; set; }
    }
    public class CheckBoxInfo : AbstractControlInfo
    {
        public bool Checked { get; set; }
    }
    public class DateTimeBoxInfo : AbstractControlInfo
    {
        public DateTime Value { get; set; }
    }
    public class ListBoxInfo : AbstractControlInfo
    {
        public List<Item> Items { get; set; }
        public List<Item> SelectedItems { get; set; }
    }
    public class MultilineTextBoxInfo : TextBoxInfo
    {
    }
    
   
}
namespace FC.Framework.WebApi.Formatters
{
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// this is very important, otherwise serialization breaks!
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
        /// <summary> 
        /// Create an instance of objectType, based properties in the JSON object 
        /// </summary> 
        /// <param name="objectType">type of object expected</param> 
        /// <param name="jObject">contents of JSON object that will be 
        /// deserialized</param> 
        /// <returns></returns> 
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType,
          object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            // Load JObject from stream 
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject 
            T target = Create(objectType, jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value,
          JsonSerializer serializer)
        {
            //throw new NotImplementedException();
        }
    }
}