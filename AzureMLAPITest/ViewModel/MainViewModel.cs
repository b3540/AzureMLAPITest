using AzureMLAPITest.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureMLAPITest.ViewModel
{
    
    public class MainViewModel : ViewModelBase
    {
        private WebApis _apis;

        public WebApis Apis
        {
            get { return _apis; }
            set { _apis = value; RaisePropertyChanged("Apis"); }
        }
        public RelayCommand ExecuteRequestResponseCommand { get; set; }

        public RelayCommand SubmitBatchExectionCommand { get; set; }
        public RelayCommand StartBatchExectionCommand { get; set; }
        public RelayCommand DeleteBatchExectionCommand { get; set; }

        public RelayCommand SelectBatchInputFileCommand { get; set; }

        public RelayCommand ExecuteRetrainCommand { get; set; }

        public RelayCommand ExecuteAddEndpointCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        public RelayCommand LoadCommand { get; set; }

        private bool _isExecting;

        public bool IsExecting
        {
            get { return _isExecting; }
            set { _isExecting = value; RaisePropertyChanged("IsExecting"); }
        }
        public MainViewModel()
        {

            IsExecting = false;
            this._apis = new WebApis();
            this.ExecuteRequestResponseCommand = new RelayCommand(async() =>
            {
                IsExecting = true;
                await this.Apis.RequestResponseApi.ExecuteAsync();
                IsExecting = false;
            });

            this.SubmitBatchExectionCommand = new RelayCommand(async() =>
            {
                IsExecting = true;
                await this.Apis.BatchExectionApi.SubmitAsync();
                IsExecting = false;
            });

            this.StartBatchExectionCommand = new RelayCommand(async () =>
            {
                IsExecting = true;
                await this.Apis.BatchExectionApi.StartAsync();
                IsExecting = false;
            });

            this.DeleteBatchExectionCommand = new RelayCommand(async () =>
            {
                IsExecting = true;
                await this.Apis.BatchExectionApi.DeleteAsync();
                IsExecting = false;
            });

            this.SelectBatchInputFileCommand = new RelayCommand(() =>
            {
                this.Apis.BatchExectionApi.SelectInputFile();
            });

            this.ExecuteRetrainCommand = new RelayCommand(async() =>
            {
                IsExecting = true;
                await this.Apis.RetrainApi.ExecuteAsync();
                IsExecting = false;
            });

            this.ExecuteAddEndpointCommand = new RelayCommand(async() =>
            {
                IsExecting = true;
                await this.Apis.AddEndpointApi.ExecuteAsync();
                IsExecting = false;
            });

            this.SaveCommand = new RelayCommand(() =>
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "APIProject file(*.apiproj)|*.apiproj";
                if (dialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(dialog.FileName, false, Encoding.ASCII))
                    {
                        var json = JsonConvert.SerializeObject(this.Apis);
                        writer.Write(json);
                    }
                }
            });

            this.LoadCommand = new RelayCommand(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "APIProject file(*.apiproj)|*.apiproj";
                if (dialog.ShowDialog() == true)
                {
                    using (var reader = new StreamReader(dialog.FileName))
                    {
                        var str = reader.ReadToEnd();
                        this.Apis = JsonConvert.DeserializeObject<WebApis>(str);
                    }
                }
            });
        }

        
    }
}