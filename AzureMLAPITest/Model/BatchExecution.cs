using GalaSoft.MvvmLight;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureMLAPITest.Model
{
    public class BatchExecution:ObservableObject
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

        private string _storageAccountName;

        public string StorageAccountName
        {
            get { return _storageAccountName; }
            set { _storageAccountName = value; RaisePropertyChanged("StorageAccountName"); }
        }

        private string _storageAccountKey;

        public string StorageAccountKey
        {
            get { return _storageAccountKey; }
            set { _storageAccountKey = value; RaisePropertyChanged("StorageAccountKey"); }
        }

        private string _storageContainerName;

        public string StorageContainerName
        {
            get { return _storageContainerName; }
            set { _storageContainerName = value; RaisePropertyChanged("StorageContainerName"); }
        }

        private string _inputFileLocation;

        public string InputFileLocation
        {
            get { return _inputFileLocation; }
            set { _inputFileLocation = value; RaisePropertyChanged("InputFileLocation"); }
        }

        private string _inputBlobName;

        public string InputBlobName
        {
            get { return _inputBlobName; }
            set { _inputBlobName = value; RaisePropertyChanged("InputBlobName"); }
        }

        private string _jobId;

        public string JobId
        {
            get { return _jobId; }
            set { _jobId = value; RaisePropertyChanged("JobId"); }
        }
        public BatchExecution()
        {
            this.StatusMessages = new ObservableCollection<string>();
        }

        public async Task SubmitAsync()
        {
            try
            {
                StatusMessages.Clear();
                ResultMessage = "";
                if (!File.Exists(InputFileLocation))
                {
                    StatusMessages.Add("Input File Location not exist!");
                    return;
                }

                StatusMessages.Add("Uploading input file to blob storage...");
                var storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
                var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(StorageContainerName);
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(InputBlobName);
                blob.UploadFromFile(InputFileLocation, FileMode.Open);

                using (HttpClient client = new HttpClient())
                {
                    BatchScoreRequest request = new BatchScoreRequest()
                    {

                        Input = new AzureBlobDataReference()
                        {
                            ConnectionString = storageConnectionString,
                            RelativeLocation = blob.Uri.LocalPath
                        },

                        GlobalParameters = new Dictionary<string, string>()
                        {
                        }
                    };

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                    StatusMessages.Add("Submitting the job...");

                    // submit the job
                    var response = await client.PostAsJsonAsync(RequestUrl + "?api-version=2.0", request);
                    if (!response.IsSuccessStatusCode)
                    {
                        StatusMessages.Add(await response.Content.ReadAsStringAsync());
                        return;
                    }

                    this.JobId = await response.Content.ReadAsAsync<string>();
                    StatusMessages.Add(string.Format("Submitting the job completed, job id is {0}", this.JobId));

                }
            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }

        public async Task StartAsync()
        {
            StatusMessages.Clear();
            ResultMessage = "";
            try{
            // set a time out for polling status
            const int TimeOutInMilliseconds = 120 * 1000; // Set a timeout of 2 minutes

            using (var client =new HttpClient()){

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                // start the job
                StatusMessages.Add("Starting the job...");
                var response = await client.PostAsync(RequestUrl + "/" + JobId + "/start?api-version=2.0", null);
                if (!response.IsSuccessStatusCode)
                {
                    StatusMessages.Add(await response.Content.ReadAsStringAsync());
                    return;
                }

                string jobLocation = RequestUrl + "/" + JobId + "?api-version=2.0";
                Stopwatch watch = Stopwatch.StartNew();
                bool done = false;
                while (!done)
                {
                    StatusMessages.Add("Checking the job status...");
                    response = await client.GetAsync(jobLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        StatusMessages.Add(await response.Content.ReadAsStringAsync());
                        return;
                    }

                    BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
                    if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
                    {
                        done = true;
                        StatusMessages.Add(string.Format("Timed out. Deleting job {0} ...", JobId));
                        await client.DeleteAsync(jobLocation);
                    }
                    switch (status.StatusCode)
                    {
                        case BatchScoreStatusCode.NotStarted:
                            StatusMessages.Add(string.Format("Job {0} not yet started...", JobId));
                            break;
                        case BatchScoreStatusCode.Running:
                            StatusMessages.Add(string.Format("Job {0} running...", JobId));
                            break;
                        case BatchScoreStatusCode.Failed:
                            StatusMessages.Add(string.Format("Job {0} failed!", JobId));
                            StatusMessages.Add(string.Format("Error details: {0}", status.Details));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Cancelled:
                            StatusMessages.Add(string.Format("Job {0} cancelled!", JobId));
                            done = true;
                            break;
                        case BatchScoreStatusCode.Finished:
                            done = true;
                            StatusMessages.Add(string.Format("Job {0} finished!", JobId));

                            foreach (var result in status.Results)
                            {
                                var location = result.Value;
                                ResultMessage += string.Format("The result '{0}' is available at the following Azure Storage location:", result.Key) + "\n";
                                ResultMessage += string.Format("BaseLocation: {0}", location.BaseLocation) + "\n";
                                ResultMessage += string.Format("RelativeLocation: {0}", location.RelativeLocation) + "\n";
                                ResultMessage += string.Format("SasBlobToken: {0}", location.SasBlobToken) + "\n";
                                

                                var dialog = new SaveFileDialog();
                                dialog.Filter = "CSV file(*.csv)|*.csv|All File(*.*)|*.*";
                                if (dialog.ShowDialog() == true)
                                {
                                    var sasUrl = location.BaseLocation + location.RelativeLocation + location.SasBlobToken;
                                    var resultBlob = new CloudBlockBlob(new Uri(sasUrl));
                                    
                                    resultBlob.DownloadToFile(dialog.FileName, FileMode.Create);


                                    ResultMessage += string.Format("Result Download Complete {0}",dialog.FileName) + "\n";
                                }
                                ResultMessage += "\n";
                            }

                            
                            break;
                    }

                    if (!done)
                    {
                        await Task.Delay(1000); // Wait one second
                    }
                }
            }
            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }

        public async Task DeleteAsync()
        {
            StatusMessages.Clear();
            ResultMessage = "";
            try{
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                // start the job
                StatusMessages.Add("Deleting the job...");
                var response = await client.DeleteAsync(RequestUrl + "/" + JobId);
                if (!response.IsSuccessStatusCode)
                {
                    StatusMessages.Add(await response.Content.ReadAsStringAsync());
                    return;
                }
                StatusMessages.Add("Job Delete Completed");
            }

            }
            catch (Exception e)
            {
                StatusMessages.Add(e.Message);
            }
        }

        public void SelectInputFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "CSV file(*.csv)|*.csv|All File(*.*)|*.*";
            if (dialog.ShowDialog() == true) {
                this.InputFileLocation = dialog.FileName;
            }
        }
    }

    

    public enum BatchScoreStatusCode
    {
        NotStarted,
        Running,
        Failed,
        Cancelled,
        Finished
    }

    public class BatchScoreStatus
    {
        // Status code for the batch scoring job
        public BatchScoreStatusCode StatusCode { get; set; }


        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Results { get; set; }

        // Error details, if any
        public string Details { get; set; }
    }

    public class BatchScoreRequest
    {
        public AzureBlobDataReference Input { get; set; }
        public IDictionary<string, string> GlobalParameters { get; set; }
    }
}
