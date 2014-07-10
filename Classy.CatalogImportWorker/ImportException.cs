using System;

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
