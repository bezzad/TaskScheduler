using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.Owin.Hosting;
using NLog;
using Topshelf.Runtime;

namespace Hasin.Taaghche.Probes
{
    public class Service
    {
        private readonly ILogger _logger;
        private readonly int _port;
        private Thread _service;
        private HostSettings _settings;
        private IDisposable _webServer;

        public Service(HostSettings settings, int port)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _settings = settings;
            _port = port;
        }

        public bool Start()
        {
            var header = @"
 _____                 _          _             
|_   _|               | |        | |            
  | | __ _  __ _  __ _| |__   ___| |__   ___    
  | |/ _` |/ _` |/ _` | '_ \ / __| '_ \ / _ \   
  | | (_| | (_| | (_| | | | | (__| | | |  __/   
  \_/\__,_|\__,_|\__, |_| |_|\___|_| |_|\___|   
                  __/ |                         
                 |___/                          
                ______          _               
                | ___ \        | |              
                | |_/ / __ ___ | |__   ___  ___ 
                |  __/ '__/ _ \| '_ \ / _ \/ __|
                | |  | | | (_) | |_) |  __/\__ \
                \_|  |_|  \___/|_.__/ \___||___/
                                                  
";

            _logger.Info(header);

            try
            {
                _service = new Thread(() =>
                {
                    // Start OWIN host 
                    _webServer = WebApp.Start<Startup>($"http://+:{_port}");

                    _logger.Info($"Server running on port {_port}");
                });
               // _service.SetApartmentState(ApartmentState.STA); // cause to run slower
                _service.Priority = ThreadPriority.AboveNormal;
                _service.Start();

                return true;
            }
            catch (Exception exp)
            {
                if (exp.InnerException is HttpListenerException)
                    throw new FieldAccessException(
                        "Access to listen this port is denied, please run as administrator!", exp.InnerException);

                _logger.Fatal(exp);
                Debugger.Break();
                return false;
            }
        }
        

        public bool Stop()
        {
            try
            {
                _logger.Info("request to service stopping...");
                _webServer?.Dispose();
                _service?.Join();
            }
            catch (Exception e)
            {
               _logger.Fatal(e);
            }
            
            return true;
        }
        
    }
}