using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DicewareGenerator
{
    class DicewareGenerator
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        // The file containing the Diceware wordlist
        // http://world.std.com/~reinhold/diceware.wordlist.asc
        private const string fileName = "diceware.wordlist";

        private static List<string> generatedKeys = new List<string>();
        private static Dictionary<string, string> dicewareWordlist = new Dictionary<string, string>();

        /* A five word passphrase provides a level of security much higher than the
        simple passwords most people use. A minimum of six words is recommended for use
        with GPG, wireless security and file encryption programs. A seven, eight or
        nine word passphrase is recommended for high value uses such as whole disk
        encryption, BitCoin, and the like. */
        private const byte numberOfWords = 5;

        public static void Main()
        {
            GetDicewareWordlist();
            GenerateKeys(numberOfWords);
            DisplayPassphrase();
        }

        private static void DisplayPassphrase()
        {
            var message = new StringBuilder();

            message.Append("Your Passphrase is: ");

            foreach (string key in generatedKeys)
            {
                message.Append(dicewareWordlist[key] + " ");
            }

            Console.WriteLine(message);
        }

        private static void GetDicewareWordlist()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDirectory, "../../../" + fileName);
            string fileContent = "";

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found: {0}", fileName);
                Environment.Exit(1);
            }
            else
                fileContent = File.ReadAllText(path);

            string[] lines = fileContent.Split("\n");
            foreach (string line in lines)
            {
                AddWordToDictionnary(line);
            }
        }

        private static void AddWordToDictionnary(string line)
        {
            string[] keyAndWord = line.Split();
            if (keyAndWord.Length == 2)
            {
                string key = keyAndWord[0];
                string word = keyAndWord[1];

                var isAValidKey = Regex.Match(key, "[1-6]{5}");
                var isAValidWord = Regex.Match(word, "\\S{1,6}");

                if (isAValidKey.Success && isAValidWord.Success)
                    dicewareWordlist[key] = word;
            }
        }

        private static void GenerateKeys(int numberOfKeys)
        {
            for (int i = 0; i < numberOfKeys; i++)
            {
                string key = GenerateAKey();
                generatedKeys.Add(key);
            }
            rngCsp.Dispose();
        }

        private static string GenerateAKey()
        {
            const int keySize = 5;
            string key = "";

            for (int i = 0; i < keySize; i++)
            {
                byte rolledNum = RollDice();
                key += rolledNum.ToString();
            }

            return key;
        }

        private static byte RollDice()
        {
            const byte numberOfSides = 6;

            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];
            do
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberOfSides));
            // Return the random number mod the number
            // of sides.  The possible values are zero-
            // based, so we add one.
            return (byte)((randomNumber[0] % numberOfSides) + 1);
        }

        private static bool IsFairRoll(byte roll, byte numSides)
        {
            // There are MaxValue / numSides full sets of numbers that can come up
            // in a single byte.  For instance, if we have a 6 sided die, there are
            // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
            int fullSetsOfValues = byte.MaxValue / numSides;


            // If the roll is within this range of fair values, then we let it continue.
            // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
            // < rather than <= since the = portion allows through an extra 0 value).
            // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
            // to use.
            return roll < numSides * fullSetsOfValues;
        }
    }
}
