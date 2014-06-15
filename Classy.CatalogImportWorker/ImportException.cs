using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.CatalogImportWorker
{
    class ImportException : Exception
    {
        public ImportException(string message, int line)
            : base(message)
        {
            Line = line;
        }

        public int Line { get; set; }
    }
}
