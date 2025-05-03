using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Model.AutoChecker
{
    public class OffensiveWord
    {
        private static int nextId = 0;
        public int OffensiveWordId { get; set; }
        public string Word { get; set; }

        public OffensiveWord(string offensoveWord) {
            Word = offensoveWord; 
            nextId = nextId + 1;
            OffensiveWordId = nextId;
        }
        public OffensiveWord() { }

    }
}
