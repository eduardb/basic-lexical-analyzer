using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace L1Analizator
{
    class Analizator
    {
        private string filename;
        private string[][] atoms;

        private Hashtable tsi, tsc, coduri;
        private List<DictionaryEntry> fip;

        private int tsi_index, tsc_index;

        public Analizator(string filename)
        {
            this.filename = filename;
        }

        public void init()
        {
            string[] sourceCode = File.ReadAllLines(filename);

            atoms = new string[sourceCode.Length][];

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

                Regex r3 = new Regex(@"(?<!(<|!|=))=(?!(<|=))");
                sourceCode[i] = r3.Replace(sourceCode[i], " = ");

                atoms[i] = sourceCode[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

#if DEBUG
                System.Console.WriteLine(string.Join(" ", atoms[i]));
#endif
            }

            coduri = new Hashtable();

            string[] codes = File.ReadAllLines("coduri.txt");

            foreach (string code in codes)
            {
                string[] t = code.Split(' ');
                coduri.Add(t[0], Int32.Parse(t[1]));
            }

            tsc = new Hashtable(); tsc_index = 1;
            tsi = new Hashtable(); tsi_index = 1;
            fip = new List<DictionaryEntry>();
        }

        public void genereaza_tabele()
        {
            int i = 0, j = 0;

            while (i < atoms.Length && (i < atoms.Length - 1 || j < atoms[atoms.Length - 1].Length - 1))
            {
                if (atoms[i].Length <= 0)
                {
                    i++;
                    continue;
                }

                string atom = atoms[i][j];

                if (atom != "identificator" && atom != "constanta" && coduri.ContainsKey(atom))
                {
                    fip.Add(new DictionaryEntry(coduri[atom], 0));
                }
                else
                {
                    if (RegExes.isIdentifier(atom))
                    {
                        int index;
                        if (tsi.ContainsKey(atom))
                            index = (int)tsi[atom];
                        else
                        {
                            index = tsi_index++;
                            tsi.Add(atom, index);
                        }
                        fip.Add(new DictionaryEntry((int)coduri["identificator"], index));
                    }
                    else
                    {
                        if (RegExes.getConstantRegex().IsMatch(atom))
                        {
                            int index;
                            if (tsc.ContainsKey(atom))
                                index = (int)tsc[atom];
                            else
                            {
                                index = tsc_index++;
                                tsc.Add(atom, index);
                            }
                            fip.Add(new DictionaryEntry((int)coduri["constanta"], index));
                        }
                        else
                        {
                            throw new AnalizatorException("Eroare la generare tabele", i, j, 99);
                        }
                    }
                }

                getNextAtom(ref i, ref j, true);
            }

            System.Console.Write("Tabele generate ");

            StreamWriter sw1 = new StreamWriter("TSI.txt");

            foreach (DictionaryEntry entry in tsi)
            {
                sw1.WriteLine(String.Format("{0} {1}", entry.Key, entry.Value));
            }
            sw1.Close();


            StreamWriter sw2 = new StreamWriter("TSC.txt");

            foreach (DictionaryEntry entry in tsc)
            {
                sw2.WriteLine(String.Format("{0} {1}", entry.Key, entry.Value));
            }
            sw2.Close();


            StreamWriter sw3 = new StreamWriter("FIP.txt");
            
            foreach (DictionaryEntry entry in fip)
            {
                sw3.WriteLine(String.Format("{0} {1}", entry.Key, entry.Value));
            }
            sw3.Close();

            System.Console.WriteLine("si scrise in fisiere");
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

                System.Console.WriteLine("Nu exista erori in codul sursa");

                genereaza_tabele();
            }
            catch (AnalizatorException ex)
            {
                printeazaEroare(ex);
            }
        }

        private void printeazaEroare(AnalizatorException ex)
        {
            System.Console.WriteLine(String.Format("Eroare {0} la linia {1}, langa atomul '{2}': {3}", ex.code, ex.i + 1, atoms[ex.i][ex.j], ex.Message));
#if DEBUG
            System.Console.ReadKey();
#endif
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

            try
            {
                getNextAtom(ref line, ref j, true);
                throw new AnalizatorException("Exista instructiuni dupa inchiderea functiei main!", line, j, 14);
            }
            catch (AnalizatorException ex)
            {
                if (ex.code != 13)
                    throw ex;
            }
            
           // if (!atEnd(line, j))
                

            
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

            //getNextAtom(ref line, ref j, true);
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
                checkStructStatement(ref line, ref j);
            }
            else if (RegExes.isIdentifier(atoms[line][j])) // assigment statement;
            {
                checkAssigmentStatement(ref line, ref j);
            }
            else // ceva neasteptat aici, eroare
            {
                throw new AnalizatorException("Sintaxa instructiune incorecta", line, j, 4);
            }
            
            //if (atEnd(line, j))
            //    return;

           // getNextAtom(ref line, ref j, true);
        }

        private void checkAssigmentStatement(ref int line, ref int j)
        {
            if (!RegExes.isIdentifier(atoms[line][j]))
                throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] == "[")// it's array item
            {
                getNextAtom(ref line, ref j, true);

                if (!RegExes.isIndex(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != "]")
                    throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                getNextAtom(ref line, ref j, true);
            }

            if (atoms[line][j] != "=")
                throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

            getNextAtom(ref line, ref j, true);

            checkExpression(ref line, ref j);

            if (atoms[line][j] != ";")
                throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

            getNextAtom(ref line, ref j, true);
        }

        private void checkStructStatement(ref int line, ref int j)
        {
            string type = atoms[line][j];

            if (atoms[line][j] != "if" && atoms[line][j] != "while")
                throw new AnalizatorException("Sintaxa instructiune compusa incorecta", line, j, 7);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] != "(")
                throw new AnalizatorException("Sintaxa instructiune compusa incorecta", line, j, 7);

            checkCondition(ref line, ref j);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] == "{")
            {
                checkBlockStatement(ref line, ref j);
                getNextAtom(ref line, ref j, true);
            }
            else
                checkStatement(ref line, ref j);

            if (type == "if" && atoms[line][j] == "else")
            {
                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] == "{")
                {
                    checkBlockStatement(ref line, ref j);
                    getNextAtom(ref line, ref j, true);
                }
                else
                    checkStatement(ref line, ref j);
            }
        }

        private void checkCondition(ref int line, ref int j)
        {
            if (atoms[line][j] != "(")
                throw new AnalizatorException("Sintaxa conditie incorecta", line, j, 8);

            getNextAtom(ref line, ref j, true);

            checkExpression(ref line, ref j);

            if (!RegExes.RelationsArray.Contains(atoms[line][j]))
                throw new AnalizatorException("Sintaxa conditie incorecta", line, j, 8);

            getNextAtom(ref line, ref j, true);

            checkExpression(ref line, ref j);

            if (atoms[line][j] != ")")
                throw new AnalizatorException("Sintaxa conditie incorecta", line, j, 8);
        }

        private void checkExpression(ref int line, ref int j)
        {
            if (!RegExes.isIdentifier(atoms[line][j]) && !RegExes.getConstantRegex().IsMatch(atoms[line][j]))
                throw new AnalizatorException("Sintaxa expresie aritmetica incorecta", line, j, 9);

            getNextAtom(ref line, ref j, true);

            if (atoms[line][j] == "[")// it's array item
            {
                getNextAtom(ref line, ref j, true);

                if (!RegExes.isIndex(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != "]")
                    throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                getNextAtom(ref line, ref j, true);
            }

            while (RegExes.OperatorsArray.Contains(atoms[line][j]))
            {
                getNextAtom(ref line, ref j, true);

                if (!RegExes.isIdentifier(atoms[line][j]) && !RegExes.getConstantRegex().IsMatch(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa expresie aritmetica incorecta", line, j, 9);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] == "[")// it's array item
                {
                    getNextAtom(ref line, ref j, true);

                    if (!RegExes.isIndex(atoms[line][j]))
                        throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                    getNextAtom(ref line, ref j, true);

                    if (atoms[line][j] != "]")
                        throw new AnalizatorException("Sintaxa atribuire incorecta", line, j, 10);

                    getNextAtom(ref line, ref j, true);
                }

                
            }
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

                if (!RegExes.isIdentifier(atoms[line][j]))
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] == "[")// it's array item
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
                else if (atoms[line][j] != ";")
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
            }
            else if (atoms[line][j] == "cout")
            {
                getNextAtom(ref line, ref j, true);

                if (atoms[line][j] != "<<")
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

                getNextAtom(ref line, ref j, true);

                if (RegExes.getConstantRegex().IsMatch(atoms[line][j]))
                {
                    getNextAtom(ref line, ref j, true);

                    if (atoms[line][j] != ";")
                        throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
                }
                else if (RegExes.isIdentifier(atoms[line][j]))
                {
                    getNextAtom(ref line, ref j, true);

                    if (atoms[line][j] == "[")// it's array item
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
                    else if (atoms[line][j] != ";")
                        throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
                }
                else
                    throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);
            }
            else
                throw new AnalizatorException("Sintaxa instructiune IO incorecta", line, j, 6);

            getNextAtom(ref line, ref j, true);
        }

        private void checkDeclaration(ref int line, ref int j)
        {
            // redundant
            if (!(atoms[line][j] == "int" || atoms[line][j] == "float"))
            {
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
            }

            getNextAtom(ref line, ref j, true);

            if (!RegExes.isIdentifier(atoms[line][j]))
            {
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);
            }

            getNextAtom(ref line, ref j, true);
            
            if (atoms[line][j] == "[") // it's array
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
            else if (atoms[line][j] != ";")// error
                throw new AnalizatorException("Sintaxa declarare incorecta", line, j, 5);

            getNextAtom(ref line, ref j, true);
        }

        private string getAtom(int offset, ref int i, ref int j, bool updateIJ)
        {
            string rez = null;

            
            int ii = i, jj = j;

            int lasti, lastj;
            lasti = ii; lastj = jj;

            try
            {
                while (offset > 0)
                {
                    if (atoms[ii].Length > jj + 1)
                    {
                        jj++; lastj = jj;
                        rez = atoms[ii][jj];
                        offset--;
                    }
                    else
                    {
                        ii++;
                        jj = -1;
                    }

                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new AnalizatorException("S-a ajuns la sfarsitul fisierului inaintea inchiderii functiei main", atoms.Length - 1, atoms[atoms.Length-1].Length - 1, 13);
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
