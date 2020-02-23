using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WpfData
{
    public class DataRequester
    {
        List<string> requests;
        HttpClient httpClient;
        List<Task<string>> tasks = new List<Task<string>>();

        public DataRequester (params string[] requests)
        {
            if(requests == null)
                throw new ArgumentNullException(nameof(requests));
            this.requests = requests.ToList();
            httpClient = new HttpClient();
        }

        public void GetReady ( )
        {
            foreach ( string request in requests )
            {
                tasks.Add(httpClient.GetStringAsync(request));
            }
        }

        public bool IsReady ( )
        {
            foreach(Task task in tasks )
            {
                if ( !task.IsCompleted )
                    return false;
            }
            return true;
        }

        public List<string> Get ( )
        {
            Task.WaitAll(tasks.ToArray());

            var ret = tasks.Select(t => t.Result).ToList();
            tasks.Clear();
            return ret;
        }
    }
}
