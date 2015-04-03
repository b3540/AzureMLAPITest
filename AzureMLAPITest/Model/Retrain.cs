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
    public class Retrain:ObservableObject
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

        private string _containerUrl;

        public string ContainerUrl
        {
            get { return _containerUrl; }
            set { _containerUrl = value; RaisePropertyChanged("ContainerUrl"); }
        }

        private string _relativeFilePath;

        public string RelativeFilePath
        {
            get { return _relativeFilePath; }
            set { _relativeFilePath = value; RaisePropertyChanged("RelativeFilePath"); }
        }

        private string _sasToken;

        public string SasToken
        {
            get { return _sasToken; }
            set { _sasToken = value; RaisePropertyChanged("SasToken"); }
        }

        private string _retrainModelName;

        public string RetrainModelName
        {
            get { return _retrainModelName; }
            set { _retrainModelName = value; RaisePropertyChanged("RetrainModelName"); }
        }

        public Retrain()
        {
            StatusMessages = new ObservableCollection<string>();
        }

        public async Task ExecuteAsync()
        {
            try
            {

                var resourceLocations = new ResourceLocations()
                {
                    Resources = new ResourceLocation[]
                {
                    new ResourceLocation()
                    {
                        Name=RetrainModelName,
                        Location=new AzureBlobDataReference()
                        {
                            BaseLocation=this.ContainerUrl,
                            RelativeLocation=this.RelativeFilePath,
                            SasBlobToken=this.SasToken
                        }
                    }
                }
                };
                StatusMessages.Add("Starting Retrain...");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), RequestUrl))
                    {
                        var json = JsonConvert.SerializeObject(resourceLocations);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var result = await client.SendAsync(request);

                        this.ResultMessage = await result.Content.ReadAsStringAsync();
                        this.StatusMessages.Add(string.Format("Retrain Completed! {0}",result.StatusCode));
                    }
                }
                
            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }
    }

    public class ResourceLocations
    {
        public ResourceLocation[] Resources { get; set; }
    }

    public class ResourceLocation
    {
        public string Name { get; set; }
        public AzureBlobDataReference Location { get; set; }

    }

    
}
