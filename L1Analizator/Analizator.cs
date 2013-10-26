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
            try
            {
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
                throw new AnalizatorException("S-a ajuns la sfarsitul fisierului inaintea inchiderii functiei main", atoms.Length - 1, atoms[atoms.Length - 1].Length - 1, 13);
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

    }
}