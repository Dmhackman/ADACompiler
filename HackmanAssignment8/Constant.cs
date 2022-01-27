//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 4
// Due Date: 3/5/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called Constant, this is a derrived class for entries into the
// symbol table.
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace HackmanAssignment7
{
    class Constant : TableEntry
    {
        public VarType TypeOfConstant;
        public int value;
        public float valueR;
    }
}
