using System;
using System.IO;
using System.Text.Json;
using System.Text;

namespace LanguageTrainer
{
    internal class Program
    {
        public static JsonSerializerOptions? jsonOptions;
        public static bool wantToClearConsole = true;

        static void Main(string[] args)
        {
            //make sure correct and wrong symbols looks correct in console
            Console.OutputEncoding = Encoding.Unicode;
            jsonOptions = new JsonSerializerOptions { IncludeFields = true };

            //main loop
            while (true)
            {
                if (wantToClearConsole)
                {
                    Console.Clear();
                }
                wantToClearConsole = true;

                if (Directory.GetFiles("Sets").Length == 0)
                {
                    //if there's no sets, jump to creating one
                    Console.WriteLine("Youd haven't made a word set yet so let's start by making one");
                    CreateNewSet();
                    continue;
                }
                Console.WriteLine("Type what you want to do: c -> create new set of words, t -> train, r -> remove set permanently, p -> print set");
                char answer = Console.ReadKey().KeyChar;
                answer = char.ToLower(answer);
                Console.Clear();
                if (answer == 'c')
                {
                    CreateNewSet();
                }
                else if (answer == 't')
                {
                    LearnSet();
                }
                else if (answer == 'r')
                {
                    RemoveSet();
                }
                else if (answer == 'p')
                {
                    PrintSet();
                }
                else
                {
                    Console.WriteLine("You gave neither c, t nor r, try again");
                    wantToClearConsole = false;
                }
            }
        }

        //function for creating new set
        //questions and languages are asked from the user
        //-__- ends function execution
        public static void CreateNewSet()
        {
            if (!Directory.Exists("Sets"))
            {
                Directory.CreateDirectory("Sets");
            }

            //asks unique name for the set and the languages to be used in the set
            //0 = name of the set, 1 and 2 are the names for the languages used in the set
            string[]? names = AskForSetName();

            //return to menu
            if (names == null)
            {
                return;
            }

            string setPath = Path.Combine("Sets", names[0]);

            Set set = new Set(names[1], names[2]);

            //instructions
            Console.WriteLine("\nNext we want you to give as many word pairs as you want, first " + set.questionLanguage + " words and then " + set.answerLanguage + " words, both are asked separately");
            Console.WriteLine("Every upper case letter will be changed to lower case letter");
            Console.WriteLine("When you are happy with your set, type -__- and press enter when " + set.questionLanguage + " word is asked");
            Console.WriteLine("If you want to give many translations to same word, separate them with comma, when you are done, press enter \nHere's an example of how it will look like:\n");

            //example
            Console.WriteLine("Give english word: sphere, ball");
            Console.WriteLine("Give swedish word: boll\n");

            Console.WriteLine("Give english word: spear, javelin throw");
            Console.WriteLine("Give swedish word: spjut\n");

            Console.WriteLine("Give english word: -__-\n");

            //asks for words
            while (true)
            {
                string[]? question = AskForStrings("Give " + set.questionLanguage + " word: ", true);
                if (question == null)
                {
                    //save the pairs to file and exit to menu
                    if (set.AnswerPairs.Count > 0)
                    {
                        string json = JsonSerializer.Serialize(set, jsonOptions);
                        File.WriteAllText(setPath, json);
                        return;
                    }
                    Console.Clear();
                    wantToClearConsole = false;
                    Console.WriteLine("The set wasn't saved because there was no word pairs yet");
                    return;
                }
                string[]? answer = AskForStrings("Give " + set.answerLanguage + " word: ", false);
                if (answer == null)
                {
                    //should never be null
                    return;
                }
                set.AddAnswerPair(question, answer);
                Console.WriteLine();
            }
        }

        //asks strings separated by comma
        //makes sure the given input is correct
        //promt = text printed to user, checkForSmiley = whether check for ending word -__-
        public static string[]? AskForStrings(string promt, bool checkForSmiley)
        {
            //output list of separated strings
            string[] strings;
            while (true)
            {
                Console.Write(promt);
                string? answer = Console.ReadLine();
                //check for empty
                if (answer == null || answer.Length == 0)
                {
                    Console.WriteLine("You didn't give any valid word, try again\n");
                    continue;
                }
                //ending word
                if (checkForSmiley && answer == "-__-")
                {
                    Console.WriteLine();
                    return null;
                }
                //separates by comma
                strings = answer.Split(',');

                bool emptyFound = false;
                //trims and converts individual strings to lowercase, makes sure there's no empty words
                for (int i = 0; i < strings.Length; i++)
                {
                    string trimmed = strings[i].Trim().ToLower();
                    if (trimmed.Length == 0)
                    {
                        emptyFound = true;
                        break;
                    }
                    strings[i] = trimmed;
                }
                if (emptyFound)
                {
                    Console.WriteLine("Some of the words were not given correctly, try again");
                    continue;
                }
                return strings;
            }
        }

        //ask for name for the new set, also the languages used for the set
        //makes sure the name is valid file name and does't exist yet
        //null is returned when want to exit to menu
        public static string[]? AskForSetName()
        {
            //0 = set name, 1 = first language, 2 = second language
            string[] strings;
            while (true)
            {
                Console.WriteLine("Give a name for the set you want to create, put the languages at the end separated by comma \nFor example: Text1Grammary, english, swedish");
                Console.WriteLine("Type 3xit if you want to exit to menu");
                string? answer = Console.ReadLine();
                //is null only if Ctrl + C
                if (answer == null)
                {
                    Console.WriteLine("Something went wrong, try again, make sure the input is correct");
                    continue;
                }

                //exit to menu
                if (answer.Trim().ToLower() == "3xit")
                {
                    return null;
                }

                strings = answer.Split(',');
                //makes sure correct amount of arguments were given
                if (strings.Length != 3)
                {
                    Console.WriteLine("You gave " + strings.Length + " parameters, make sure to give exactly 3 of them, try again");
                    continue;
                }

                bool foundEmpty = false;
                //trims the individual strings, makes sure none is empty after that
                for (int i = 0; i < 3; i++)
                {
                    string trimmed = strings[i].Trim();
                    if (trimmed.Length == 0)
                    {
                        foundEmpty = true;
                        break;
                    }
                    strings[i] = trimmed;
                }
                if (foundEmpty)
                {
                    Console.WriteLine("You gave incorrect parameters, make sure to give them as in the example, try again");
                    continue;
                }

                //checks validity of the name
                if (!IsValidName(strings[0]))
                {
                    Console.WriteLine("You didn't give valid set name, you can only use latin letters and numbers starting with letter, try again");
                    continue;
                }
                //makes sure set of given name doesn't exist
                if (File.Exists(Path.Combine("Sets", strings[0])))
                {
                    Console.WriteLine("Set with the given name already exists, try again");
                    continue;
                }
                return strings;
            }
        }

        //makes sure name is valid as a file name
        //name must start with latin letter and only latin letters or numbers after that
        //empty names are not allowed
        public static bool IsValidName(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!IsLetter(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!IsValidCharacter(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        //checks whether character is either letter or number
        public static bool IsValidCharacter(char c)
        {
            if (IsLetter(c))
            {
                return true;
            }
            if (c >= '0' && c <= '9')
            {
                return true;
            }
            return false;
        }

        //checks whether char is letter
        public static bool IsLetter(char c)
        {
            if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z'))
            {
                return true;
            }
            return false;
        }

        //functions training a set
        //set is asked from the user
        //asks which way the languages are asked, also whether the questions should be suffled
        public static void LearnSet()
        {
            bool firstRun = true;
            Set? set = null;
            bool isFirstLanguage = true;
            bool wantToSuffle = false;
            string setPath = "";

            while (true)
            {
                if (firstRun)
                {
                    //asks set
                    set = AskSet("Give the name of the set you'd like to learn", out string? path);
                    if (path != null)
                    {
                        setPath = path;
                    }
                    //user wants to exit to menu
                    if (set == null)
                    {
                        return;
                    }

                    //asks which way the words/languages are
                    isFirstLanguage = AskLanguage(set);
                    Console.WriteLine();
                    //asks and suffles the set when needed
                    wantToSuffle = AskForSuffle(set);
                }
                else
                {
                    if (wantToSuffle)
                    {
                        set?.SufflePairs();
                    }
                }
                //check just to get rid of error, should never be null
                if (set == null) return;

                if (isFirstLanguage)
                {
                    Console.WriteLine("\nNext you will get " + set.questionLanguage + " word and you have to respond in " + set.answerLanguage);
                }
                else
                {
                    Console.WriteLine("\nNext you will get " + set.answerLanguage + " word and you have to respond in " + set.questionLanguage);
                }
                Console.WriteLine("If there are multiple answers, you can respond with either");

                int correctCount = 0;
                var sb = new StringBuilder(100);

                HashSet<int> removeIndexes = new HashSet<int>(set.AnswerPairs.Count);

                //ask all the word pairs
                for (int i = 0; i < set.AnswerPairs.Count; i++)
                {
                    var ansPair = set.AnswerPairs[i];

                    string[] question, correctAnswer;
                    if (isFirstLanguage)
                    {
                        question = ansPair.question;
                        correctAnswer = ansPair.answer;
                    }
                    else
                    {
                        question = ansPair.answer;
                        correctAnswer = ansPair.question;
                    }
                    //prints the string array comma in between strings
                    WriteStringArray(question, sb);
                    Console.Write(": ");

                    string? answer = Console.ReadLine();

                    //checks the answer, prints whether it's right or not
                    bool answerCorrect = CheckAnswer(answer, correctAnswer, out bool wantToRemove);
                    if (answerCorrect)
                    {
                        //should the pair index be added to the list of pairs to be removed
                        if (wantToRemove)
                        {
                            removeIndexes.Add(i);
                            Console.WriteLine(" This pair will be removed from the set");
                        }
                        correctCount++;
                    }
                }
                Console.WriteLine("You got " + correctCount + "/" + set.AnswerPairs.Count + " right");

                //there are pairs to be removed
                if (removeIndexes.Count != 0)
                {
                    bool wantToRemove = false;
                    while (true)
                    {
                        Console.Write("are you sure you want to remove the given pairs from the set? y for yes, n for no");
                        char answer = Console.ReadKey().KeyChar;
                        answer = char.ToLower(answer);
                        if (answer == 'y')
                        {
                            wantToRemove = true;
                            break;
                        }
                        if (answer == 'n')
                        {
                            break;
                        }
                        Console.WriteLine("\nYou typed neither y nor n, try again");
                    }
                    //user wants to remove the selected pairs
                    if (wantToRemove)
                    {
                        //path to the file without _original suffix
                        string nonOriginalPath = "";
                        if (setPath.Length - (setPath.LastIndexOf(Path.DirectorySeparatorChar) + 1) > 9)
                        {
                            nonOriginalPath = setPath.Substring(0, setPath.Length - 9);
                        }
                        
                        //does this file have _original suffix
                        bool isOriginal = HasSuffix(setPath, "_original");
                        if (isOriginal && File.Exists(nonOriginalPath))
                        {
                            Console.WriteLine("\nYou have already created a subset of this set");
                        }
                        else
                        {
                            //if this is _original file and non original doesn't exist, we should make new non original file
                            if (isOriginal)
                            {
                                setPath = nonOriginalPath;
                            }

                            //amount of elements left after removal of selected pairs
                            int newSetElementCount = set.AnswerPairs.Count - removeIndexes.Count;
                            //set of pairs being used for the new set (where the selected pairs are removed)
                            AnswerPair[] newPairs = new AnswerPair[newSetElementCount];
                            //add all pairs not meant to remove to the array
                            for (int i = 0, newIndex = 0; i < set.AnswerPairs.Count; i++)
                            {
                                if (!removeIndexes.Contains(i))
                                {
                                    newPairs[newIndex] = set.AnswerPairs[i];
                                    newIndex++;
                                }
                            }
                            //create new Set with selected pairs gone
                            set = new Set(set.questionLanguage, set.answerLanguage, newPairs);

                            //if original doesn't exist yet, suffix this set with _original and overwrite with the new set
                            if (!File.Exists(setPath + "_original"))
                            {
                                //rename with _original suffix
                                File.Move(setPath, setPath + "_original");
                            }
                            string json = JsonSerializer.Serialize(set, jsonOptions);
                            File.WriteAllText(setPath, json);
                        }
                    }
                }
                while (true)
                {
                    Console.Write("\nDo you want to learn this same set again with same settings? y for yes, n for no");
                    char answer = Console.ReadKey().KeyChar;
                    answer = char.ToLower(answer);
                    if (answer == 'y')
                    {
                        firstRun = false;
                        break;
                    }
                    if (answer == 'n')
                    {
                        return;
                    }
                    Console.WriteLine("\nYou typed neither y nor n, try again");
                }
            }
        }

        //checks whether answer is right or not
        //if not, the correct answer is given
        //if only partially right, gives the other right answers
        //answerString = answer to be tested, can include many anwers comma in between
        //correctAnswer the correct words
        //wantsToRemove is true when the user inputs --> at the end meaning the word should be removed
        public static bool CheckAnswer(string? answerString, string[] correctAnswer, out bool wantsToRemove)
        {
            wantsToRemove = false;

            var sb = new StringBuilder(100);
            //only null when Ctrl + C is pressed
            if (answerString == null)
            {
                //tells the user the answer was not right and the correct answer
                PrintIncorrect(correctAnswer, sb);
                return false;
            }

            string[] answers = answerString.Split(',');

            //trims individual strings and makes them lowercase
            for (int i = 0; i < answers.Length; i++)
            {
                answers[i] = answers[i].Trim().ToLower();
            }

            //if no answer is given, it's concidered wrong answer
            if (answers.Length == 1 && answers[0].Length == 0)
            {
                PrintIncorrect(correctAnswer, sb);
                return false;
            }

            //detects whether this pair should be removed based on --> at the end
            int lastIndex = answers.Length - 1;
            if (HasSuffix(answers[lastIndex], "-->"))
            {
                answers[lastIndex] = answers[lastIndex].Substring(0, answers[lastIndex].Length - 3).Trim();
                wantsToRemove = true;
            }

            //set of answers that the user didn't give so they can be shown in the end
            HashSet<string> answersNotFound = new HashSet<string>(correctAnswer);

            //checks all the given words against all the correct answers
            for (int i = 0; i < answers.Length; i++)
            {
                bool correctFound = false;
                for (int j = 0; j < correctAnswer.Length; j++)
                {
                    if (answers[i] == correctAnswer[j])
                    {
                        answersNotFound.Remove(answers[i]);
                        correctFound = true;
                        break;
                    }
                }
                if (!correctFound)
                {
                    PrintIncorrect(correctAnswer, sb);
                    return false;
                }
            }
            //every answer found
            if (answersNotFound.Count == 0)
            {
                Console.WriteLine("✅ correct");
            }
            else
            {
                Console.Write("✅ correct, ");
                WriteStringArray(answersNotFound.ToArray(), sb);
                if (answersNotFound.Count == 1)
                {
                    Console.WriteLine(" is correct too");
                }
                else
                {
                    Console.WriteLine(" are correct too");
                }
            }
            return true;
        }

        //prints the answer was incorrect and the correct answer
        public static void PrintIncorrect(string[] correctAnswer, StringBuilder sb)
        {
            Console.Write("❌ incorrect, the answer was: ");
            WriteStringArray(correctAnswer, sb);
            Console.WriteLine();
        }

        //prints array of strings separated by comma
        public static int WriteStringArray(string[] strings)
        {
            var sb = new StringBuilder(strings[0], 100);
            for (int i = 1; i < strings.Length; i++)
            {
                sb.Append(", ");
                sb.Append(strings[i]);
            }
            Console.Write(sb);
            return sb.Length;
        }

        //declaration with predeclared stringbuilder
        public static int WriteStringArray(string[] strings, StringBuilder sb)
        {
            sb.Clear();
            sb.Append(strings[0]);
            for (int i = 1; i < strings.Length; i++)
            {
                sb.Append(", ");
                sb.Append(strings[i]);
            }
            Console.Write(sb);
            return sb.Length;
        }

        //asks whether the words should be suffled, if so the suffling is done
        public static bool AskForSuffle(Set set)
        {
            while (true)
            {
                Console.Write("Do you want to suffle the words? type y for yes, n for no: ");
                char answer = Console.ReadKey().KeyChar;
                answer = char.ToLower(answer);
                if (answer == 'y')
                {
                    set.SufflePairs();
                    return true;
                }
                else if (answer == 'n')
                {
                    return false;
                }
                Console.WriteLine("\nYou typed neither y nor n, try again");
            }
        }

        //ask which way the words/languages should be asked
        public static bool AskLanguage(Set set)
        {
            while (true)
            {
                Console.Write("Type 1 if you want to type " + set.answerLanguage + " words yourself, 2 if " + set.questionLanguage + " words: ");
                char answer = Console.ReadKey().KeyChar;
                if (answer == '1')
                {
                    return true;
                }
                if (answer == '2')
                {
                    return false;
                }
                Console.WriteLine("\nYou gave neither 1 nor 2, try again");
            }
        }

        //menu for choosing a set
        //null if wants to exit with 3xit
        //reads the set from memory and returns that
        public static Set? AskSet(string promt, out string? path)
        {
            while (true)
            {
                path = AskSetPath(promt);
                //wants to exit
                if (path == null)
                {
                    return null;
                }
                //read set from memory
                string json = File.ReadAllText(path);
                Set? set = JsonSerializer.Deserialize<Set>(json, jsonOptions);
                if (set == null)
                {
                    Console.WriteLine("Something went wrong on reading the file. Try again or try different set");
                    continue;
                }
                return set;
            }
        }

        //menu for removing sets, ask for set and makes sure user really want to delete it
        //3xit exits from the function
        public static void RemoveSet()
        {
            //keeps asking if don't actually want to delete
            while (true)
            {
                string? path = AskSetPath("Give the name of the set you'd like to remove");
                //returns to menu
                if (path == null)
                {
                    return;
                }
                //makes sure user really wants to delete
                if (ConfirmWantToDelete(path.Substring(5)))
                {
                    File.Delete(path);
                    //if no files are left, exit to menu
                    if (Directory.GetFiles("Sets").Length == 0)
                    {
                        return;
                    }
                }
            }
        }

        //asks set from user
        //prints the whole set like a language dictionary
        //3xit exits to menu
        public static void PrintSet()
        {
            while (true)
            {
                Set? set = AskSet("Give the name of the set you'd like to print", out string? path);
                //returns to menu
                if (set == null)
                {
                    return;
                }

                Console.WriteLine();

                const int amountOfSpacesBetween = 10;
                int questionWidth = FindLongestQuestion(set);

                for (int i = 0; i < set.AnswerPairs.Count; i++)
                {
                    var pair = set.AnswerPairs[i];
                    int questionLen = WriteStringArray(pair.question);
                    Console.Write(new string(' ', questionWidth - questionLen + amountOfSpacesBetween));
                    WriteStringArray(pair.answer);
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
        //finds the longest question (first language) so the print can be formatted correctly
        public static int FindLongestQuestion(Set set)
        {
            int longest = -1;
            foreach (var pair in set.AnswerPairs)
            {
                //there's no ", " in front of the first word
                int length = -2;
                foreach (string question in pair.question)
                {
                    length += question.Length + 2;
                }
                if (length > longest)
                {
                    longest = length;
                }
            }
            return longest;
        }

        //asks whether file with given name is really intendet to be deleted
        public static bool ConfirmWantToDelete(string fileNameToDelete)
        {
            while (true)
            {
                Console.Write("Do you really want to permanently destroy a set by name " + fileNameToDelete + "? Type n for no, y for yes");
                char answer = Console.ReadKey().KeyChar;
                answer = char.ToLower(answer);
                if (answer == 'n')
                {
                    return false;
                }
                else if (answer == 'y')
                {
                    return true;
                }
                Console.WriteLine("\nYou typed neither n nor y, try again");
            }
        }

        //asks a set to be chosen, accepts the index of the set
        //prints the list when asked
        //promt = text that's printed at the beginning
        //makes sure the file exists
        //returns null when user want to exit to main menu
        public static string? AskSetPath(string promt)
        {
            while (true)
            {
                Console.WriteLine(promt + ", type ? if you want to see list of all sets");
                Console.WriteLine("You can also use the index from the list to access set. Type 3xit if you want to exit to menu");
                string? answer = Console.ReadLine();
                //should be null only when Ctrl + C is pressed
                if (answer != null)
                {
                    answer = answer.Trim();
                }
                //no name given
                if (answer == null || answer.Length == 0)
                {
                    Console.WriteLine("Set of given name doesn't exist, try again");
                    continue;
                }

                //user wants to go back to menu
                if (answer.ToLower() == "3xit")
                {
                    return null;
                }

                //get all the set paths
                string[] setFiles = Directory.GetFiles("Sets");

                //prints list of sets and indexes
                //indexing starts from 1 in the interface
                if (answer.Length == 1 && answer[0] == '?')
                {
                    Console.WriteLine("Here are the sets listed:");

                    for (int i = 0; i < setFiles.Length; i++)
                    {
                        string file = setFiles[i];
                        Console.WriteLine((i + 1) + ". " + file.Substring(5));
                    }
                    continue;
                }

                //path to the chosen set
                string path = Path.Combine("Sets", answer);

                //if number is given, check whether it's valid as a index
                if (int.TryParse(answer, out int value))
                {
                    if (value < 1 || value > setFiles.Length)
                    {
                        Console.WriteLine("You gave incorrect index, make sure the number exists in the list");
                        continue;
                    }
                    return setFiles[value - 1];
                }
                //checks existense of file with given name
                if (File.Exists(path))
                {
                    return path;
                }
                //file doesn't exist
                Console.WriteLine("Set on the given name couldn't be found, make sure you type the name correctly");
                continue;
            }
        }

        //returns true when str has suffix at the end of it, false otherwise
        public static bool HasSuffix(string str, string suffix)
        {
            int index = str.IndexOf(suffix);
            if (index == -1)
            {
                return false;
            }
            if (index == str.Length - suffix.Length)
            {
                return true;
            }
            return false;
        }
    }
}