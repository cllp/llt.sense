using System;
using Elasticsearch.Net;

namespace LLT.Sense.ElasticClient
{
    public class ElasticPipeline
    {
        private static ElasticLowLevelClient lowlevelClient;

        public ElasticPipeline()
        {
            var settings = new ConnectionConfiguration(new Uri("https://06z771.stackhero-network.com:9200"))
           .RequestTimeout(TimeSpan.FromMinutes(2));

            lowlevelClient = new ElasticLowLevelClient(settings);

            //var result = Post("messages", "1", message);



        }

        public byte[] Post<T>(string index, string id, T data)
        {
            var ndexResponse = lowlevelClient.Index<BytesResponse>(index, id, PostData.Serializable(data));
            byte[] responseBytes = ndexResponse.Body;

            //var asyncIndexResponse = await lowlevelClient.IndexAsync<StringResponse>("people", "1", PostData.Serializable(data));
            //string responseString = asyncIndexResponse.Body;

            return responseBytes;

        }
    }
}
