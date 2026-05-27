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

        // n_outputs0_loss, val_n_outputs0_loss 등 추가
    }
    public class  DatabaseEntry
    {
        [JsonPropertyName("Number")]
        public int Number { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Pilot")]
        public string Pilot { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Tubs")]
        public string Tubs { get; set; }

        [JsonPropertyName("Time")]
        public double Time { get; set; }
        [JsonPropertyName("Transfer")]
        public string Transfer { get; set; }
        [JsonPropertyName("Comment")]
        public string Comment { get; set; }

        [JsonPropertyName("History")]
        public ModelHistory History { get; set; }
    }
}
