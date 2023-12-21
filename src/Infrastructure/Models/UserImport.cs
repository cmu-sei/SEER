// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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