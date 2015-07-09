using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibGxFormat.ModelLoader
{
    /// <summary>Utility class to parse .OBJ and .MTL files easily.</summary>
    class ObjMtlParser : IDisposable
    {
        /// <summary>The current line number in the file (one-based).</summary>
        public int LineNum { get; private set; }

        /// <summary>The line currently being parsed, or null if no line is available.</summary>
        private string line;
        /// <summary>The index of the next character inside the line being parsed.</summary>
        private int linePosition;

        /// <summary>The path of the file being parsed.</summary>
        private string filePath;
        /// <summary>Instance of StreamReader being used to read the input stream.</summary>
        private StreamReader fileStream;

        /// <summary>Create a new .OBJ / .MTL parser.</summary>
        /// <param name="filePath">The path of the file to parse.</param>

        public ObjMtlParser(string filePath)
        {
            this.filePath = filePath;
            this.fileStream = new StreamReader(filePath);

            this.LineNum = 0;
            this.line = null;
            this.linePosition = 0;
        }

        ~ObjMtlParser()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clear up managed resources
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }
            }
        }

        /// <summary>
        /// Read the next line from the input stream.
        /// </summary>
        /// <returns>true if a new line could be read, false otherwise.</returns>
        public bool ReadNextLine()
        {
            LineNum++;
            line = fileStream.ReadLine();
            linePosition = 0;
            return line != null;
        }

        /// <summary>
        /// Returns true if the line position is at the end of the line.
        /// </summary>
        public bool IsEndOfLine
        {
            get
            {
                if (line == null)
                    throw new InvalidOperationException("No line is available.");
                return linePosition == line.Length;
            }
        }

        /// <summary>
        /// Get the next character from the input stream without consuming it.
        /// </summary>
        /// <returns>The next character from the input stream.</returns>
        public char PeekCharacter()
        {
            if (line == null)
                throw new InvalidOperationException("No line is available.");
            if (IsEndOfLine)
                throw new InvalidOperationException("Cursor is at the end of the line.");

            return line[linePosition];
        }

        /// <summary>Returns a string containing all characters between the current line position and the end of the line.</summary>
        /// <returns>A string containing all characters between the current line position and the end of the line.</returns>
        public string ReadRestOfLine()
        {
            string restOfLine = line.Substring(linePosition);
            linePosition = line.Length;
            return restOfLine;
        }

        /// <summary>
        /// Advance the current line position to the next non-whitespace character.
        /// </summary>
        /// <returns>true if the line position could be updated, false if the end of the line has been reached.</returns>
        public bool AdvanceToNextNonWhiteSpace()
        {
            if (line == null)
                throw new InvalidOperationException("No line is available.");

            for (; linePosition < line.Length; linePosition++)
            {
                if (!char.IsWhiteSpace(line[linePosition]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Advance the current line position to the next whitespace character.
        /// </summary>
        /// <returns>true if the line position could be updated, false if the end of the line has been reached.</returns>
        public bool AdvanceToNextWhiteSpace()
        {
            if (line == null)
                throw new InvalidOperationException("No line is available.");

            for (; linePosition < line.Length; linePosition++)
            {
                if (char.IsWhiteSpace(line[linePosition]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Read the next word (string of characters surrounded by whitespace) in the current line.
        /// The current line position is set to the end of the word.
        /// </summary>
        /// <returns>The next word in the file, or null if there are no more words in the line.</returns>
        public string GetNextWord()
        {
            if (line == null)
                throw new InvalidOperationException("No line is available.");

            if (!AdvanceToNextNonWhiteSpace())
                return null;

            int wordStartIndex = linePosition;
            AdvanceToNextWhiteSpace();

            return line.Substring(wordStartIndex, linePosition - wordStartIndex);
        }

        /// <summary>
        /// Gets a string representing the position (file name and line) inside the currently loaded file.
        /// </summary>
        /// <returns>A string representing the position (file name and line).</returns>
        public string GetFilePositionStr()
        {
            return string.Format("On line {0} of file {1}", LineNum, filePath);
        }
    }
}
