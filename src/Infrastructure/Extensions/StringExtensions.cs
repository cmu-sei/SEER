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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Http;
using NLog.Fluent;

namespace Seer.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ReadAsList(this IFormFile file)
        {
            var result = new StringBuilder();
            using var reader = new StreamReader(file.OpenReadStream());
            while (reader.Peek() >= 0)
                result.AppendLine(reader.ReadLine());
            return result.ToString();
        }

        public static string TrimImport(this string o)
        {
            if (string.IsNullOrEmpty(o)) return o;
            return o.Replace("\"", string.Empty).Trim();
        }

        public static string TitleCase(this string o)
        {
            if (string.IsNullOrEmpty(o)) return o;
            o = o.ToLower();
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(o);
        }


        public static IEnumerable<string> GetEmails(this string o)
        {
            var splitText = o.Split(new[] { ' ' });
            var emails = splitText.Where(s => s.Contains("@"));
            return emails;
        }

        public static string CapitalizeFirst(this string o)
        {
            if (string.IsNullOrEmpty(o)) return o;
            var isNew = true;
            var result = new StringBuilder(o.Length);
            foreach (var t in o)
            {
                if (isNew && char.IsLetter(t))
                {
                    result.Append(char.ToUpper(t));
                    isNew = false;
                }
                else
                    result.Append(t);

                if (t == '!' || t == '?' || t == '.')
                {
                    isNew = true;
                }
            }

            var s = result.ToString();
            foreach (var email in s.GetEmails())
            {
                s = s.Replace(email, email.ToLower());
            }

            return o;
        }
        
        public static string TruncateLongString(this string str, int maxLength) =>
            str?[0..Math.Min(str.Length, maxLength)];

        public static string Clean(this string o)
        {
            if (string.IsNullOrEmpty(o)) return "";
            o = o.StripXss();
            o = Regex.Replace(o, @"[\r\n]{2,}", Environment.NewLine);

            return o
                .Replace("**", "").Replace(" to ,", ", ")
                .Replace(",", ", ").Replace(",  ", ", ")
                .Replace(",", " ").TruncateLongString(2500);
            //2704 is max for postgres
        }

        public static string StripXss(this string o)
        {
            if (string.IsNullOrEmpty(o)) return o;
            return o.Replace("<script>", "").Replace("</script>", "").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string BreakUpLongStrings(this string s, int maxCharsInLine)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.StripXss();
            if (s.Length <= maxCharsInLine)
            {
                return s;
            }

            var breakPos = maxCharsInLine;
            while (breakPos > 0 && s[breakPos] != ' ')
            {
                breakPos--;
            }

            var charOffset = 1;
            if (breakPos == 0)
            {
                breakPos = maxCharsInLine;
                charOffset = 0;
            }

            return s.Substring(0, breakPos) + ' ' +
                   BreakUpLongStrings(s.Substring(breakPos + charOffset), maxCharsInLine);
        }

        public static string GetPublicUrl(this string location)
        {
            if (!location.ToLower().Contains("wwwroot")) return string.Empty;

            var path = location.ToLower();
            var pos = path.IndexOf("wwwroot", StringComparison.InvariantCultureIgnoreCase);
            path = path.Substring(pos, path.Length - pos);
            path = path.Replace("wwwroot", "");
            return path;
        }

        public static string MakeEmailAddress(this string o)
        {
            try
            {
                if (string.IsNullOrEmpty(o))
                    return $"{Guid.NewGuid().ToString()}@site.local";
                return o.Contains('@') ? o.ToLower() : $"{o}@site.local".ToLower();
            }
            catch (Exception)
            {
                Console.WriteLine("Error in making email address");
                return $"{Guid.NewGuid().ToString()}@site.local";
            }
        }
        
        public static IEnumerable<int> ToIntList(this string str) {
            if (string.IsNullOrEmpty(str))
                yield break;

            foreach(var s in str.Split(',')) {
                if (int.TryParse(s, out var num))
                    yield return num;
            }
        }

        public static string RemoveSubstringsByArray(this string x, IEnumerable<string> stringsToRemove)
        {
            foreach (var s in stringsToRemove)
            {
                if (x.Contains(s))
                {
                    x = x.Replace(s, "");
                }
            }

            if (x.Length > 1)
            {
                x = x.Trim();
            }
            
            return x;
        }
    }
}