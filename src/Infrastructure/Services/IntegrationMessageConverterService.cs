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
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class IntegrationMessageConverterService
    {
        private readonly ApplicationDbContext _dbContext;
        
        public readonly HiveObject HiveObject;
        public IEnumerable<KeyValuePair<string, string>> Updates { get; private set; }
        public EventDetailHistory Detail { get; set; } = new();

        public IntegrationMessageConverterService(string payload, ApplicationDbContext dbContext)
        {
            try
            {
                this.HiveObject = JsonConvert.DeserializeObject<HiveObject>(payload);
                if (this.HiveObject == null)
                    throw new Exception();
            }
            catch(Exception e)
            {
                // TODO: need to put failed requests somewhere
                throw new Exceptions.HiveFormattingException($"Cannot Deserialize HiveObject. Payload is not formatted as expected: [{payload}] {e}");
            }
            
            this._dbContext = dbContext;
        
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

            this.SetTags();
            this.SetAssessmentAndEvent();
            this.SetUpdates();
            this.SetMessage();
        }

        private void SetMessage()
        {
            var (_, assignee) = this.HiveObject.Details.FirstOrDefault(x => x.Key is "assignee" or "owner");
            var title = this.HiveObject.BaseObject.Title;
            var (_, status) = this.HiveObject.Details.FirstOrDefault(x => x.Key is "status");
            
            switch (this.Detail.HistoryType)
            {
                default: //USER | ALERT | PROFILE
                    this.Detail.Message = $"{this.Detail.HistoryAction} {this.Detail.HistoryType}".TitleCase();
                    break;
                case "CASE TEMPLATE":
                    this.Detail.Message = $"{this.Detail.HistoryType} {this.Detail.HistoryAction}".TitleCase();
                    break;
                case "CASE TASK":
                case "CASE TASK LOG":
                    if (assignee != null && this.Detail.HistoryType == "CASE TASK")
                    {
                        this.Detail.Message = $"Assigned task \"{title}\" to {assignee}";
                        break;
                    }
                    if (status != null && this.Detail.HistoryType == "CASE TASK")
                    {
                        this.Detail.Message = $"Task \"{title}\" status set to {status.ToString()?.ToLower()}";
                        break;
                    }
                    var (_, message) = this.HiveObject.Details.FirstOrDefault(x => x.Key == "message");
                    if (message != null)
                    {
                        this.Detail.Message = $"Task log updated: {ReplaceUgly(message.ToString())}".TitleCase();
                    }
                    else
                    {
                        //what was updated specifically?
                        foreach (var detail in this.Updates.ToArray())
                        {
                            this.Detail.Message += $"Task {detail.Key} set to \"{detail.Value}\". ";
                        }
                    }
                    break;
                case "TASK":
                case "CASE":
                case "CASE ARTIFACT":
                    if (assignee != null && this.Detail.HistoryType == "CASE")
                    {
                        this.Detail.Message = $"Assigned case \"{title}\" to {assignee}";
                        break;
                    }
                    this.Detail.Message = $"{this.Detail.HistoryType} {this.Detail.HistoryAction}".TitleCase();
                    if (this.Detail.HistoryAction == "UPDATE")
                    {
                        var details = this.Updates.Select(update => $"{update.Key} to \"{update.Value}\"").ToList();
                        var o = $": {string.Join(",", details)}";
                        o = ReplaceUgly(o);
                        this.Detail.Message += o;
                    }
                    break;
            }

            this.Detail.Message = this.Detail.Message.Clean();
        }

        private static string ReplaceUgly(string o)
        {
            var x = string.IsNullOrEmpty(o) ? o : o.CapitalizeFirst();
            var junkArray = new[] { "to \"System.Dynamic.ExpandoObject\"" };
            x = x.RemoveSubstringsByArray(junkArray);
            return x;
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
                    updates.Add(new KeyValuePair<string, string>(key, value.ToString()));
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

        private readonly string[] SplitChars = { ":", "=" };

        private void SetAssessmentAndEvent()
        {
            // find this event by the integration id
            var assessmentEvent = IntegrationMessageService.FindId(this._dbContext, this.HiveObject);
            if (assessmentEvent.AssessmentId > 0)
            {
                this.Detail.AssessmentId = assessmentEvent.AssessmentId;
                this.Detail.EventId = assessmentEvent.EventId;
            }
            else
            {
                var assessment = string.Empty;
                var team = string.Empty;
                var inject = string.Empty;
                
                // or by tags that are k:v format
                foreach (var t in this.Detail.Tags.Split(','))
                {
                    var tag = t.Trim();
                    foreach (var splitChar in SplitChars)
                    {
                        if (!tag.Contains(splitChar)) continue;

                        var tagArray = tag.Split(Convert.ToChar(splitChar));
                        
                        var key = tagArray[0].Trim().ToUpper();
                        var val = tagArray[1].Replace("\"", "");
                        switch (key)
                        {
                            case "ASSESSMENTID":
                            {
                                this.Detail.AssessmentId = Convert.ToInt32(tagArray[1].Replace("\"", ""));
                                break;
                            }
                            case "EVENTID":
                            {
                                if (int.TryParse(val, out var i))
                                {
                                    this.Detail.EventId = Convert.ToInt32(i);
                                }
                                else
                                {
                                    var ev = this._dbContext.Events.FirstOrDefault(x => x.Name.ToUpper() == t.ToUpper());
                                    if (ev != null)
                                    {
                                        this.Detail.EventId = ev.Id;
                                    }
                                }
                                break;
                            }
                            case "ASSESSMENT"://MC3?
                                assessment = val;
                                break;
                            case "TEAM"://GOLD
                                team = val;
                                break;
                            case "INJECT"://GCD-2022-38
                                inject = val;
                                break;
                        }
                    }
                }
                
                if((this.Detail.EventId < 1 || this.Detail.AssessmentId < 1) &&(!string.IsNullOrEmpty(team) && !string.IsNullOrEmpty(assessment)))
                {
                    //don't have all the ids, so find by the assessment, team, and inject name
                    var dbTeam = this._dbContext.Groups.FirstOrDefault(x => x.Name == team);
                    if (dbTeam != null)
                    {
                        var dbAssessment = this._dbContext.Assessments.FirstOrDefault(x => x.Name == assessment && x.GroupId == dbTeam.Id);
                        if (dbAssessment != null)
                        {
                            var dbEvent =  this._dbContext.Events.FirstOrDefault(x => x.Name.ToUpper() == inject.ToUpper() && x.AssessmentId == dbAssessment.Id);
                            if (dbEvent != null)
                            {
                                this.Detail.AssessmentId = dbEvent.AssessmentId;
                                this.Detail.EventId = dbEvent.Id;
                            }
                        }
                    }
                }
            }


            // match any that we missed
            IntegrationMessageService.TryMatchUnmatched(this._dbContext).GetAwaiter().GetResult();
        }
    }
}