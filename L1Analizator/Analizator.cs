using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace L1Analizator
{
    class Analizator
    {
        private string filename;
        private static string[][] atoms;

        public Analizator(string filename)
        {
            this.filename = filename;
        }

        public void init()
        {
            string[] sourceCode = System.IO.File.ReadAllLines(filename);

            atoms = new string[sourceCode.Length][];

            Regex identifierRegex = RegExes.getIdentifierRegex();


            for (int i = 0; i < sourceCode.Length; i++)
            {
                sourceCode[i] = sourceCode[i].
                    Replace("(", " ( ").
                    Replace(")", " ) ").
                    Replace("[", " [ ").
                    Replace("]", " ] ").
                    Replace("{", " { ").
                    Replace("}", " } ").
                    Replace("+", " + ").
                    Replace("-", " - ").
                    Replace("*", " * ").
                    Replace("/", " / ").
                    Replace("<=", " <= ").
                    Replace(">=", " >= ").
                    Replace("==", " == ").
                    Replace("!=", " != ").
                    Replace("::", " :: ").
                    Replace(">>", " >> ").
                    Replace("<<", " << ").
                    Replace("\r\n", "").
                    Replace("\n", "").
                    Replace("\t", " ").
                    Replace(";", " ; ");

                Regex r1 = new Regex(@"(?<!>)>(?!(>|=))"); // ce nu e precedat de > si nu e urmat de > sau =
                sourceCode[i] = r1.Replace(sourceCode[i], " > ");

                Regex r2 = new Regex(@"(?<!<)<(?!(<|=))");
                sourceCode[i] = r2.Replace(sourceCode[i], " < ");

                Regex r3 = new Regex(@"(?<!<)=(?!(<|=))");
                sourceCode[i] = r3.Replace(sourceCode[i], " = ");

                atoms[i] = sourceCode[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public void run()
        {
            bool includesDone = false;
            int i, j;

            for (i = 0; i < atoms.Length; i++)
            {
                if (atoms[i].Length <= 0)
                    continue;

                j = 0;

                if (!includesDone)
                {
                    if (atoms[i][j] != "#include")
                        includesDone = true;
                    else
                    {
                        try
                        {
                            checkIncludeLine(i);
                        }
                        catch (AnalizatorException ex)
                        {
                            printeazaEroare(ex);
                        }
                        continue;
                    }

                    // intram in programul principal :)
                    break;
                }
            }

            j = 0;
            try
            {
                checkMain(ref i, ref j);
                //break;
            }
            catch (AnalizatorException ex)
            {
                printeazaEroare(ex);
            }
        }

        private void printeazaEroare(AnalizatorException ex)
        {
            System.Console.WriteLine(String.Format("Eroare {0} la linia {1}, langa atomul '{2}': {3}", ex.code, ex.i + 1, atoms[ex.i][ex.j], ex.Message));
            System.Console.ReadKey();
            Environment.Exit(ex.code);
        }

        private void checkIncludeLine(int line)
        {
            if (atoms[line][1] != "<")
                throw new AnalizatorException("Sintaxa 'include' incorecta", line, 1, 1);
            if (!RegExes.getIdentifierRegex().IsMatch(atoms[line][2]))
                throw new AnalizatorException("Sintaxa 'include' incorecta", line, 2, 1);
            if (atoms[line][3] != ">")
                throw new AnalizatorException("Sintaxa 'include' incorecta", line, 3, 1);

            if (atoms[line].Length > 4)
                throw new AnalizatorException("Sintaxa 'include' incorecta", line, 4, 1);
        }

        private void checkMain(ref int line, ref int j)
        {
            if (atoms[line][j] != "int")
                throw new AnalizatorException("Sintaxa 'main' incorecta", line, j, 2);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] != "main")
                throw new AnalizatorException("Sintaxa 'main' incorecta", line, j, 2);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] != "(")
                throw new AnalizatorException("Sintaxa 'main' incorecta", line, j, 2);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] != ")")
                throw new AnalizatorException("Sintaxa 'main' incorecta", line, j, 2);

            getNextAtom(ref line, ref j, true);

            // check block statement
            checkBlockStatement(ref line, ref j);

            
        }

        private void checkBlockStatement(ref int line, ref int j)
        {
            if (atoms[line][j] != "{")
                throw new AnalizatorException("Sintaxa 'bloc de cod' incorecta", line, j, 3);

            getNextAtom(ref line, ref j, true);

            checkStatementList(ref line, ref j);


            // last }
            if (atoms[line][j] != "}")
                throw new AnalizatorException("Sintaxa 'bloc de cod' incorecta", line, j, 3);
        }

        private void checkStatementList(ref int line, ref int j)
        {
            //return;
            while (!atEnd(line,j) && atoms[line][j] != "}")
            {
                checkStatement(ref line, ref j);
            }
        }

        private void checkStatement(ref int line, ref int j)
        {
            if (atoms[line][j] == "std") // io statement 
            {
                checkIOStatement(ref line, ref j);
            }
            else if (atoms[line][j] == "int" || atoms[line][j] == "float") // declaration
            {
                checkDeclaration(ref line, ref j);
            }
            else if (atoms[line][j] == "if" || atoms[line][j] == "while") // if/while statement
            {
                ;
            }
            else if (RegExes.getIdentifierRegex().IsMatch(atoms[line][j])) // assigment statement;
            {
                ;
            }
            else // ceva neasteptat aici, eroare
            {
                throw new AnalizatorException("Sintaxa instructiune incorecta", line, j, 4);
            }
            getNextAtom(ref line, ref j, true);
        }

        private void checkIOStatement(ref int line, ref int j)
        {
            if (atoms[line][j] != "std")
                throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] != "::")
                throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] == "cin")
            {
                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != ">>")
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                getNextAtom(ref line, ref j, true);

                if (!RegExes.getIdentifierRegex().IsMatch(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] == ";")
                    return;
                else if (atoms[line][j] == "[")// it's array item
                {
                    getNextAtom(ref line, ref j, true);

                    if (!RegExes.isIndex(atoms[line][j]))
                        throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                    getNextAtom(ref line, ref j, true);

                    if (atoms[line][j] != "]")
                        throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                    getNextAtom(ref line, ref j, true);

                    if (atoms[line][j] != ";")
                        throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
                }
                else
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
            }
            else if (atoms[line][j] == "cout")
            {
                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != "<<")
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
            }
            else
                throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
        }

        private void checkDeclaration(ref int line, ref int j)
        {
            // redundant
            if (!(atoms[line][j] == "int" || atoms[line][j] == "float"))
            {
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
            }

            getNextAtom(ref line, ref j, true);

            if (!RegExes.getIdentifierRegex().IsMatch(atoms[line][j]))
            {
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
            }

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] == ";")
                return;
            else if (atoms[line][j] == "[") // it's array
            {
                getNextAtom(ref line, ref j, true);

                if (!RegExes.getArrayLengthRegex().IsMatch(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != "]")
                    throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != ";")
                    throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
            }
            else // error
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
        }

        private string getAtom(int offset, ref int i, ref int j, bool updateIJ)
        {
            string rez = null;

            int ii = i, jj = j;

            while (offset > 0)
            {
                if (atoms[ii].Length > jj + 1)
                {
                    jj++;
                    rez = atoms[ii][jj];
                    offset--;
                }
                else
                {
                    ii++;
                    jj = -1;
                }
            }

            if (updateIJ)
            {
                i = ii;
                j = jj;
            }

            return rez;
        }

        private string getNextAtom(ref int i, ref int j, bool updateIJ)
        {
            return getAtom(1, ref i, ref j, updateIJ);
        }

        private bool atEnd(int i, int j)
        {
            return i >= atoms.Length && j >= atoms[i].Length;
        }
    }
}
