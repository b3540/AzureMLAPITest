using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureMLAPITest.Model
{
    public class AddEndpoint:ObservableObject
    {
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

        private string _endpointName;

        public string EndpointName
        {
            get { return _endpointName; }
            set { _endpointName = value; RaisePropertyChanged("EndpointName"); }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); }
        }

        public AddEndpoint()
        {
            StatusMessages = new ObservableCollection<string>();
        }

        public async Task ExecuteAsync()
        {
            try
            {

                var endpoint = new WebServiceEndpoint()
                {
                    Description=this.Description,
                    ThrottleLevel="Low"
                };
                StatusMessages.Add("Starting New Endpoint...");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PUT"), RequestUrl))
                    {
                        var json = JsonConvert.SerializeObject(endpoint);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var result = await client.SendAsync(request);

                        this.ResultMessage = await result.Content.ReadAsStringAsync();
                        this.StatusMessages.Add(string.Format("Endpoint Add Completed! {0}",result.StatusCode));
                    }
                }
                
            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }
    }

    public class WebServiceEndpoint
    {
        public string Description { get; set; }

        public string ThrottleLevel { get; set; }
        public string MaxConcurrentCalls { get; set; }
    }

    
}
