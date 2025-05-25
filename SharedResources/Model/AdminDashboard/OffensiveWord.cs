namespace DataAccess.Model.AdminDashboard
{
    public class OffensiveWord
    {
        private static int nextId = 0;
        public int OffensiveWordId { get; set; }
        public string Word { get; set; }

        public OffensiveWord(string offensoveWord)
        {
            this.Word = offensoveWord;
            OffensiveWord.nextId = OffensiveWord.nextId + 1;
            this.OffensiveWordId = OffensiveWord.nextId;
        }
        public OffensiveWord()
        {
            this.Word = string.Empty;
        }
    }
}
