using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordGame.API.Domain.Enums;

namespace WordGame.API.Models
{
    public class HintModel
    {
        public string HintWord { get; set; }

        public int WordCount { get; set; }
    }
}
