using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace L1Analizator
{
    public class RegExes
    {
        private static Regex identifierRegex, constantRegex, numericalIndexRegex, arrayLengthRegex;
        private static string[] RelationsArray = { "<" , "<=" , "==" , "!=" , ">=" , ">" };

        public static Regex getIdentifierRegex()
        {
            if (identifierRegex == null)
                identifierRegex = new Regex(@"^([_a-zA-Z])(\w){0,249}$"); // maxim 250 de caractere
            return identifierRegex;
        }

        public static Regex getConstantRegex()
        {
            if (constantRegex == null)
                constantRegex = new Regex(@"^(0|(\+|\-)?[1-9]\d*)(\.\d*[1-9])?$");
            return constantRegex;
        }

        public static Regex getNumericalIndexRegex()
        {
            if (numericalIndexRegex == null)
                numericalIndexRegex = new Regex(@"^(0|[1-9]\d*)$");
            return numericalIndexRegex;
        }

        public static Regex getArrayLengthRegex()
        {
            if (arrayLengthRegex == null)
                arrayLengthRegex = new Regex(@"^[1-9]\d*$");
            return arrayLengthRegex;
        }

        public static bool isIndex(string atom)
        {
            return getNumericalIndexRegex().IsMatch(atom) || getIdentifierRegex().IsMatch(atom);
        }

        public static bool isRelation(string atom)
        {
            return RelationsArray.Contains(atom);
        }
    }
}
