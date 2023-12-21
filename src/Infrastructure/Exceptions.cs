// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;

namespace Seer.Infrastructure
{
    public class Exceptions
    {
        public class HiveFormattingException : Exception
        {
            public HiveFormattingException() { }

            public HiveFormattingException(string message) : base(message) { }
        }

        public class UserCreationException : Exception
        {
            public UserCreationException() { }

            public UserCreationException(string message) : base(message) { }
        }
    }
}