using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureMLAPITest.Model
{
    public class RequestResponse:ObservableObject
    {
        private string _requestStr;

        public string RequestStr
        {
            get { return _requestStr; }
            set { _requestStr = value; RaisePropertyChanged("RequestStr"); }
        }

        private string _apiKey;

        public string ApiKey
        {
            get { return _apiKey; }
            set { _apiKey = value; RaisePropertyChanged("ApiKey"); }
        }

        private string _requestUrl;

        public string RequestUrl
        {
            get { return _requestUrl; }
            set { _requestUrl = value; RaisePropertyChanged("RequestUrl"); }
        }

        public ObservableCollection<string> StatusMessages { get; set; }
        private string _resultMessage;

        public string ResultMessage
        {
            get { return _resultMessage; }
            set { _resultMessage = value; RaisePropertyChanged("ResultMessage"); }
        }
        public RequestResponse()
        {
            StatusMessages = new ObservableCollection<string>();
        }

        public async Task ExecuteAsync()
        {
            StatusMessages.Clear();
            ResultMessage = "";
            try
            {
                using (var client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                    client.BaseAddress = new Uri(RequestUrl);

                    var obj = JsonConvert.DeserializeObject<RequestRootObject>(RequestStr);
                    HttpResponseMessage response = await client.PostAsJsonAsync("",obj);

                    if (response.IsSuccessStatusCode)
                    {
                        StatusMessages.Add(string.Format("The request success with status code: {0}", response.StatusCode));
                        string result = await response.Content.ReadAsStringAsync();
                        var responseObj = JsonConvert.DeserializeObject<ResponseRootObject>(result);
                        ResultMessage += string.Join("\t",responseObj.Results.output1.value.ColumnNames);
                        ResultMessage += "\n";
                        foreach (var v in responseObj.Results.output1.value.Values)
                        {
                            ResultMessage += string.Join("\t",v)+"\n";
                        }

                    }
                    else
                    {
                        StatusMessages.Add(string.Format("The request failed with status code: {0}", response.StatusCode));
                        StatusMessages.Add(response.Headers.ToString());
                        StatusMessages.Add(await response.Content.ReadAsStringAsync());

                    }
                }
            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }

        public class RequestInput1
        {
            public List<string> ColumnNames { get; set; }
            public List<List<string>> Values { get; set; }
        }

        public class RequestInputs
        {
            public RequestInput1 input1 { get; set; }
        }

        public class RequestGlobalParameters
        {
        }

        public class RequestRootObject
        {
            public RequestInputs Inputs { get; set; }
            public RequestGlobalParameters GlobalParameters { get; set; }
        }

        public class Value
        {
            public List<string> ColumnNames { get; set; }
            public List<string> ColumnTypes { get; set; }
            public List<List<string>> Values { get; set; }
        }

        public class ResponseOutput1
        {
            public string type { get; set; }
            public Value value { get; set; }
        }

        public class ResponseResults
        {
            public ResponseOutput1 output1 { get; set; }
        }

        public class ResponseRootObject
        {
            public ResponseResults Results { get; set; }
        }
    }


    
}
