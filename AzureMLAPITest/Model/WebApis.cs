using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMLAPITest.Model
{
    public class WebApis:ObservableObject
    {
        RequestResponse _requestResponseApi;

        public RequestResponse RequestResponseApi
        {
            get { return _requestResponseApi; }
            set { _requestResponseApi = value; RaisePropertyChanged("RequestResponseApi"); }
        }

        BatchExecution _batchExectionApi;

        public BatchExecution BatchExectionApi
        {
            get { return _batchExectionApi; }
            set { _batchExectionApi = value; RaisePropertyChanged("BatchExectionApi"); }
        }

        Retrain _retrainApi;

        public Retrain RetrainApi
        {
            get { return _retrainApi; }
            set { _retrainApi = value; RaisePropertyChanged("RetrainApi"); }
        }

        AddEndpoint _addEndpointApi;

        public AddEndpoint AddEndpointApi
        {
            get { return _addEndpointApi; }
            set { _addEndpointApi = value; RaisePropertyChanged("AddEndpointApi"); }
        }
        public WebApis()
        {
            this._requestResponseApi = new RequestResponse();
            this._batchExectionApi = new BatchExecution();
            this._retrainApi = new Retrain();
            this._addEndpointApi = new AddEndpoint();
        }
    }
}
