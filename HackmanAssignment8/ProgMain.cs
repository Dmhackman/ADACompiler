//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 3
// Due Date: 2/3/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called ProgMain, it is the main driver file for the program.
// This will consist of writing a Lexical Analyzer for a subset of the Ada programming 
// language. The lexical analyzer is a module that is written in C#. This program will 
// read the Ada code from a .ada file and produce an output of the line number, tokens, 
// lexemes, and attributes. Now, that we have completed the lexical analyzer, we will be 
// creating a recursive decent parser that uses the lexical analyzer, main will also 
// then incorporate the parser.
//*****************************************************************************************
using System;

namespace HackmanAssignment7
{
    // The token enum
    public enum Tokens
    {
        begint, modulet, constantt, proceduret, ist, ift, thent, elset, elsift,
        whilet, loopt, floatt, integert, chart, gett, putt, endt, inargt, outargt, inoutargt, idt, numt, literalt,
        relopt, addopt, mulopt, assignopt, lparent, rparent, commat, colont, semit, 
        periodt, quotet, unknown, eoft, restart, nott, putlnt
    }

    // Effectively the global variables for this project
    static class Globals
    {
        public static Tokens Token;
        public static string Lexeme = "";
        public static int depth = 0;
        public static int Value = 0;
        public static float ValueF = 0.0f;
        public static string Literal = "";
        public static int NextChar = 0;
        public static int LineNumber = 1; 
        public static string adaProg = "";
        public static bool param = false;
        public static VarType varT;
        public static EntryType entrT;
        public static ParamPassingMode parampass = ParamPassingMode.inarg;
        public static int tempNum = 1;

    }
    class ProgMain
    {
        static void Main(string[] args)
        {
            Boolean fileOpened;
            fileOpened = false;
            ReadAdaFile codeFile = new ReadAdaFile();
            string adaCode = "";
            string fileName = "";
            LexicalAnalyzer getTokens = new LexicalAnalyzer();

            while (!fileOpened)
            {
                // open file using the command line arguement for the ada program code
                if (args.Length != 0)
                {
                    fileName = args[0];
                    fileOpened = codeFile.GetFileInfo(fileName, out adaCode);
                    while (!fileOpened)
                    {
                        Console.Write("This file did not open, enter a valid file name: ");
                        fileName = Console.ReadLine();
                        fileOpened = codeFile.GetFileInfo(fileName, out adaCode);
                    }
                    

                }
                // else ask for a file name to open for the ada program code
                else
                {
                    while (!fileOpened)
                    {
                        Console.Write("This file did not open, enter a valid file name: ");
                        fileName = Console.ReadLine();
                        fileOpened = codeFile.GetFileInfo(fileName, out adaCode);
                    }
                }
            }

            Console.Write(adaCode);
            adaCode += "--";
            Globals.adaProg = adaCode.Trim();

            ///Console.WriteLine();
            //adaCode = adaCode.Trim();
            //Console.Write(adaCode.Length);
            //Console.Write(adaCode[226]);


            /*for (int i = 0; i < adaCode.Length; i ++)
            {
                Console.Write(adaCode[i]);
            }*/
            //string test = "the";
            //Console.WriteLine(test.Length);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Running the Scanner...");
            //Formats the columns for the output of each token
            Console.WriteLine();
            Console.Write("Line".PadRight(5));
            Console.Write("Token".PadRight(20));
            Console.Write("Lexeme".PadRight(25));
            Console.WriteLine("Attribute");
            int i = 0;

            

            while (!getTokens.GetNextToken(adaCode.Trim()))
            {
                i++;
                if (i % 20 == 0)
                {
                    Console.ReadKey();      // This is so the output only goes for 20 lines and then press a key to continue
                }
                    //done = getTokens.GetNextToken(adaCode[i].Trim());
                    // Output Data for tokens
                    // Console.Write(Globals.LineNumber + " ");
                    
                    Console.Write(Globals.LineNumber.ToString().PadRight(5));
                    Console.Write(Globals.Token.ToString().PadRight(20));
                    Console.Write(Globals.Lexeme.PadRight(25));

                // Output attribute for certain tokens
                if (Globals.Token.ToString() == "numt" && Globals.Lexeme.Contains('.'))
                {
                    Console.Write(Globals.ValueF);
                }
                else if (Globals.Token.ToString() == "numt")
                {
                    Console.Write(Globals.Value);
                }
                else if (Globals.Token.ToString() == "literalt")
                {
                    Console.Write(Globals.Literal);
                }
                else if (Globals.Token.ToString() == "unknown")
                {
                    Console.Write(Globals.Literal);
                }
                Console.WriteLine();
            }

            if (Globals.Token != Tokens.eoft)
            {
                //Console.WriteLine("eoft reached");
                Console.Write(Globals.LineNumber.ToString().PadRight(5));
                Console.Write(Globals.Token.ToString().PadRight(20));
                Console.WriteLine(Globals.Lexeme.PadRight(25));

                // Output attribute for certain tokens
                if (Globals.Token.ToString() == "numt" && Globals.Lexeme.Contains('.'))
                {
                    Console.Write(Globals.ValueF);
                }
                else if (Globals.Token.ToString() == "numt")
                {
                    Console.Write(Globals.Value);
                }
                else if (Globals.Token.ToString() == "literalt")
                {
                    Console.Write(Globals.Literal);
                }
                else if (Globals.Token.ToString() == "unkown")
                {
                    Console.Write(Globals.Literal);
                }
                Console.WriteLine();
            }

            /*for (int i = 0; i < adaCode.Length; i++)
            {
                Console.WriteLine(adaCode[i].Trim());
                while (!done)
                {
                    done = getTokens.GetNextToken(adaCode[i].Trim());
                    // Output Data for tokens
                    // Console.Write(Globals.LineNumber + " ");

                    Console.Write(Globals.LineNumber.ToString().PadRight(5));
                    Console.Write(Globals.Token.PadRight(15));
                    Console.WriteLine(Globals.Lexeme.PadRight(20));

                }
                Globals.LineNumber++;
                Globals.NextChar = 0;
                done = false;
            }*/


            //Console.Write(Globals.adaProg);
            Console.WriteLine();
            Console.WriteLine("Now Running the parser... ");
            Globals.Lexeme = "";
            Globals.Value = 0;
            Globals.ValueF = 0.0f;
            Globals.Literal = "";
            Globals.LineNumber = 1;
            Globals.NextChar = 0;
            Globals.Token = Tokens.restart;

            Parser parsing = new Parser();
            getTokens.GetNextToken(Globals.adaProg);

            parsing.Prog();
            //parsing.writeFinalDepth();
            //Globals.Token == Tokens.eoft
            //getTokens.GetNextToken(Globals.adaProg) 
            if (Globals.Token == Tokens.eoft)
            {
                Console.WriteLine("Successfull Compilation");
            }
            else
            {
                Console.WriteLine("Error: Unused token(s)");
            }


            Console.WriteLine();
            foreach(string a in parsing.tacList)
            {
                
                Console.Write(a);
                i++;
                if (i % 20 == 0)
                {
                    Console.ReadKey();      // This is so the output only goes for 20 lines and then press a key to continue
                }
            }

            WriteTacFile tac = new WriteTacFile();
            fileName = fileName.Replace(".ada", ".TAC");

            tac.WriteFileInfo(fileName, parsing.tacList);

            fileName = fileName.Replace(".TAC", ".asm");

            parsing.generateASMCode();
            tac.WriteFileInfo(fileName, parsing.asmList);


        }
}
}
