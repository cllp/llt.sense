using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Netmore
{
    public class StorageTableResult<T> where T : TableEntity
    {
        public string NextPartitionKey { get; set; }
        public string NextRowKey { get; set; }
        public List<T> Messages { get; set; }
    }

}
