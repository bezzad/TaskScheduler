﻿using RestSharp;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices.SMS
{
    public class RahyabSmsService
    {
        private readonly string _userName;
        private readonly string _password;
        private readonly string _sendNumber;
        private readonly RestClient _client;

        public RahyabSmsService(string userName, string password, string sendNumber)
        {
            _client = new RestClient("http://linepayamak.ir/url/post");
            _userName = userName;
            _password = password;
            _sendNumber = sendNumber;
        }

        public void SendSms(string receiveNumber, string message)
        {
            var request = new RestRequest("SendSMS.ashx", Method.POST);
            request.AddParameter("from", _sendNumber);
            request.AddParameter("to", receiveNumber);
            request.AddParameter("text", message);
            request.AddParameter("password", _password);
            request.AddParameter("username", _userName);
            _client.Execute(request);
        }
    }
}