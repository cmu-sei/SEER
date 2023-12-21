// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

namespace Seer.Infrastructure.ViewModels
{
    public class SurveyOptionViewModel
    {

        public int OptionNumber { get; set; }

        public string Option { get; set; }

        public SurveyOptionViewModel(int optionNumber, string option)
        {
            this.OptionNumber = optionNumber;
            this.Option = option;
        }
    }
}