//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 3
// Due Date: 2/3/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called GetNextToken, this will do exactly that. It will get
// the next token, lexeme and any attributes for that token. It will then edit the info
// for the global variables in order to update each token. 
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HackmanAssignment7
{
    class LexicalAnalyzer
    {
        //Boolean first = false;

        // This function gets called by the lexical analyzer, which has main in it
        // it will take a string as input and loop through it to get each char
        // as it goes it will decide what to do with each char, if it is to combine
        // with another char in order to create a larger token, or if it is a symbol
        // and token on its own. Then it will call the processToken function when 
        // a token is ready to be processed, that fucntion will assign the token to
        // a global variable for token, lexeme and attribute (Token dependent).
        // this function also lets the lexical analyzer know when it has reached the 
        // end of the file by setting a return value of true to this function.
        public Boolean GetNextToken(string Code)
        {
            //Boolean endFile = false;
            int lookahead;
            string words = "";
            //Code += " ";

            for (int i = Globals.NextChar; i < Code.Length; i++)  // this will loop through each character of code and then the 
            {                                                       //following if/ else if statements decide what to do with it

                lookahead = i + 1;

                if (lookahead < Code.Length)
                {
                    if (words.Length > 0 && Code[i] == ' ') // this allows for blanks to separate reserved words, when there is a string in words and a space is next it calls
                    {
                        processToken(words, Code[i]);           // the process token function in order to process that token before moving to the next one
                        Globals.NextChar++;
                        return false;
                    }
                    else if (words == "" && Code[i] == ' ')  // this allows for the program to remove spaces from the tokens by incrementing over them
                    {

                    }
                    else if (words.Length > 0)
                    {
                        if (Char.IsLetter(words[0]) && (Char.IsLetter(Code[i]) || Char.IsDigit(Code[i]) || Code[i] == '_'))     // This part of the code creates id tokens
                        {
                            words += Code[i];
                        }
                        else if (Char.IsDigit(words[0]) && (Char.IsDigit(Code[i]) || (Code[i] == '.' && !words.Contains(".")))) // this part creates the num tokens
                        {
                            words += Code[i];
                        }
                        else
                        {
                            processToken(words, Code[i]);
                            return false;
                        }
                    }
                    else if (Char.IsLetterOrDigit(Code[i]) && words == "")     // if the words string is blank and a letter or number is next it will add it to the string
                    {
                        words += Code[i];
                    }
                    else if (words.Length > 0 && Code[i] == '\r')     // \r\n is the windows new line and carriage return values, when the next character is a new line the processTokens
                    {
                        processToken(words, Code[i]);                   // function is called for the current token
                    }
                    else if (words.Length > 0 && Code[i] == '\n')
                    {
                        // do nothing let it increment, the new line was flagged with the \r 
                    }
                    else if (words.Length > 0 && (Code[i] == '\t' || Code[i] == '\v' || Code[i] == '\f'))
                    {
                        // do nothing let it increment, this increments over any other whitespace characters
                    }
                    else if (words == "" && Code[i] == '\r')
                    {
                        i++;
                        Globals.NextChar++;     // increments the nextchar variable and the line number variable
                        Globals.LineNumber++;
                    }
                    else if (words == "" && Code[i] == '\n')
                    {
                        // do nothing let it increment
                    }
                    else if (words == "" && (Code[i] == '\t' || Code[i] == '\v' || Code[i] == '\f'))
                    {
                        // do nothing let it increment
                    }
                    else if (Code[i] == '-' && Code[lookahead] == '-' && words == "")  // this gets rid of comments by skipping over them, still increments the line number
                    {
                        while (i < Code.Length && Code[i] != '\r')
                        {
                            i++;
                            Globals.NextChar++;
                        }
                        if (i >= Code.Length)
                        {
                            Globals.Token = ((Tokens)35);
                            return true;
                        }

                        Globals.LineNumber++;
                    }
                    //Comments and Literals
                    else if (Code[i] == '"' && words == "") // this will detect and retrieve a literal token by seeing the start of it is a "
                    {
                        words += Code[i];
                        while (Code[i + 1] != '"' && Code[i + 1] != '\r')
                        {
                            words += Code[i + 1];
                            i++;
                            Globals.NextChar++;
                        }
                        if (Code[i + 1] == '"')
                        {
                            words += Code[i + 1];
                            i++;
                            Globals.NextChar++;
                        }
                        processToken(words, ' ');
                    }
                    else if (words == "")
                    {
                        words += Code[i];
                        while (Code[lookahead] == ' ' || Code[lookahead] == '\t') // this makes sure that if a lookahead is needed to process a token
                        {                                                       // such as for : vs := vs : = 
                            Globals.NextChar++;                                 // it will be the correct lookahead and will remove/skip any spaces
                            lookahead++;
                        }
                        processToken(words, Code[lookahead]);
                        Globals.NextChar++;
                        return false;
                    }
                    else
                    {
                        processToken(words, Code[i]);
                        return false;
                    }

                    Globals.NextChar++;
                }
                else
                {
                    if (words.Length > 0 && (Char.IsLetter(words[0]) && (Char.IsLetter(Code[i]) || Char.IsDigit(Code[i]) || Code[i] == '_')))           // some simple logic for when the end of file is approached and reached in order to process the whole file
                    {
                        words += Code[Code.Length - 1];
                        processToken(words, ' ');
                        return true;
                    }
                    if (words.Length > 0 && (Char.IsDigit(words[0]) && (Char.IsDigit(Code[i]) || (Code[i] == '.' && !words.Contains(".")))))
                    {
                        words += Code[Code.Length - 1];
                        processToken(words, ' ');
                        return true;

                    }
                    if (words.Length > 0)
                    {
                        processToken(words, ' ');
                        return false;
                    }
                    else
                    {
                        words += Code[Code.Length - 1];
                        processToken(words, ' ');
                        return true;
                    }



                }
            }

            return true;

        }


        // in one of the three times re-writing this program, i was going to use this function instead of the for loop
        // as i was having some trouble dealing with whitespace at first, then got stuck and found a better way
        // that made more sense to me, so this did not get used
        /*public char GetNextChar(string line)
        {
            char next;
            if(Globals.NextChar < line.Length)
            {
                nextChar = true;
                next = line[Globals.NextChar];
                Globals.NextChar++;
                return next;
                
            }
            else
            {
                nextChar = false;
                next = '\n';
                return next;
            }
        }*/


        // The second largest function in this program, this takes the string of a token and processes it to match a token, lexeme, and calculate and attribute (where applicable)
        // it takes in a string and the next character as a lookahead when necessary, it only modifys the Globals class variables
        public void processToken(string word, char lookahead)
        {
            string[] reswords = { "begin", "module", "constant", "procedure", "is", "if",
                "then", "else", "elsif", "while", "loop", "float", "integer", "char",
                "get" , "put", "end", "in", "out", "inout"};

            string[] relops = { "=", "/=", "<", "<=", ">", ">=" };      //initialization of all possible variables/ strings/ symbols that can be used in this language
            string[] addops = { "+", "-", "or" };
            string[] mulops = { "*", "/", "rem", "mod", "and" };
            string assignop = ":=";
            char leftpar = '(';
            char rightpar = ')';
            char comma = ',';
            char colon = ':';
            char semi = ';';
            char period = '.';
            char quote = '"';
            Boolean match = false;


            // This uses the current string and checks it against all of the reserved words
            // if it matches then it makes the match true so no other functions run and override it
            // and assigns a global varibe of token and lexeme to be printed back in the lexical analyzer
            for (int j = 0; j < reswords.Length; j++)
            {
                if (word.Equals(reswords[j], StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = ((Tokens)j);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // handles the not token
            if (!match && word.Length > 0)
            {
                if (word.Equals("not", StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = Tokens.nott;
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word.Equals("putln", StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = Tokens.putlnt;
                    Globals.Lexeme = word;
                    match = true;
                }
            }


            // This section uses the lookahead to check if the a symbol such as a relop or
            // assignop are actually the token instead of just a colon, /, <, or >
            if (!match && word.Length > 0)
            {
                /*if (word[0] == colon)
                {
                    Console.WriteLine(lookahead);
                }*/
                if (word[0] == colon && lookahead == '=')
                {
                    word += lookahead;
                    Globals.NextChar++;
                }
                else if (word[0] == '/' && lookahead == '=')
                {
                    word += lookahead;
                    Globals.NextChar++;
                }
                else if (word[0] == '<' && lookahead == '=')
                {
                    word += lookahead;
                    Globals.NextChar++;
                }
                else if (word[0] == '>' && lookahead == '=')
                {
                    word += lookahead;
                    Globals.NextChar++;
                }

            }

            // same as the reswords section but for relops
            for (int k = 0; k < relops.Length; k++)
            {
                if (word.Equals(relops[k], StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = ((Tokens)23);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // same as the reswords section but for addops
            for (int k = 0; k < addops.Length; k++)
            {
                if (word.Equals(addops[k], StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = ((Tokens)24);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // same as the reswords section but for mulops
            for (int k = 0; k < mulops.Length; k++)
            {
                if (word.Equals(mulops[k], StringComparison.OrdinalIgnoreCase))
                {
                    Globals.Token = ((Tokens)25);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // used for assignop
            if (!match && word.Length > 0)
            {
                if (word == assignop)
                {
                    Globals.Token = ((Tokens)26);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // any other symbols
            if (!match && word.Length == 1)
            {
                if (word[0] == leftpar)
                {
                    Globals.Token = ((Tokens)27);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == rightpar)
                {
                    Globals.Token = ((Tokens)28);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == comma)
                {
                    Globals.Token = ((Tokens)29);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == colon)
                {
                    Globals.Token = ((Tokens)30);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == semi)
                {
                    Globals.Token = ((Tokens)31);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == period)
                {
                    Globals.Token = ((Tokens)32);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (word[0] == quote)
                {
                    Globals.Token = ((Tokens)33);
                    Globals.Lexeme = word;
                    match = true;
                }
            }

            // Deals with literals
            if (!match && word.Length > 0)
            {
                if ((word.StartsWith("\"") && word.EndsWith("\"")) && word.Length <= 17)
                {
                    Globals.Token = ((Tokens)22);
                    Globals.Lexeme = word;
                    Globals.Literal = word;
                    match = true;
                }
                else if ((word.StartsWith("\"") && word.EndsWith("\"")) && word.Length > 17)
                {
                    Globals.Token = ((Tokens)22);
                    Globals.Lexeme = word.Substring(0, 17);
                    Globals.Literal = word;
                    match = true;
                }
            }

            // deals with numbers and converting them from strings into their attributes
            if (!match && word.Length > 0)
            {
                if (Char.IsLetter(word[0]) && word.Length <= 17)
                {
                    Globals.Token = ((Tokens)20);
                    Globals.Lexeme = word;
                    match = true;
                }
                else if (Char.IsDigit(word[0]) && !word.EndsWith("."))
                {
                    Globals.Token = ((Tokens)21);
                    Globals.Lexeme = word;
                    if (word.Contains('.'))
                    {
                        // convert to float
                        Globals.ValueF = float.Parse(word, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    else
                    {
                        // convert to int
                        Globals.Value = int.Parse(word);
                    }
                    match = true;
                }
            }

            // if it hasn't been matched to something yet then there was something wrong with the token
            // or invalid 
            if (!match && word.Length > 0)
            {
                Globals.Token = ((Tokens)34);
                Globals.Lexeme = word;
                Globals.Literal = "Error: malformed token on line " + Globals.LineNumber.ToString() + ".";
            }


        }

    }
}
