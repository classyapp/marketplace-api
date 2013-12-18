using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ITransactionRepository
    {
        Transaction GetById(string appId, string transactionId);
        string Save(Transaction transaction);
    }
}
