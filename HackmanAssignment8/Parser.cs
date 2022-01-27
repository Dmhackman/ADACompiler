//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 3
// Due Date: 2/3/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called Parser, this is where we will be creating the 
// recursive decent parser for the ada language. It will use the lexical analyzer to 
// be able to get the next token.
//*****************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HackmanAssignment7
{
    class Parser
    {
        //Creates a lexical analyzer class in order to use the getNextToken Function out of that class
        LexicalAnalyzer getNext = new LexicalAnalyzer();
        SymbolTable symTab = new SymbolTable();
        List<string> listIdt = new List<string>();
        List<ParamPassingMode> paramlist = new List<ParamPassingMode>();
        List<ParamPassingMode> tempParamlist = new List<ParamPassingMode>();
        //int sizeLocal = 0; offset and locals size is going to be the same
        int numParams = 0;
        int sizeParams = 0;
        int offset = 0;
        int tempOffsets;
        int value;
        float valueF;
        public List<string> tacList = new List<string>();
        string line = "";
        string currentProc = "";
        int litNum = 0;

        List<Procedure> procs = new List<Procedure>();
        List<Literals> lits = new List<Literals>();
        List<Variable> vars = new List<Variable>();
        public List<string> asmList = new List<string>();

        //Globals.depth = 0;

        /*public void writeFinalDepth()
        {
            Console.WriteLine("Printing Depth: 0");
            symTab.writeTable(0);
        }*/

        //Simple match function to check if the desired token is the token that gets recieved
        // if it isnt it ends the program and returns an error message to the user of what was expected
        // then what line the error was on, and lastly what it actually recieved instead of the expected value
        void match(Tokens desired)
        {
            if (Globals.Token == desired)
            {
                getNext.GetNextToken(Globals.adaProg);
            }
            else
            {
                Console.WriteLine("Parse Error : Expected " + desired.ToString() + " on line "
                    + Globals.LineNumber.ToString() + " instead it gets " + Globals.Token);
                Console.ReadKey();
                System.Environment.Exit(0);
            }
        }

        // this is the Prog part of the grammar, it matches all the tokens in proper order and calls args, declarativePart, procedures, and seqofStatments
        public void Prog()
        {
            Procedure proc = new Procedure();
            match(Tokens.proceduret);
            proc.Lexeme = Globals.Lexeme;
            proc.token = Globals.Token;
            proc.depth = Globals.depth;
            proc.TypeOfEntry = EntryType.procEntry;
            if (!symTab.checkDup(proc.Lexeme, proc.depth))
            {
                symTab.insert(proc);
                Console.WriteLine("Inserting... " + proc.Lexeme);
            }
            match(Tokens.idt);
            Globals.depth++;
            Args();
            proc.NumberOfParameters = numParams;
            proc.SizeOfParameters = sizeParams;
            // add the list of in and out 
            foreach (ParamPassingMode a in paramlist)
            {
                //Console.WriteLine("Adding... " + a.ToString());
                proc.paramPassingModeList.Add(a);
            }
            paramlist.Clear();

            match(Tokens.ist);
            DeclarativePart();
            proc.SizeOfLocal = offset;
            offset = 0;

            procs.Add(proc);
            /*Procedure proc2 = new Procedure();
            proc2 = symTab.lookupProc(proc.Lexeme);
            foreach(ParamPassingMode b in proc2.paramPassingModeList)
            {
                Console.WriteLine(b.ToString());
            }*/


            // insert before this point at latest
            Procedures();
            match(Tokens.begint);
            line = "proc" + '\t' + proc.Lexeme + '\n';
            tacList.Add(line);
            currentProc = proc.Lexeme;
            offset = proc.SizeOfLocal;

            SeqOfStatements();
            if (Globals.depth > 1)
            {
                proc.SizeOfLocal = offset;
            }

            match(Tokens.endt);
            if (Globals.Lexeme != proc.Lexeme)
            {
                Console.WriteLine("Error on line " + Globals.LineNumber + ": Expected idt \"" + proc.Lexeme + "\", instead recieved idt \"" + Globals.Lexeme + "\"");
                Console.ReadKey();
                System.Environment.Exit(0);
            }
            line = "endp" + '\t' + proc.Lexeme + '\n' + '\n';
            tacList.Add(line);

            match(Tokens.idt);
            match(Tokens.semit);
            Console.WriteLine("Printing Depth: " + Globals.depth);
            symTab.writeTable(Globals.depth);
            if (Globals.depth == 1)
            {
                symTab.getDepth1Vars(1, ref vars);
            }

            Console.WriteLine("Deleting Depth: " + Globals.depth);
            symTab.deleteDepth(Globals.depth);
            Globals.depth--;
            if (Globals.depth == 0)
            {
                line = "start" + '\t' + "proc" + '\t' + proc.Lexeme;
                tacList.Add(line);
            }
            Console.WriteLine();
            if (Globals.depth == 0)
            {
                Console.WriteLine("Printing Depth: 0");
                symTab.writeTable(0);
            }
            //symTab.writeTable(Globals.depth);
        }

        // this is the args fucntion for reading in any parameters that the function takes in
        // it does nothing unless there is a left parenthsis, if there is it calls the argList function
        void Args()
        {
            if (Globals.Token == Tokens.lparent)
            {
                match(Tokens.lparent);
                ArgList();
                match(Tokens.rparent);
            }
        }

        // this is the declarative part for taking and identifier list followed by colon, then typemark, then semicolon, then can call itsefl
        // if there is more than one declarative part, only executed if an idt is the next token otherwise skips and procedures is called by prog 
        void DeclarativePart()
        {
            if (Globals.Token == Tokens.idt)
            {

                IdentifierList();
                match(Tokens.colont);
                TypeMark();
                if (Globals.entrT == EntryType.varEntry)
                {
                    //Variable var1 = new Variable();
                    foreach (string a in listIdt)
                    {
                        Variable var1 = new Variable();
                        var1.Lexeme = a;
                        var1.token = Tokens.idt;
                        var1.depth = Globals.depth;
                        var1.TypeOfEntry = EntryType.varEntry;
                        var1.offset = offset;
                        var1.param = Globals.param;

                        if (Globals.varT == VarType.intType)
                        {
                            var1.size = 2;
                            var1.TypeOfVariable = VarType.intType;
                            offset += 2;
                            if (!symTab.checkDup(var1.Lexeme, var1.depth))
                            {
                                symTab.insert(var1);
                                Console.WriteLine("Inserting... " + var1.Lexeme);
                            }
                        }
                        else if (Globals.varT == VarType.charType)
                        {
                            var1.size = 1;
                            var1.TypeOfVariable = VarType.charType;
                            offset += 1;
                            if (!symTab.checkDup(var1.Lexeme, var1.depth))
                            {
                                symTab.insert(var1);
                                Console.WriteLine("Inserting... " + var1.Lexeme);
                            }
                        }
                        else if (Globals.varT == VarType.floatType)
                        {
                            var1.size = 4;
                            var1.TypeOfVariable = VarType.floatType;
                            offset += 4;
                            if (!symTab.checkDup(var1.Lexeme, var1.depth))
                            {
                                symTab.insert(var1);
                                Console.WriteLine("Inserting... " + var1.Lexeme);
                            }
                        }
                    }

                }
                else if (Globals.entrT == EntryType.constEntry)
                {

                    foreach (string a in listIdt)
                    {
                        Constant cons1 = new Constant();
                        cons1.Lexeme = a;
                        cons1.token = Tokens.idt;
                        cons1.depth = Globals.depth;
                        cons1.TypeOfEntry = EntryType.constEntry;
                        cons1.TypeOfConstant = Globals.varT;
                        if (Globals.varT == VarType.intType)
                        {
                            cons1.value = value;
                        }
                        else if (Globals.varT == VarType.floatType)
                        {
                            cons1.valueR = valueF;
                        }
                        if (!symTab.checkDup(cons1.Lexeme, cons1.depth))
                        {
                            symTab.insert(cons1);
                            Console.WriteLine("Inserting... " + cons1.Lexeme);
                        }
                    }
                }

                listIdt.Clear();

                match(Tokens.semit);
                DeclarativePart();
            }
        }

        // this is procedures function, it looks for another procedure token within the previous prog call, if there is one it will call prog again 
        void Procedures()
        {
            if (Globals.Token == Tokens.proceduret)
            {
                Prog();
                Procedures();
            }
        }

        // this is the arg list, it calls mode, then identifier list, then looks for a colon
        // then typemark, and lastly calls moreArgs to see if there are more than just one parameter
        void ArgList()
        {
            Globals.param = true;
            Mode();
            IdentifierList();
            match(Tokens.colont);
            TypeMark();
            // typemark is assigned now insert the variable
            if (Globals.entrT == EntryType.varEntry)
            {
                //Variable var1 = new Variable();
                foreach (string a in listIdt)
                {
                    Variable var1 = new Variable();
                    var1.Lexeme = a;
                    var1.token = Tokens.idt;
                    var1.depth = Globals.depth;
                    var1.TypeOfEntry = EntryType.varEntry;
                    var1.offset = sizeParams;
                    var1.param = Globals.param;
                    var1.paramMode = Globals.parampass;
                    paramlist.Add(Globals.parampass);

                    if (Globals.varT == VarType.intType)
                    {
                        var1.size = 2;
                        //offset += 2;
                        sizeParams += 2;
                        if (!symTab.checkDup(var1.Lexeme, var1.depth))
                        {
                            symTab.insert(var1);
                            Console.WriteLine("Inserting... " + var1.Lexeme);
                        }
                    }
                    else if (Globals.varT == VarType.charType)
                    {
                        var1.size = 1;
                        //offset += 1;
                        sizeParams += 1;
                        if (!symTab.checkDup(var1.Lexeme, var1.depth))
                        {
                            symTab.insert(var1);
                            Console.WriteLine("Inserting... " + var1.Lexeme);
                        }
                    }
                    else if (Globals.varT == VarType.floatType)
                    {
                        var1.size = 4;
                        //offset += 4;
                        sizeParams += 4;
                        if (!symTab.checkDup(var1.Lexeme, var1.depth))
                        {
                            symTab.insert(var1);
                            Console.WriteLine("Inserting... " + var1.Lexeme);
                        }
                    }

                    numParams++;
                }

            }
            else if (Globals.entrT == EntryType.constEntry)
            {

                foreach (string a in listIdt)
                {
                    Constant cons1 = new Constant();
                    cons1.Lexeme = a;
                    cons1.token = Tokens.idt;
                    cons1.depth = Globals.depth;
                    cons1.TypeOfEntry = EntryType.constEntry;
                    cons1.TypeOfConstant = Globals.varT;
                    if (Globals.varT == VarType.intType)
                    {
                        cons1.value = value;
                    }
                    else if (Globals.varT == VarType.floatType)
                    {
                        cons1.valueR = valueF;
                    }
                    if (!symTab.checkDup(cons1.Lexeme, cons1.depth))
                    {
                        symTab.insert(cons1);
                        Console.WriteLine("Inserting... " + cons1.Lexeme);
                    }
                }
            }
            listIdt.Clear();
            MoreArgs();
            Globals.param = false;
        }

        // looks for in, out, inout, or it can be unspecified and in that case does nothing
        void Mode()
        {
            if (Globals.Token == Tokens.inargt)
            {

                Globals.parampass = ParamPassingMode.inarg;
                match(Tokens.inargt);
            }
            else if (Globals.Token == Tokens.outargt)
            {

                Globals.parampass = ParamPassingMode.outarg;
                match(Tokens.outargt);
            }
            else if (Globals.Token == Tokens.inoutargt)
            {

                Globals.parampass = ParamPassingMode.inoutarg;
                match(Tokens.inoutargt);
            }
            else
            {
                Globals.parampass = ParamPassingMode.inarg;
            }
        }

        // this is the more args function for if there is more than one arg, it can tell this by looking at the next character for a semicolon, if there is not one it skips
        void MoreArgs()
        {
            if (Globals.Token == Tokens.semit)
            {
                match(Tokens.semit);
                ArgList();
            }

        }

        // this is the sequence of statements which in this grammar if there is an idt up then this will call statment, followed by matching a semicolon
        // and then call statTail
        void SeqOfStatements()
        {
            if (Globals.Token == Tokens.idt || Globals.Token == Tokens.gett || Globals.Token == Tokens.putt || Globals.Token == Tokens.putlnt)
            {
                Statement();
                match(Tokens.semit);
                StatTail();
            }
        }

        //this is the typemark which simply looks to see what the typemark is and makes sure there is one, either integer, float, char or constant
        void TypeMark()
        {
            if (Globals.Token == Tokens.integert)
            {
                Globals.entrT = EntryType.varEntry;
                Globals.varT = VarType.intType;
                match(Tokens.integert);
            }
            else if (Globals.Token == Tokens.floatt)
            {
                Globals.entrT = EntryType.varEntry;
                Globals.varT = VarType.floatType;
                match(Tokens.floatt);
            }
            else if (Globals.Token == Tokens.chart)
            {
                Globals.entrT = EntryType.varEntry;
                Globals.varT = VarType.charType;
                match(Tokens.chart);
            }
            else
            {
                Globals.entrT = EntryType.constEntry;
                match(Tokens.constantt);
                match(Tokens.assignopt);
                if (Globals.Lexeme.Contains('.'))
                {
                    Globals.varT = VarType.floatType;
                    valueF = Globals.ValueF;
                }
                else
                {
                    Globals.varT = VarType.intType;
                    value = Globals.Value;
                }
                match(Tokens.numt);
            }
        }

        // the IdentifierList had to be changed to the grammar -> idt | idt , IdenifierList
        // otherwise you would not be able to choose which way to go
        // it takes an idt, and then looks to see if a comma is the next token, if so it calls itself again for the next idt
        void IdentifierList()
        {
            listIdt.Add(Globals.Lexeme);
            match(Tokens.idt);
            if (Globals.Token == Tokens.commat)
            {
                match(Tokens.commat);
                IdentifierList();
            }
        }

        //this is statement, and depending on if an idt is the next token it will either call AssignStat or IOStat
        void Statement()
        {
            if (Globals.Token == Tokens.idt)
            {
                AssignStat();
            }
            else
            {
                IOStat();
            }
        }

        // this is statTail, if an idt is the next token then it calls statment and after that matches the semicolon and calls itself
        void StatTail()
        {
            if (Globals.Token == Tokens.idt || Globals.Token == Tokens.gett || Globals.Token == Tokens.putt || Globals.Token == Tokens.putlnt)
            {
                Statement();
                match(Tokens.semit);
                StatTail();
            }
        }

        // this checks to make sure that the idt is currently in the symbol table, if so then it will match that, then match assignopt, then call Expr
        // else it will send an error message and stop compilation from using a undeclared variable
        void AssignStat()
        {
            // check that this variable exists
            int depth = 0;
            if (symTab.checkForUndeclared(Globals.Lexeme, ref depth))
            {
                EntryType entType;

                Variable idtptr = new Variable();
                Variable syn = new Variable();

                entType = symTab.getTypeOfVar(Globals.Lexeme, depth);
                if (entType == EntryType.varEntry)
                {
                    idtptr = symTab.lookupVar(Globals.Lexeme);
                    match(Tokens.idt);
                    match(Tokens.assignopt);
                    Expr(ref syn);

                    //syn.Lexeme.StartsWith("_t") idtptr.Lexeme.StartsWith("_t")
                    bool isGood = false;
                    int depth2 = 0;
                    isGood = symTab.checkForUndeclared2(syn.Lexeme, ref depth2);

                    if (depth2 >= 2 && isGood)
                    {
                        syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                    }

                    isGood = symTab.checkForUndeclared2(idtptr.Lexeme, ref depth2);
                    if (depth2 >= 2 && isGood)
                    {
                        idtptr = symTab.ConvertToBP(idtptr.Lexeme, currentProc);
                    }


                    line = idtptr.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\n' + '\n';

                    tacList.Add(line);

                    //line = "Proc" + '\t' + proc.Lexeme + '\n';
                    //tacList.Add(line);
                }
                else if (entType == EntryType.procEntry)
                {
                    ProcCall();
                }

            }
            else
            {
                Console.WriteLine("Error : Undeclared variable " + Globals.Lexeme + " on line "
                    + Globals.LineNumber.ToString());
                Console.ReadKey();
                //Console.WriteLine("Here");
                System.Environment.Exit(0);
            }


        }

        // this is IOStat which currently does nothing in this grammar
        void IOStat()
        {
            if (Globals.Token == Tokens.gett)
            {
                InStat();
            }
            else
            {
                OutStat();
            }
        }

        void InStat()
        {
            match(Tokens.gett);
            match(Tokens.lparent);
            IDList();
            match(Tokens.rparent);
        }

        void IDList()
        {
            Variable syn = new Variable();
            bool isGood = false;
            int depth2 = 0;
            isGood = symTab.checkForUndeclared2(Globals.Lexeme, ref depth2);
            syn.Lexeme = Globals.Lexeme;

            if (depth2 >= 2 && isGood)
            {
                syn = symTab.ConvertToBP(Globals.Lexeme, currentProc);
            }
            line = "rdi" + '\t' + syn.Lexeme + '\n';
            tacList.Add(line);
            match(Tokens.idt);
            IDListTail();
        }

        void IDListTail()
        {
            if (Globals.Token == Tokens.commat)
            {
                match(Tokens.commat);
                Variable syn = new Variable();
                bool isGood = false;
                int depth2 = 0;
                isGood = symTab.checkForUndeclared2(Globals.Lexeme, ref depth2);
                syn.Lexeme = Globals.Lexeme;

                if (depth2 >= 2 && isGood)
                {
                    syn = symTab.ConvertToBP(Globals.Lexeme, currentProc);
                }
                line = "rdi" + '\t' + syn.Lexeme + '\n';
                tacList.Add(line);
                match(Tokens.idt);
                IDListTail();
            }
        }

        void OutStat()
        {
            if (Globals.Token == Tokens.putt)
            {
                match(Tokens.putt);
                match(Tokens.lparent);
                WriteList();
                match(Tokens.rparent);
            }
            else
            {
                match(Tokens.putlnt);
                match(Tokens.lparent);
                WriteList();
                line = "wrln" + '\n';
                tacList.Add(line);
                match(Tokens.rparent);
            }
        }

        void WriteList()
        {
            WriteToken();
            WriteListTail();
        }

        void WriteListTail()
        {
            if (Globals.Token == Tokens.commat)
            {
                match(Tokens.commat);
                WriteToken();
                WriteListTail();
            }
        }

        void WriteToken()
        {
            if (Globals.Token == Tokens.idt)
            {
                Variable syn = new Variable();
                bool isGood = false;
                int depth2 = 0;
                isGood = symTab.checkForUndeclared2(Globals.Lexeme, ref depth2);
                syn.Lexeme = Globals.Lexeme;

                if (depth2 >= 2 && isGood)
                {
                    syn = symTab.ConvertToBP(Globals.Lexeme, currentProc);
                }
                line = "wri" + '\t' + syn.Lexeme + '\n';
                tacList.Add(line);

                match(Tokens.idt);
            }
            else if (Globals.Token == Tokens.numt)
            {
                line = "wri" + '\t' + Globals.Lexeme + '\n';
                tacList.Add(line);
                match(Tokens.numt);
            }
            else
            {
                Literals lit = new Literals();
                lit.depth = 1;
                lit.attribute = Globals.Lexeme;
                lit.token = Globals.Token;
                lit.TypeOfEntry = EntryType.litEntry;
                lit.Lexeme = "_S" + litNum.ToString();
                litNum++;

                symTab.insert(lit);
                Console.WriteLine("Inserting... " + lit.Lexeme);
                lits.Add(lit);

                line = "wrs" + '\t' + lit.Lexeme + '\n';
                tacList.Add(line);

                match(Tokens.literalt);
            }
        }

        // this is expr it currently just calls relation
        void Expr(ref Variable syn)
        {
            Relation(ref syn);
        }

        // this is relation it currently just calls SimpleExpr
        void Relation(ref Variable syn)
        {
            SimpleExpr(ref syn);

            if (Globals.depth < 2 && !syn.Lexeme.StartsWith("_t"))
            {
                Variable temp = new Variable();
                temp = symTab.newTemp(currentProc, ref offset);

                line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\n';
                tacList.Add(line);
                syn = temp;
            }
            else if (Globals.depth >= 2 && !syn.Lexeme.StartsWith("_bp"))
            {
                Variable temp = new Variable();
                temp = symTab.newTemp(currentProc, ref offset);
                if (Globals.depth >= 2 && syn.Lexeme.StartsWith("_t"))
                {
                    syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                }
                if (Globals.depth >= 2 && temp.Lexeme.StartsWith("_t"))
                {
                    temp = symTab.ConvertToBP(temp.Lexeme, currentProc);
                }

                line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\n';
                tacList.Add(line);
                syn = temp;
            }


        }

        // this is SimpleExpr it calls term and then moreTerm
        void SimpleExpr(ref Variable syn)
        {
            string op1 = "";
            Variable syn2 = new Variable();

            Term(ref syn);

            MoreTerm(ref syn2, ref op1);
            if (op1 != "")
            {
                Variable temp = new Variable();
                temp = symTab.newTemp(currentProc, ref offset);
                if (Globals.depth >= 2 && syn.Lexeme.StartsWith("_t"))
                {
                    syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                }
                if (Globals.depth >= 2 && temp.Lexeme.StartsWith("_t"))
                {
                    temp = symTab.ConvertToBP(temp.Lexeme, currentProc);
                }
                if (Globals.depth >= 2 && syn2.Lexeme.StartsWith("_t"))
                {
                    syn2 = symTab.ConvertToBP(syn2.Lexeme, currentProc);
                }

                line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\t' + op1 + '\t' + syn2.Lexeme + '\n';
                tacList.Add(line);
                syn = temp;
            }


        }

        // this is moreterm it checks for the next token to be addopt, if so it matches that and then calls term and then itself
        void MoreTerm(ref Variable syn, ref string op)
        {
            if (Globals.Token == Tokens.addopt)
            {
                Variable syn2 = new Variable();
                op = Globals.Lexeme;
                string op2 = "";

                match(Tokens.addopt);

                Term(ref syn);

                MoreTerm(ref syn2, ref op2);

                if (op2 != "")
                {
                    Variable temp = new Variable();
                    temp = symTab.newTemp(currentProc, ref offset);

                    if (Globals.depth >= 2 && syn.Lexeme.StartsWith("_t"))
                    {
                        syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                    }
                    if (Globals.depth >= 2 && temp.Lexeme.StartsWith("_t"))
                    {
                        temp = symTab.ConvertToBP(temp.Lexeme, currentProc);
                    }
                    if (Globals.depth >= 2 && syn2.Lexeme.StartsWith("_t"))
                    {
                        syn2 = symTab.ConvertToBP(syn2.Lexeme, currentProc);
                    }

                    line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\t' + op2 + '\t' + syn2.Lexeme + '\n';
                    tacList.Add(line);
                    syn = temp;
                }
            }
            else
            {
                op = "";
            }
        }

        // this is term it calls factor and then moreFactor
        void Term(ref Variable syn)
        {
            string op1 = "";
            Variable syn2 = new Variable();

            Factor(ref syn);

            MoreFactor(ref syn2, ref op1);

            if (op1 != "")
            {
                Variable temp = new Variable();
                temp = symTab.newTemp(currentProc, ref offset);

                if (Globals.depth >= 2 && syn.Lexeme.StartsWith("_t"))
                {
                    syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                }
                if (Globals.depth >= 2 && temp.Lexeme.StartsWith("_t"))
                {
                    temp = symTab.ConvertToBP(temp.Lexeme, currentProc);
                }
                if (Globals.depth >= 2 && syn2.Lexeme.StartsWith("_t"))
                {
                    syn2 = symTab.ConvertToBP(syn2.Lexeme, currentProc);
                }

                line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\t' + op1 + '\t' + syn2.Lexeme + '\n';
                tacList.Add(line);
                syn = temp;
            }

        }

        // this is MoreFactor it checks for the next token to be mulopt, if so it matches that and then calls factor and itself
        void MoreFactor(ref Variable syn, ref string op)
        {
            if (Globals.Token == Tokens.mulopt)
            {
                //string op = "";
                Variable syn2 = new Variable();
                op = Globals.Lexeme;
                string op2 = "";

                match(Tokens.mulopt);

                Factor(ref syn);

                MoreFactor(ref syn2, ref op2);


                if (op2 != "")
                {
                    Variable temp = new Variable();
                    temp = symTab.newTemp(currentProc, ref offset);
                    if (Globals.depth >= 2 && syn.Lexeme.StartsWith("_t"))
                    {
                        syn = symTab.ConvertToBP(syn.Lexeme, currentProc);
                    }
                    if (Globals.depth >= 2 && temp.Lexeme.StartsWith("_t"))
                    {
                        temp = symTab.ConvertToBP(temp.Lexeme, currentProc);
                    }
                    if (Globals.depth >= 2 && syn2.Lexeme.StartsWith("_t"))
                    {
                        syn2 = symTab.ConvertToBP(syn2.Lexeme, currentProc);
                    }

                    line = temp.Lexeme + '\t' + '=' + '\t' + syn.Lexeme + '\t' + op2 + '\t' + syn2.Lexeme + '\n';
                    tacList.Add(line);
                    syn = temp;
                }

                //line += syn.Lexeme;
                //line = idtptr.Lexeme + '\t' + '=' + '\t' + syn.Lexeme;


                // assign syn to temp value for passing back up 

            }
            else
            {
                op = "";
            }

        }

        /*Variable idtptr = new Variable();
                Variable syn = new Variable();
                idtptr = symTab.lookupVar(Globals.Lexeme);
                Expr(ref syn);
                line = idtptr.Lexeme + '\t' + '=' + '\t' + syn.Lexeme;
                tacList.Add(line);*/
        // this is factor it matches either an idt, numt, ( expr() ), nott factor(), addopt factor()
        void Factor(ref Variable syn)
        {
            if (Globals.Token == Tokens.idt)
            {
                int depth = 0;
                if (symTab.checkForUndeclared(Globals.Lexeme, ref depth))
                {
                    bool consEnt = false;
                    int num = 0;
                    consEnt = symTab.checkConst(Globals.Lexeme, ref num);
                    if (consEnt)
                    {
                        syn.Lexeme = num.ToString();
                    }
                    else
                    {
                        syn = symTab.lookupVar(Globals.Lexeme);
                        if (depth >= 2)
                        {
                            syn = symTab.ConvertToBP(Globals.Lexeme, currentProc);
                        }
                    }

                    match(Tokens.idt);
                }
                else
                {
                    Console.WriteLine("Error : Undeclared variable " + Globals.Lexeme + " on line "
                   + Globals.LineNumber.ToString());
                    Console.ReadKey();
                    //System.Environment.Exit(0);
                }
            }
            else if (Globals.Token == Tokens.numt)
            {
                syn.Lexeme = Globals.Lexeme;
                match(Tokens.numt);
            }
            else if (Globals.Token == Tokens.lparent)
            {
                match(Tokens.lparent);

                Expr(ref syn);

                match(Tokens.rparent);
            }
            else if (Globals.Token == Tokens.nott)
            {
                string not = Globals.Lexeme;
                match(Tokens.nott);
                Factor(ref syn);
                syn.Lexeme = not + " " + syn.Lexeme;
            }
            else if (Globals.Token == Tokens.addopt)
            {
                string op = Globals.Lexeme;
                match(Tokens.addopt);
                Factor(ref syn);
                syn.Lexeme = op + " " + syn.Lexeme;
            }
        }


        void ProcCall()
        {
            string procNm = Globals.Lexeme;
            Procedure procLook = new Procedure();
            procLook = symTab.lookupProc(procNm);
            if (procLook == null)
            {
                Console.WriteLine("Error : Undeclared procedure call " + Globals.Lexeme + " on line "
                    + Globals.LineNumber.ToString());
                Console.ReadKey();
                System.Environment.Exit(0);
            }
            foreach (ParamPassingMode a in procLook.paramPassingModeList)
            {
                tempParamlist.Add(a);
            }

            match(Tokens.idt);
            match(Tokens.lparent);
            Params();
            //Console.WriteLine("Here Already");
            match(Tokens.rparent);
            line = "call " + '\t' + procNm + '\n';
            tacList.Add(line);

        }

        void Params()
        {
            if (Globals.Token == Tokens.idt)
            {
                if (tempParamlist.Count != 0 && (tempParamlist[0] == ParamPassingMode.outarg || tempParamlist[0] == ParamPassingMode.inoutarg))
                {
                    line = "push" + '\t' + "@" + Globals.Lexeme + '\n';
                    tacList.Add(line);
                    tempParamlist.RemoveAt(0);
                }
                else
                {
                    line = "push" + '\t' + Globals.Lexeme + '\n';
                    tacList.Add(line);
                    if (tempParamlist.Count != 0)
                    {
                        tempParamlist.RemoveAt(0);
                    }
                }
                //Console.WriteLine("Here1");
                match(Tokens.idt);
                ParamsTail();
            }
            else if (Globals.Token == Tokens.numt)
            {
                if (tempParamlist.Count != 0 && (tempParamlist[0] == ParamPassingMode.outarg || tempParamlist[0] == ParamPassingMode.inoutarg))
                {
                    line = "push" + '\t' + "@" + Globals.Lexeme + '\n';
                    tacList.Add(line);
                    tempParamlist.RemoveAt(0);
                }
                else
                {
                    line = "push" + '\t' + Globals.Lexeme + '\n';
                    tacList.Add(line);
                    if (tempParamlist.Count != 0)
                    {
                        tempParamlist.RemoveAt(0);
                    }
                }
                //Console.WriteLine("Here2");
                match(Tokens.numt);
                ParamsTail();
            }

        }

        void ParamsTail()
        {
            if (Globals.Token == Tokens.commat)
            {
                match(Tokens.commat);
                if (Globals.Token == Tokens.idt)
                {

                    if (tempParamlist.Count != 0 && (tempParamlist[0] == ParamPassingMode.outarg || tempParamlist[0] == ParamPassingMode.inoutarg))
                    {
                        line = "push" + '\t' + "@" + Globals.Lexeme + '\n';
                        tacList.Add(line);
                        tempParamlist.RemoveAt(0);
                    }
                    else
                    {
                        line = "push" + '\t' + Globals.Lexeme + '\n';
                        tacList.Add(line);
                        if (tempParamlist.Count != 0)
                        {
                            tempParamlist.RemoveAt(0);
                        }

                    }
                    //Console.WriteLine("Here3");
                    match(Tokens.idt);
                    ParamsTail();
                }
                else
                {
                    if (tempParamlist.Count != 0 && (tempParamlist[0] == ParamPassingMode.outarg || tempParamlist[0] == ParamPassingMode.inoutarg))
                    {
                        line = "push" + '\t' + "@" + Globals.Lexeme + '\n';
                        tacList.Add(line);
                        tempParamlist.RemoveAt(0);
                    }
                    else
                    {
                        line = "push" + '\t' + Globals.Lexeme + '\n';
                        tacList.Add(line);
                        if (tempParamlist.Count != 0)
                        {
                            tempParamlist.RemoveAt(0);
                        }
                    }
                    //Console.WriteLine("Here4");
                    match(Tokens.numt);
                    ParamsTail();
                }
            }



        }


        public void generateASMCode()
        {
            string[] tacCode = tacList.ToArray();
            string asmLine;

            asmLine = "\t.model small\n";
            asmList.Add(asmLine);
            asmLine = "\t.586\n";
            asmList.Add(asmLine);
            asmLine = "\t.stack 100h\n";
            asmList.Add(asmLine);
            asmLine = "\t.data\n";
            asmList.Add(asmLine);

            foreach (Literals lit in lits)
            {
                asmLine = lit.Lexeme + '\t' + "DB" + '\t' + lit.attribute + ", \"$\"" + '\n';
                asmList.Add(asmLine);
            }

            foreach (Variable va in vars)
            {
                asmLine = va.Lexeme + '\t' + "DW" + '\t' + "?\n";
                asmList.Add(asmLine);
            }

            asmLine = "\t.code\n";
            asmList.Add(asmLine);
            asmLine = "\tinclude io.asm\n\n";
            asmList.Add(asmLine);

            asmLine = "start\tPROC\n";
            asmList.Add(asmLine);
            asmLine = "\tmov ax, @data\n";
            asmList.Add(asmLine);
            asmLine = "\tmov ds, ax\n";
            asmList.Add(asmLine);
            asmLine = "\tcall " + procs[0].Lexeme + '\n';
            asmList.Add(asmLine);
            asmLine = "\tmov ah, 04ch\n";
            asmList.Add(asmLine);
            asmLine = "\tint 21h\n";
            asmList.Add(asmLine);
            asmLine = "start\tENDP\n\n";
            asmList.Add(asmLine);

            foreach (Procedure p in procs)
            {
                int start = 0;
                int end = 0;
                string lookStart;
                lookStart = "proc" + '\t' + p.Lexeme;
                string lookEnd;
                lookEnd = "endp" + '\t' + p.Lexeme;
                for (int i = 0; i < tacCode.Length; i++)
                {
                    if (tacCode[i].Contains(lookStart) && !tacCode[i].Contains("start"))
                    {
                        start = i;
                        //Console.WriteLine(start.ToString());
                    }

                    if (tacCode[i].Contains(lookEnd))
                    {
                        end = i;
                        //Console.WriteLine(end.ToString());
                    }
                }

                asmLine = p.Lexeme + "\tPROC\n";
                asmList.Add(asmLine);
                asmLine = "\tpush bp\n";
                asmList.Add(asmLine);
                asmLine = "\tmov bp, sp\n";
                asmList.Add(asmLine);
                asmLine = "\tsub sp, " + p.SizeOfLocal.ToString() + "\n";
                asmList.Add(asmLine);

                for (int i = start + 1; i < end; i++)
                {
                    tacCode[i] = tacCode[i].Trim();
                    string[] separators = { " ", "\t" };
                    string[] strlist = tacCode[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    if (strlist[0].Equals("call", StringComparison.OrdinalIgnoreCase))
                    {
                        asmLine = "\tcall " + strlist[1] + '\n';
                        asmList.Add(asmLine);
                    }
                    else if (strlist[0].Equals("push", StringComparison.OrdinalIgnoreCase))
                    {
                        asmLine = "\tpush " + strlist[1] + '\n';
                        asmList.Add(asmLine);
                    }
                    else if (strlist[0].Equals("wrs", StringComparison.OrdinalIgnoreCase))
                    {
                        asmLine = "\tmov dx, OFFSET " + strlist[1] + '\n';
                        asmList.Add(asmLine);
                        asmLine = "\tcall writestr\n";
                        asmList.Add(asmLine);
                    }
                    else if (strlist[0].Equals("wri", StringComparison.OrdinalIgnoreCase))
                    {
                        if (strlist[1].StartsWith("_bp"))
                        {
                            asmLine = "\tmov ax, [" + strlist[1].Remove(0, 1) + "]\n";
                            asmList.Add(asmLine);
                        }
                        else
                        {
                            asmLine = "\tmov ax, [" + strlist[1] + "]\n";
                            asmList.Add(asmLine);
                        }

                        asmLine = "\tmov dx, ax\n";
                        asmList.Add(asmLine);
                        asmLine = "\tcall writeint\n";
                        asmList.Add(asmLine);
                    }
                    else if (strlist[0].Equals("wrln", StringComparison.OrdinalIgnoreCase))
                    {
                        asmLine = "\tcall writeln\n";
                        asmList.Add(asmLine);
                    }
                    else if (strlist[0].Equals("rdi", StringComparison.OrdinalIgnoreCase))
                    {
                        asmLine = "\tcall readint\n";
                        asmList.Add(asmLine);
                        if (strlist[1].StartsWith("_bp"))
                        {
                            asmLine = "\tmov [" + strlist[1].Remove(0, 1) + "], bx\n";
                            asmList.Add(asmLine);
                        }
                        else
                        {
                            asmLine = "\tmov [" + strlist[1] + "], bx\n";
                            asmList.Add(asmLine);
                        }
                    }
                    else if (strlist.Length == 3)
                    {
                        if (strlist[2].StartsWith("_bp"))
                        {
                            asmLine = "\tmov ax, [" + strlist[2].Remove(0, 1) + "]\n";
                            asmList.Add(asmLine);
                        }
                        else if (!char.IsDigit(strlist[2][0]))
                        {
                            asmLine = "\tmov ax, [" + strlist[2] + "]\n";
                            asmList.Add(asmLine);
                        }
                        else
                        {
                            asmLine = "\tmov ax, " + strlist[2] + "\n";
                            asmList.Add(asmLine);
                        }



                        if (strlist[0].StartsWith("_bp"))
                        {
                            asmLine = "\tmov [" + strlist[0].Remove(0, 1) + "], ax\n";
                            asmList.Add(asmLine);
                        }
                        else
                        {
                            asmLine = "\tmov [" + strlist[0] + "], ax\n";
                            asmList.Add(asmLine);
                        }
                    }
                    else if (strlist.Length == 5)
                    {
                        if (strlist[3] == "+")
                        {
                            if (strlist[2].StartsWith("_bp"))
                            {
                                asmLine = "\tmov ax, [" + strlist[2].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[2][0]))
                            {
                                asmLine = "\tmov ax, [" + strlist[2] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov ax, " + strlist[2] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[4].StartsWith("_bp"))
                            {
                                asmLine = "\tadd ax, [" + strlist[4].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[4][0]))
                            {
                                asmLine = "\tadd ax, [" + strlist[4] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tadd ax, " + strlist[4] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[0].StartsWith("_bp"))
                            {
                                asmLine = "\tmov [" + strlist[0].Remove(0, 1) + "], ax\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov [" + strlist[0] + "], ax\n";
                                asmList.Add(asmLine);
                            }

                        }
                        else if (strlist[3] == "-")
                        {
                            if (strlist[2].StartsWith("_bp"))
                            {
                                asmLine = "\tmov ax, [" + strlist[2].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[2][0]))
                            {
                                asmLine = "\tmov ax, [" + strlist[2] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov ax, " + strlist[2] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[4].StartsWith("_bp"))
                            {
                                asmLine = "\tsub ax, [" + strlist[4].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[4][0]))
                            {
                                asmLine = "\tsub ax, [" + strlist[4] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tsub ax, " + strlist[4] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[0].StartsWith("_bp"))
                            {
                                asmLine = "\tmov [" + strlist[0].Remove(0, 1) + "], ax\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov [" + strlist[0] + "], ax\n";
                                asmList.Add(asmLine);
                            }
                        }
                        else if (strlist[3] == "*")
                        {
                            if (strlist[2].StartsWith("_bp"))
                            {
                                asmLine = "\tmov ax, [" + strlist[2].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[2][0]))
                            {
                                asmLine = "\tmov ax, [" + strlist[2] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov ax, " + strlist[2] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[4].StartsWith("_bp"))
                            {
                                asmLine = "\tmov bx, [" + strlist[4].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[4][0]))
                            {
                                asmLine = "\tmov bx, [" + strlist[4] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov bx, " + strlist[4] + "\n";
                                asmList.Add(asmLine);
                            }

                            asmLine = "\timul bx\n";
                            asmList.Add(asmLine);

                            if (strlist[0].StartsWith("_bp"))
                            {
                                asmLine = "\tmov [" + strlist[0].Remove(0, 1) + "], ax\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov [" + strlist[0] + "], ax\n";
                                asmList.Add(asmLine);
                            }

                        }
                        else if (strlist[3] == "/")
                        {
                            if (strlist[2].StartsWith("_bp"))
                            {
                                asmLine = "\tmov ax, [" + strlist[2].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[2][0]))
                            {
                                asmLine = "\tmov ax, [" + strlist[2] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov ax, " + strlist[2] + "\n";
                                asmList.Add(asmLine);
                            }

                            if (strlist[4].StartsWith("_bp"))
                            {
                                asmLine = "\tmov bx, [" + strlist[4].Remove(0, 1) + "]\n";
                                asmList.Add(asmLine);
                            }
                            else if (!char.IsDigit(strlist[4][0]))
                            {
                                asmLine = "\tmov bx, [" + strlist[4] + "]\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov bx, " + strlist[4] + "\n";
                                asmList.Add(asmLine);
                            }

                            asmLine = "\tidiv bx\n";
                            asmList.Add(asmLine);

                            if (strlist[0].StartsWith("_bp"))
                            {
                                asmLine = "\tmov [" + strlist[0].Remove(0, 1) + "], ax\n";
                                asmList.Add(asmLine);
                            }
                            else
                            {
                                asmLine = "\tmov [" + strlist[0] + "], ax\n";
                                asmList.Add(asmLine);
                            }

                        }
                    }

                }


                asmLine = "\tadd sp, " + p.SizeOfLocal.ToString() + "\n";
                asmList.Add(asmLine);
                asmLine = "\tpop bp\n";
                asmList.Add(asmLine);
                asmLine = "\tret " + p.SizeOfParameters.ToString() + "\n";
                asmList.Add(asmLine);
                asmLine = p.Lexeme + "\tendp\n\n";
                asmList.Add(asmLine);


            }

            asmLine = "main\tPROC\n";
            asmList.Add(asmLine);
            asmLine = "\tcall " + procs[0].Lexeme + '\n';
            asmList.Add(asmLine);
            asmLine = "main\tendp\n";
            asmList.Add(asmLine);
            asmLine = "END\tstart\n";
            asmList.Add(asmLine);

        }
    }
}
