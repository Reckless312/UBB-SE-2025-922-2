namespace UnitTests.ReviewChecker.AuxiliaryTestsClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Moq;

    public class TestOffensiveContentDetector
    {
        public bool IsOffensive { get; set; }

        public string MockDetectOffensiveContent(string text)
        {
            if (this.IsOffensive)
            {
                return "[[{\"label\":\"hate\",\"score\":\"0.9\"}]]";
            }
            else
            {
                return "[[{\"label\":\"hate\",\"score\":\"0.05\"}]]";
            }
        }
    }
}
