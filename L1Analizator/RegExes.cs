using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace L1Analizator
{
    public class RegExes : System.Object
    {
        public static string[] KeywordsArray = { "while", "if", "else", "std", "cin", "cout", "int", "float" };
        public static string[] OperatorsArray = { "+", "-", "*", "/" };
        public static string[] RelationsArray = { "<", "<=", "==", "!=", ">=", ">" };
        private static Regex constantRegex;
        private static Regex identifierRegex;

        public static Regex getConstantRegex()
        {
            if (constantRegex == null)
                constantRegex = new Regex(@"^(0|(\+|\-)?[1-9]\d*)(\.\d*[1-9])?$");
            return constantRegex;
        }

        public static Regex getIdentifierRegex()
        {
            if (identifierRegex == null)
                identifierRegex = new Regex(@"^([_a-zA-Z])(\w){0,249}$"); // maxim 250 de caractere
            return identifierRegex;
        }

        public static bool isIdentifier(string atom)
        {
            return getIdentifierRegex().IsMatch(atom) && !KeywordsArray.Contains(atom);
        }

        public static bool isRelation(string atom)
        {
            return RelationsArray.Contains(atom);
        }
    }
}
