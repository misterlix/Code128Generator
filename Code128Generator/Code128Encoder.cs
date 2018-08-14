using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Code128Generator
{
    class Code128Encoder
    {
        private const char StartCharA = 'Ë';
        private const char StartCharB = 'Ì';
        private const char StartcharC = 'Í';
        private const char SwitchA = 'É';
        private const char SwitchB = 'È';
        private const char SwitchC = 'Ç';
        private const char Stop = 'Î';


        enum CharacterSetNames { A, B, C }


        public string Encode(string input)
        {
            CharacterSetNames? currentSet = null;

            var encoded = "";

            for (var i = 0; i < input.Length; i += 1)
            {
                var currentChar = input[i]; // input character being examined
                var charSet = GetCharacterSet(input, i);

                // character set has changed (will also happen for first character in the input string)
                if (currentSet != charSet)
                {
                    var chr = currentSet == null 
                        ? GetStartChar(charSet) 
                        : GetSwitchChar(charSet);

                    encoded += chr;
                    currentSet = charSet;
                }

                if (currentSet == CharacterSetNames.C)
                {
                    var idx = i;
                    var consecutiveDigits = 0;
                    while (idx < input.Length && char.IsNumber(input[idx]))
                    {
                        consecutiveDigits += 1;
                        idx += 1;
                    }

                    var reps = consecutiveDigits / 2;
                    while (reps > 0)
                    {
                        var code128Val = Convert.ToInt32($"{input[i]}{input[i + 1]}");
                        var asciiVal = GetAsciiVal(code128Val);
                        currentChar = (char)asciiVal;
                        encoded += currentChar;

                        i += 2;
                        reps -= 1;
                    }

                    i -= 1;
                }
                else if (currentChar == '\t')
                {
                    encoded += 'i';
                }
                else
                {
                    encoded += currentChar;
                }
            }

            var checkSum = GetCheckSum(encoded);

            encoded += checkSum;
            encoded += Stop;

            return encoded;
        }


        private char GetCheckSum(string asciiValues)
        {
            var runningTot = GetCode128CharVal(asciiValues[0]);

            for (var i = 1; i < asciiValues.Length; i += 1)
            {
                var charVal = GetCode128CharVal(asciiValues[i]) * i;
                runningTot += charVal;
            }

            var div = runningTot / 103;
            var checkSum = runningTot - (div * 103);
            var checkSumAsciiVal = GetAsciiVal(checkSum);
            var csChar = (char)checkSumAsciiVal;

            return csChar;
        }


        private int GetCode128CharVal(int asciiVal)
        {
            var adjustment = asciiVal <= 126 ? 32 : 100;
            var charVal = asciiVal - adjustment;
            return charVal;
        }


        private int GetAsciiVal(int code128CharVal)
        {
            var adjustment = code128CharVal <= 94 ? 32 : 100;
            var asciiVal = code128CharVal + adjustment;
            return asciiVal;
        }



        private char GetStartChar(CharacterSetNames setName)
        {
            switch (setName)
            {
                case CharacterSetNames.A:
                    return StartCharA;
                case CharacterSetNames.B:
                    return StartCharB;
                default:
                    return StartcharC;
            }
        }

        private char GetSwitchChar(CharacterSetNames setName)
        {
            switch (setName)
            {
                case CharacterSetNames.A:
                    return SwitchA;
                case CharacterSetNames.B:
                    return SwitchB;
                default:
                    return SwitchC;
            }
        }


        private static CharacterSetNames GetCharacterSet(string data, int charIdx)
        {
            CharacterSetNames charSet;

            var refChar = data[charIdx]; // 'reference' character at the position being examined

            // Determine number of consecutive digits, including the reference character
            if (IsSetC(data, charIdx))
            {
                charSet = CharacterSetNames.C;
            }
            else if (char.IsControl(refChar))
            {
                charSet = CharacterSetNames.A;
            }
            else
            {
                charSet = CharacterSetNames.B;
            }

            return charSet;
        }

        private static bool IsSetC(string data, int charIdx)
        {
            var idx = charIdx;
            var consecutiveDigits = 0;
            while (idx < data.Length && char.IsNumber(data[idx]))
            {
                consecutiveDigits += 1;
                idx += 1;
            }

            var startOfData = charIdx == 0;
            var endOfData = data.Length == charIdx + consecutiveDigits;
            var middleOfData = !startOfData && !endOfData;

            // numbers make up the entire string
            if (consecutiveDigits == data.Length && (consecutiveDigits == 2 || consecutiveDigits >= 4))
            {
                return true;
            }

            // numbers are surrounded by set A or set B characters
            if (consecutiveDigits >= 6 && middleOfData)
            {
                return true;
            }

            // numbers are at the end of the string
            if (consecutiveDigits >= 4 && (startOfData || endOfData))
            {
                return true;
            }

            return false;
        }
    }
}
