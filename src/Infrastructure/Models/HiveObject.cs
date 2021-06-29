/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace Seer.Infrastructure.Models
{
    /// <summary>
    /// var myDeserializedClass = JsonConvert.DeserializeObject HiveObject (myJsonResponse); 
    /// </summary>
    public class HiveObject
    {
        [JsonProperty("rootId")] public string Id { get; set; }
        [JsonProperty("requestId")] public string RequestId { get; set; }
        [JsonProperty("details")] public ExpandoObject Details { get; set; }
        [JsonProperty("operation")] public string Operation { get; set; }
        [JsonProperty("objectId")] public string ObjectId { get; set; }
        [JsonProperty("startDate")] public long StartDate { get; set; }
        [JsonProperty("objectType")] public string ObjectType { get; set; }
        [JsonProperty("base")] public bool IsBase { get; set; }
        [JsonProperty("object")] public HiveRoot BaseObject { get; set; }

        public class HiveRoot
        {
            [JsonProperty("severity")] public int Severity { get; set; }
            [JsonProperty("owner")] public string Owner { get; set; }
            [JsonProperty("_routing")] public object Routing { get; set; }
            [JsonProperty("flag")] public bool Flag { get; set; }
            [JsonProperty("updatedBy")] public string UpdatedBy { get; set; }
            [JsonProperty("customFields")] public object CustomFields { get; set; }
            [JsonProperty("_type")] public string Type { get; set; }
            [JsonProperty("description")] public object Description { get; set; }
            [JsonProperty("title")] public object Title { get; set; }
            [JsonProperty("tags")] public List<object> Tags { get; set; }
            [JsonProperty("createdAt")] public long CreatedAt { get; set; }
            [JsonProperty("_parent")] public object Parent { get; set; }
            [JsonProperty("createdBy")] public string CreatedBy { get; set; }
            [JsonProperty("caseId")] public int CaseId { get; set; }
            [JsonProperty("tlp")] public int Tlp { get; set; }
            [JsonProperty("metrics")] public object Metrics { get; set; }
            [JsonProperty("_id")] public object _Id { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("_version")] public int Version { get; set; }
            [JsonProperty("pap")] public int Paa { get; set; }
            [JsonProperty("startDate")] public long StartDate { get; set; }
            [JsonProperty("updatedAt")] public long UpdatedAt { get; set; }
            [JsonProperty("status")] public string Status { get; set; }
            [JsonProperty("data")] public object Data { get; set; }
            [JsonProperty("dataType")] public object DataType { get; set; }
        }
    }
}