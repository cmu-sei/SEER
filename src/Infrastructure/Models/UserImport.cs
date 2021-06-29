/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using FileHelpers;

namespace Seer.Infrastructure.Models
{
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class UserImport
    {
        public string Id;
        public int? GroupId;
        [FieldTrim(TrimMode.Both)]
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string GroupName;
        [FieldTrim(TrimMode.Both)]
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string FirstName;
        [FieldTrim(TrimMode.Both)]
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string LastName;
        [FieldTrim(TrimMode.Both)]
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string Rank;
        [FieldTrim(TrimMode.Both)]
        [FieldQuoted(QuoteMode.OptionalForBoth)]
        public string DutyPosition;
        [FieldTrim(TrimMode.Both)]
        public string Email;
        [FieldTrim(TrimMode.Both)]
        public string UserName;
        [FieldTrim(TrimMode.Both)]
        public string DefaultPassword;

        [FieldOptional]
        public string a;
        [FieldOptional]
        public string b;
        [FieldOptional]
        public string c;
    }
}