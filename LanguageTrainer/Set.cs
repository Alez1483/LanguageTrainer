using System;

namespace LanguageTrainer
{
    //set of word pairs in two languages
    public class Set
    {
        public Random random = new Random();
        public List<AnswerPair> AnswerPairs { get; set; }

        public string questionLanguage { get; set; }
        public string answerLanguage { get; set; }

        public void AddAnswerPair(AnswerPair pair)
        {
            AnswerPairs.Add(pair);
        }
        public void AddAnswerPair(string[] question, string[] answer)
        {
            AnswerPairs.Add(new AnswerPair(question, answer));
        }
        //constructors
        public Set(string qLanguage, string aLanguage)
        {
            AnswerPairs = new List<AnswerPair>(10);
            questionLanguage = qLanguage;
            answerLanguage = aLanguage;
        }
        public Set()
        {
            AnswerPairs = new List<AnswerPair>(10);
            questionLanguage = string.Empty;
            answerLanguage = string.Empty;
        }
        public Set(string qLanguage, string aLanguage, AnswerPair[] answerPairs)
        {
            AnswerPairs = new List<AnswerPair>(answerPairs);
            questionLanguage = qLanguage;
            answerLanguage = aLanguage;
        }
        //suffles pair order
        public void SufflePairs()
        {
            //fisher-yates
            for (int i = AnswerPairs.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = AnswerPairs[j];
                AnswerPairs[j] = AnswerPairs[i];
                AnswerPairs[i] = temp;
            }
        }
    }
    //pair of words with two languages
    public class AnswerPair
    {
        //first language words
        public string[] question {  get; set; }
        //seconds language wordsd
        public string[] answer { get; set; }

        public AnswerPair(string[] question, string[] answer)
        {
            this.question = question;
            this.answer = answer;
        }
        public AnswerPair()
        {
            question = Array.Empty<string>();
            answer = Array.Empty<string>();
        }
    }
}
