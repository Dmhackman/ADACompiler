//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 4
// Due Date: 3/5/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called Procedure, this is a derrived class for entries into the
// symbol table.
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace HackmanAssignment7
{
    class Procedure : TableEntry
    {
        public int SizeOfLocal;
        public int SizeOfParameters;
        public int NumberOfParameters;
        //VarType ReturnType; is not used in ada
        public List<VarType> paramList = new List<VarType>();
        public List<ParamPassingMode> paramPassingModeList = new List<ParamPassingMode>();
        
    }
}
