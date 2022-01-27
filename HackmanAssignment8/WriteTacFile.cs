//*****************************************************************************************
// Name: Derek Hackman
// Class: CSC 446 (Compilers)
// Assignment: Assignment 7
// Due Date: 4/19/21
// Instructor: Hamer
//*****************************************************************************************
// Description: This file is called WriteTacFile, it will be used to create/open and write
// the TAC to this file for output on A7
//*****************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HackmanAssignment7
{
    class WriteTacFile
    {
        public void WriteFileInfo(string fileName, List<string> lines)
        {
            // gets the path to the file
            string filePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    foreach(string a in lines)
                    {
                        sw.Write(a);
                    }
                }

                //lines = File.ReadAllText(filePath);
                //return true;
            }
            else
            {
                //Console.WriteLine("Error: The file " + fileName + " could not be opened");
                //lines = "Error: File was not opened correctly";
                //return false;
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    foreach (string a in lines)
                    {
                        sw.Write(a);
                    }
                }
            }

        }
    }
}
