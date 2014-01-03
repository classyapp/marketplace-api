using Classy.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Repository
{
    public class MongoTransactionRepository : ITransactionRepository
    {
        private MongoCollection<Transaction> TransactionsCollection;

        public MongoTransactionRepository(MongoDatabase db)
        {
            TransactionsCollection = db.GetCollection<Transaction>("transactions");
        }
    
        public string Save(Transaction transaction)
        {
            try
            {
                TransactionsCollection.Save(transaction);
                return transaction.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Transaction GetById(string appId, string transactionId)
        {
            try
            {
                var transaction = TransactionsCollection.FindOne(Query<Transaction>.Where(x => x.Id == transactionId && x.AppId == appId));
                return transaction;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
