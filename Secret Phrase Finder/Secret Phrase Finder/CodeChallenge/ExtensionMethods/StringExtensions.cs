using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BackendDeveloperCodeChallenge.ExtensionMethods
{
    public static class StringExtensions
    {
        public static byte[] ComputeMD5Hash(this string input)
        {
            byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
            MD5 md5 = MD5.Create();

            return md5.ComputeHash(inputByteArray);
        }

        public static bool IsSubsetOf(this string word, string potentialSuperSetWord)
        {
            if (word.Length > potentialSuperSetWord.Length)
            {
                return false;
            }

            const char AlreadySeen = '#';

            char[] subsetArray = word.ToCharArray();
            char[] supersetArray = potentialSuperSetWord.ToCharArray();

            foreach (char character in subsetArray)
            {
                if (!supersetArray.Contains(character))
                {
                    return false;
                }

                int charIndexInSuperset = potentialSuperSetWord.IndexOf(character);
                supersetArray[charIndexInSuperset] = AlreadySeen;
            }

            return true;
        }
    }
}