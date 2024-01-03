namespace Az.Storage
{
    using Azure;
    using Azure.Data.Tables;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    public class AzTableEntity : ITableEntity
    {
        #region ITableEntity
        [JsonIgnore]
        public string PartitionKey { get; set; }
        [JsonIgnore]
        public string RowKey { get; set; }
        [JsonIgnore]
        public DateTimeOffset? Timestamp { get; set; }
        [JsonIgnore]
        public ETag ETag { get; set; }
        #endregion

        #region JSON Purpose
        [JsonIgnore]
        [IgnoreDataMember]
        protected string SplitBy = "-";
        [JsonIgnore]
        [IgnoreDataMember]
        protected int SplitAt = 0;
        [IgnoreDataMember]
        public string ID { 
            get => SplitAt>0 ? this.RowKey : $"{this.PartitionKey}{this.SplitBy}{this.RowKey}";
            set {
                if (this.SplitAt > 0)
                {
                    if (value.Length < SplitAt) throw new InvalidEnumArgumentException($"ID should be at least {SplitAt} characters.");
                    this.RowKey = value;
                    this.PartitionKey = value.Substring(0, SplitAt);
                }
                else
                {
                    if (value.IndexOf(SplitBy) <= 0) throw new InvalidEnumArgumentException($"ID should contain '{SplitBy}'.");
                    var vals = value.Split(this.SplitBy);
                    if (vals.Length != 2) throw new InvalidEnumArgumentException($"ID should contain '{SplitBy}' only once.");
                    this.PartitionKey = vals[0];
                    this.RowKey = vals[1];
                }
            }
        }
        #endregion
    }
}
