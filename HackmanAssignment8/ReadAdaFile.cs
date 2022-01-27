//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 3
// Due Date: 2/3/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called ReadAdaFile, it will be used to open and read the Ada
// file that has the code to be broken into tokens. It will use a file name passes from 
// the command line or as the user to input a file name to be opened if there is nothing 
// in the command line. 
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HackmanAssignment7
{
    public class ReadAdaFile
    {
        public Boolean GetFileInfo(string fileName, out string lines)
        {
            // gets the path to the file
            string filePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, fileName);

            if (File.Exists(filePath))
            {
                lines = File.ReadAllText(filePath);
                return true;
            }
            else
            {
                Console.WriteLine("Error: The file " + fileName + " could not be opened");
                lines =  "Error: File was not opened correctly" ;
                return false;

            }

        }
    }
}
