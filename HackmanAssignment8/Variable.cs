//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 4
// Due Date: 3/5/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called Variable, this is a derrived class for entries into the
// symbol table.
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace HackmanAssignment7
{
    class Variable : TableEntry
    {
        public VarType TypeOfVariable;
        public int offset;
        public int size;
        public bool param;
        public ParamPassingMode paramMode;

    }
}
