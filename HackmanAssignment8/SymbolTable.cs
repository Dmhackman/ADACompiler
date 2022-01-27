//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 4
// Due Date: 3/5/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called SymbolTable, this is where the structure for the symbol
// table is created, and all of the functions are created
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;


namespace HackmanAssignment7
{
    enum VarType { charType, intType, floatType };

    enum EntryType { constEntry, varEntry, procEntry, litEntry };

    enum ParamPassingMode { inarg, outarg, inoutarg };

    class SymbolTable
    {
        const int TableSize = 211;
        public static List<TableEntry>[] tableArr = new List<TableEntry>[TableSize];

        // Constructor to initialize the array of lists to empty
        public SymbolTable()
        {
            for (int i = 0; i < TableSize; i++)
            {
                tableArr[i] = new List<TableEntry>();
            }
        }

        // This will take in a lex, token, and depth and insert them into the symbol table as a Table Entry
        // in this case it is always inserting a base class tableEntry and has not inserted any of the derrived 
        // class versions of tableEntry, that will be figured out by the parser later on.

        public void insert(TableEntry entry)
        {
            int x;
            x = (int)SymbolTable.PJWHash(entry.Lexeme);

            /*if(type == EntryType.varEntry)
            {
                Variable vEntry = new Variable();
                vEntry.token = tok;
                vEntry.Lexeme = lex;
                vEntry.depth = depth;
                tableArr[x].Insert(0, vEntry);
            }
            else if(type == EntryType.constEntry)
            {
                Constant cEntry = new Constant();
                cEntry.token = tok;
                cEntry.Lexeme = lex;
                cEntry.depth = depth;
                tableArr[x].Insert(0, cEntry);
            }
            else if(type == EntryType.procEntry)
            {
                Procedure pEntry = new Procedure();
                pEntry.token = tok;
                pEntry.Lexeme = lex;
                pEntry.depth = depth;
                tableArr[x].Insert(0, pEntry);
            }
            else
            {
                TableEntry entry = new TableEntry();
                entry.token = tok;
                entry.Lexeme = lex;
                entry.depth = depth;
                tableArr[x].Insert(0, entry); // inserts into the front of the list
            }*/

            tableArr[x].Insert(0, entry);

        }

        // This will take in a lex and look to see if it is in the symbol table,
        // if it is it returns it as a tableEntry, if not it will return null
        public TableEntry lookup(string lex)
        {
            int x;
            x = (int)SymbolTable.PJWHash(lex);

            if (tableArr[x].Count == 0)
            {
                return null;
            }
            else if (tableArr[x].Exists(e => e.Lexeme == lex))
            {
                return tableArr[x].Find(e => e.Lexeme == lex);
            }
            else
                return null;

        }

        //this function looksup and returns a procedure entry 
        public Procedure lookupProc(string lex)
        {
            int x;
            x = (int)SymbolTable.PJWHash(lex);

            if (tableArr[x].Count == 0)
            {
                return null;
            }
            else if (tableArr[x].Exists(e => e.Lexeme == lex))
            {
                return (Procedure)tableArr[x].Find(e => e.Lexeme == lex);
            }
            else
                return null;
        }

        // this function is passed a lexeme from the parser and will lookup and then convert that into its form of _bp+/-#
        public Variable ConvertToBP(string lex, string currentProc)
        {
            int x;
            x = (int)SymbolTable.PJWHash(lex);

            if (tableArr[x].Count == 0)
            {
                return null;
            }
            else if (tableArr[x].Exists(e => e.Lexeme == lex))
            {
                Variable vart = new Variable();
                Variable NoChange = new Variable();
                NoChange = (Variable)tableArr[x].Find(e => e.Lexeme == lex);

                vart.Lexeme = NoChange.Lexeme;
                vart.offset = NoChange.offset;
                vart.param = NoChange.param;
                vart.paramMode = NoChange.paramMode;

                if (vart.param)
                {
                    Procedure proc1 = new Procedure();
                    proc1 = lookupProc(currentProc);
                    int sz = 0;
                    sz = proc1.SizeOfParameters;
                    sz += 2;
                    sz -= vart.offset;
                    string newLex;
                    if (vart.paramMode != ParamPassingMode.inarg)
                    {
                        newLex = "@_bp+" + sz.ToString();
                    }
                    else
                    {
                        newLex = "_bp+" + sz.ToString();
                    }
                    vart.Lexeme = newLex;
                }
                else
                {
                    Procedure proc1 = new Procedure();
                    proc1 = lookupProc(currentProc);
                    int sz = 0;
                    //sz = proc1.SizeOfParameters;
                    sz += 2;
                    sz += vart.offset;
                    string newLex;
                    newLex = "_bp-" + sz.ToString();
                    vart.Lexeme = newLex;
                }

                return vart;
            }
            else
                return null;
        }

        //this function is used to see if a lexeme is a constant, if it is then the value will be passed back by ref
        // so that this value can be inserted into the TAC instead of the variable name
        public bool checkConst(string lex, ref int num)
        {
            int x;
            x = (int)SymbolTable.PJWHash(lex);

            if (tableArr[x].Count == 0)
            {
                return false;
            }
            else if (tableArr[x].Exists(e => e.Lexeme == lex))
            {
                TableEntry con = new TableEntry();
                con = tableArr[x].Find(e => e.Lexeme == lex);
                if (con.TypeOfEntry == EntryType.constEntry)
                {
                    Constant con2 = new Constant();
                    con2 = (Constant)tableArr[x].Find(e => e.Lexeme == lex);
                    num = con2.value;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
                return false;

        }

        //this function looksup and returns a variable entry 
        public Variable lookupVar(string lex)
        {
            int x;
            x = (int)SymbolTable.PJWHash(lex);

            if (tableArr[x].Count == 0)
            {
                return null;
            }
            else if (tableArr[x].Exists(e => e.Lexeme == lex))
            {
                return (Variable)tableArr[x].Find(e => e.Lexeme == lex);
            }
            else
                return null;
        }
        // checks for undeclared variables when used in the parser, basically the same as check dup but switched around for if it is not found then there is
        // an error message
        public bool checkForUndeclared(string lex, ref int depth)
        {
            TableEntry check = new TableEntry();

            check = lookup(lex);
            if (check != null)
            {
                depth = check.depth;
                //check.depth == depth
                return true;
            }
            else
            {
                Console.WriteLine("Undeclared Variable used at line " + Globals.LineNumber + ": \"" + lex + "\" was not declared at this depth");
                return false;
            }
        }

        //this was used again and then modified slightly from above, because i wanted to be able to check and see if a lex was a variable, but not return error messages like above
        public bool checkForUndeclared2(string lex, ref int depth)
        {
            TableEntry check = new TableEntry();

            check = lookup(lex);
            if (check != null)
            {
                depth = check.depth;

                //Console.WriteLine("Duplicate Entry at line " + Globals.LineNumber + ": \"" + lex + "\" already exists at this depth in the symbol table");
                return true;


            }
            else
            {
                //Console.WriteLine("Undeclared Variable used at line " + Globals.LineNumber + ": \"" + lex + "\" was not declared at this depth");
                return false;
            }
        }

        // this simply gets passed a depth and lexeme and will return the type of entry that this is, either proc var or const entry type
        public EntryType getTypeOfVar(string lex, int depth)
        {
            TableEntry check = new TableEntry();

            check = lookup(lex);

            if (check.TypeOfEntry == EntryType.procEntry)
            {
                return EntryType.procEntry;
            }
            else if (check.TypeOfEntry == EntryType.varEntry)
            {
                return EntryType.varEntry;
            }
            else
            {
                return EntryType.constEntry;
            }
        }


        //nthis function creates a new temp variable and inserts it into the table, and returns a pointer back to the parser for this
        // temp variable
        public Variable newTemp(string currentProc, ref int offset)
        {
            // use tempNum
            string temp = "";
            temp = "_t" + Globals.tempNum.ToString();
            Globals.tempNum++;
            Variable tempVar = new Variable();
            tempVar.Lexeme = temp;
            tempVar.depth = Globals.depth;
            tempVar.token = Tokens.idt;
            tempVar.TypeOfEntry = EntryType.varEntry;
            tempVar.offset = offset;
            offset += 2;
            tempVar.size = 2;
            tempVar.TypeOfVariable = VarType.intType;
            //offset += 2;
            tempVar.param = false;

            if (!checkDup(tempVar.Lexeme, Globals.depth))
            {
                insert(tempVar);
                Console.WriteLine("Inserting... " + tempVar.Lexeme);
            }

            return lookupVar(tempVar.Lexeme);

        }

        public bool checkDup(string lex, int depth)
        {
            TableEntry check = new TableEntry();

            check = lookup(lex);
            if (check != null)
            {
                if (check.depth == depth)
                {
                    Console.WriteLine("Duplicate Entry at line " + Globals.LineNumber + ": \"" + lex + "\" already exists at this depth in the symbol table");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        // this deletes all table entries at the given depth
        public void deleteDepth(int depth)
        {
            for (int i = 0; i < TableSize; i++)
            {
                if (tableArr[i].Count != 0)
                {
                    tableArr[i].RemoveAll(e => e.depth == depth);
                }
            }
            Globals.tempNum = 1;
        }

        // this prints all table entries lexeme's at the given depth
        // make print token lexeme and depth 
        public void writeTable(int depth)
        {

            List<TableEntry> write = new List<TableEntry>();
            for (int i = 0; i < TableSize; i++)
            {
                if (tableArr[i].Count != 0)
                {
                    write.AddRange(tableArr[i].FindAll(e => e.depth == depth));
                }
            }

            foreach (TableEntry temp in write)
            {
                Console.WriteLine(temp.Lexeme + " " + temp.token + " " + temp.depth);
            }
        }

        public void getDepth1Vars(int depth, ref List<Variable> variables)
        {
            List<TableEntry> write = new List<TableEntry>();
            for (int i = 0; i < TableSize; i++)
            {
                if (tableArr[i].Count != 0)
                {
                    write.AddRange(tableArr[i].FindAll(e => e.depth == depth));
                }
            }

            foreach (TableEntry temp in write)
            {
                if (temp.TypeOfEntry == EntryType.varEntry)
                {
                    variables.Add((Variable)temp);
                }
            }
        }


        // this is a pjw hash function, i was able to find it online
        // https:/ /www.programmingalgorithms.com/algorithm/pjw-hash/
        // i had to tweak it to get it to work with our program, 
        // i changed it to a private method, added in the tableSize calculation
        // and tested it to make sure that it was consistent and fairly random with
        // the placement of words in the table
        private static uint PJWHash(string str)
        {
            const uint BitsInUnsignedInt = (uint)(sizeof(uint) * 8);
            const uint ThreeQuarters = (uint)((BitsInUnsignedInt * 3) / 4);
            const uint OneEighth = (uint)(BitsInUnsignedInt / 8);
            const uint HighBits = (uint)(0xFFFFFFFF) << (int)(BitsInUnsignedInt - OneEighth);
            uint hash = 0;
            uint test = 0;
            uint i = 0;

            for (i = 0; i < str.Length; i++)
            {
                hash = (hash << (int)OneEighth) + ((byte)str[(int)i]);

                if ((test = hash & HighBits) != 0)
                {
                    hash = ((hash ^ (test >> (int)ThreeQuarters)) & (~HighBits));
                }
            }

            return hash % TableSize;
        }



    }

}
