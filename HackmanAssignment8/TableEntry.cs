//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 4
// Due Date: 3/5/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called table entry, it is the base class for entries into the
// symbol table.
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace HackmanAssignment7
{
    class TableEntry
    {
        public Tokens token;
        public string Lexeme;
        public int depth;
        public EntryType TypeOfEntry;
    }
}
