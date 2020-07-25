#define PRINT_TO_FILE

using Destr.Codegen;
using System.Collections.Generic;
using Destr.Codegen.Source;

#if PRINT_TO_FILE
using System.IO;
using System.Text;
#else
using System;
#endif

namespace Destr.Codegen
{
    public abstract class WriterCodeGenerator : ICodeGenerator
    {
        protected abstract IEnumerable<(string, IEnumerable<string>)> GetSources();

        public void Generate()
        {
            foreach((string file, IEnumerable<string> source) in GetSources())
            {
#if PRINT_TO_FILE
                //using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Write);
                using var writer = new StreamWriter(file);
                foreach (var line in source)
                    writer.WriteLine(line);
#else
                Console.WriteLine("File: " + file);
                foreach (var line in source)
                    Console.WriteLine(line);
#endif
            }
        }
    }
}
