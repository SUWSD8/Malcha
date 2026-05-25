using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Malcha.Model
{
    public class ModelHistory
    {
        [JsonPropertyName("loss")]
        public List<double> Loss { get; set; }
        [JsonPropertyName("val_loss")]
        public List<double> ValLoss { get; set; }
    }
    public class  DatabaseEntry
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("History")]
        public ModelHistory History { get; set; }
    }
}
