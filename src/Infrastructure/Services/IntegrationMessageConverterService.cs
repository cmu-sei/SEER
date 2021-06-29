/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
namespace Seer.Infrastructure.Services
{
    public class IntegrationMessageConverterService
    {
        public readonly HiveObject HiveObject;
        public IEnumerable<KeyValuePair<string, string>> Updates { get; private set; }
        public EventDetailHistory Detail { get; set; } = new();

        public IntegrationMessageConverterService(string payload)
        {
            try
            {
                this.HiveObject = JsonConvert.DeserializeObject<HiveObject>(payload);
            }
            catch
            {
                throw new Exceptions.HiveFormattingException($"Cannot Deserialize HiveObject. Payload is not formatted as expected: [{payload}]");
            }

            this.Detail.User = new User();
            this.Detail.AssessmentId = -1;
            this.Detail.EventId = -1;

            this.Detail.HistoryType = this.HiveObject.ObjectType.ToUpper().Replace("CASETEMPLATE", "CASE TEMPLATE").Replace("_", " ");
            this.Detail.HistoryAction = this.HiveObject.Operation.ToUpper();
            this.Detail.User.UserName = this.HiveObject.BaseObject.CreatedBy;
            this.Detail.Created = this.HiveObject.StartDate.FromJavaTimeStampToDateTime().ToUniversalTime();
            this.Detail.IntegrationId = this.HiveObject.Id;
            this.Detail.IntegrationObject = JsonConvert.SerializeObject(this.HiveObject).StripXss();
            this.Detail.IntegrationRequestId = this.HiveObject.RequestId;

            this.SetAssessmentAndEvent();
            this.SetUpdates();
            this.SetMessage();
            this.SetTags();
        }

        private void SetMessage()
        {
            switch (this.Detail.HistoryType)
            {
                default: //USER | ALERT | PROFILE
                    this.Detail.Message = $"{this.Detail.HistoryAction} {this.Detail.HistoryType}".TitleCase();
                    break;
                case "CASE TEMPLATE":
                    this.Detail.Message = $"{this.Detail.HistoryType} {this.Detail.HistoryAction}".TitleCase();
                    break;
                case "TASK":
                case "CASE":
                case "CASE TASK LOG":
                case "CASE TASK":
                case "CASE ARTIFACT":
                    this.Detail.Message = $"{this.Detail.IntegrationId} {this.Detail.HistoryType} {this.Detail.HistoryAction}".TitleCase();
                    if (this.Detail.HistoryAction == "UPDATE")
                    {
                        var details = this.Updates
                            .Select(update => $"{update.Key} to {update.Value}").ToList();
                        var o = $": {string.Join(",", details)}";
                        o = ReplaceUgly(o);
                        this.Detail.Message += o;

                    }
                    break;
            }

            this.Detail.Message = this.Detail.Message.Clean();
        }

        private string ReplaceUgly(string o)
        {
            if (string.IsNullOrEmpty(o)) return o;
            //o = o.SplitCamelCase();
            return o.CapitalizeFirst();
        }

        private void SetUpdates()
        {
            var updates = new List<KeyValuePair<string, string>>();
            foreach (var (key, value) in this.HiveObject.Details)
            {
                if (value == null) continue;

                if (value.GetType().ToString().Contains("List"))
                {
                    updates.Add(new KeyValuePair<string, string>(key,
                        string.Join(", ", (List<object>)value)));
                }
                else if (value.GetType().ToString().Contains("ExpandoObject"))
                {
                    var s = new StringBuilder();
                    foreach (var (key1, value2) in (ExpandoObject)value)
                    {
                        if (value2 != null && value2.GetType().ToString().Contains("ExpandoObject"))
                        {
                            foreach (var item3 in (ExpandoObject)value2)
                            {
                                s.Append($" {item3.Value},");
                            }
                        }
                        else
                        {
                            s.Append($" {key1}:{value2},");
                        }
                    }

                    updates.Add(new KeyValuePair<string, string>(key, s.ToString().Trim().TrimEnd(',')));
                }
                else
                {
                    updates.Add(new KeyValuePair<string, string>(key, value?.ToString()));
                }

            }
            this.Updates = updates.Distinct().OrderBy(x => x.Key);
        }

        private void SetTags()
        {
            var tags = new List<string>();
            //explicit tags
            if (this.HiveObject.BaseObject.Tags != null)
            {
                foreach (var tag in this.HiveObject.BaseObject.Tags)
                {
                    var tagString = tag.ToString();
                    if (string.IsNullOrEmpty(tagString)) continue;
                    tags.Add(tagString);
                }
            }
            //implicit tags
            var regex = @"(?<=\s|^)#(\w*[A-Za-z_]+\w*)";
            foreach (var tag in Regex.Matches(this.Detail.IntegrationObject, regex))
            {
                var tagString = tag.ToString();
                if (string.IsNullOrEmpty(tagString)) continue;
                tags.Add(tagString);
            }
            this.Detail.SetTags(string.Join(",", tags));
        }

        private readonly string[] SplitChars = { ":", ",", "=" };

        private void SetAssessmentAndEvent()
        {
            if (this.HiveObject.BaseObject.Tags == null) return;

            foreach (var tag in this.HiveObject.BaseObject.Tags)
            {
                if (tag == null) continue;
                foreach (var splitChar in SplitChars)
                {
                    var tagString = tag.ToString();
                    if (string.IsNullOrEmpty(tagString) || !tagString.Contains(splitChar)) continue;

                    var tagArray = tagString.Split(Convert.ToChar(splitChar));
                    if (tagArray[0].ToUpper().StartsWith("ASSESSMENT"))
                        this.Detail.AssessmentId = Convert.ToInt32(tagArray[1].Replace("\"", ""));
                    if (tagArray[0].ToUpper().StartsWith("EVENT"))
                        this.Detail.EventId = Convert.ToInt32(tagArray[1].Replace("\"", ""));
                }
            }
        }
    }
}