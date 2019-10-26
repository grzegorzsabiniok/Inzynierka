using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDL.Parser
{
    class ParserController
    {
        private List<string> separators = new List<string>()
        {
            " ","\n","\r","\t",",","{","}","(",")",";","<=","!"
        };

        private List<string> worthless = new List<string>()
        {
            " ","\n","\r","\t",""
        };

        private List<string> keywords = new List<string>()
        {
            "module","gates"
        };

        private List<Token> tokens;

        private List<File> files;

        public ParserController(List<File> files)
        {
            this.files = files;
            Token.Keywords = keywords;
        }

        public void Start(Action OnComplete)
        {
            tokens = new List<Token>();
            files.ForEach(x => ProcessFile(x));

            //cleaning
            tokens.RemoveAll(x => worthless.Contains(x.Value));

            tokens.ForEach(x => Console.WriteLine(x));
        }

        private void ProcessFile(File file)
        {

            string[] lines = file.Value.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                ProcessLine(lines[i], i, file);
            }
        }

        private void ProcessLine(string line, int lineNumber, File file)
        {
            string saver = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (FindSeparator(line, i, "//"))
                {
                    return;
                }

                var findedSeparator = FindSeparator(line, i);

                if (findedSeparator != null)
                {
                    tokens.Add(new Token()
                    {
                        Value = saver,
                        Line = lineNumber,
                        Position = i - saver.Length,
                        File = file
                    });

                    findedSeparator.Line = lineNumber;
                    findedSeparator.File = file;

                    tokens.Add(findedSeparator);

                    saver = "";
                    i += findedSeparator.Value.Length - 1;
                }
                else
                {
                    saver += line[i];
                }
            }
        }
        private Token FindSeparator(string input, int position)
        {

            separators = separators.OrderByDescending(x => x.Length).ToList();

            foreach (var x in separators)
            {
                if (FindSeparator(input, position, x))
                {
                    return new Token()
                    {
                        Type = Token.TokenType.Separator,
                        Value = x,
                        Position = position
                    };
                }
            }
            return null;
        }
        private bool FindSeparator(string input, int position, string word)
        {
            if (input.Length > position + word.Length - 1)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (input[position + i] != word[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
