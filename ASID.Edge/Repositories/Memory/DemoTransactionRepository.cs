using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Repositories.Memory
{
    public class DemoTransactionRepository
    {
        private readonly List<PUBodyTransactionHistoryItem> _items = new();

        public IReadOnlyList<PUBodyTransactionHistoryItem> Items => _items;

        public void Add(PUBodyTransactionHistoryItem item)
        {
            _items.Insert(0, item);   // newest first
        }
    }
}
