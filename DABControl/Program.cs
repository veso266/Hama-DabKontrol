using System;
using System.Threading; //To distribute load efficiently
using System.IO; //To check for URadioServer.cfg

using DABControl.GlobalTuners; //Our RadioControl Hook

using URadioServer; //Our GT Interface
using URadioServer.Radio; //Our GT Interface

using System.Diagnostics;


namespace DABControl
{
    class Program : IServerFrontend
    {
        public bool IsClosing { get; private set; }
        private HamaParse hama;
        private ServerManager server;
        public bool RadioStatus
        {
            get
            {
                //return hama.RadioStatus();
                return true;
            }
        }

        internal void StartPlugin()
        {
            if (this.server != null || this.IsClosing)
            {
                return;
            }
            this.server = new ServerManager();
            this.server.OnGetReceiver += this.OnGetReceiver;
            new Thread(new ParameterizedThreadStart(this.ThreadProc)).Start();
            //new Thread(new ParameterizedThreadStart(this.RadioHeartBeat)).Start(); //To check for Radio Heart Beat
        }

        //To put it into 2 thread
        private void ThreadProc(object state)
        {
            this.server.Run(this);
        }
        //Radio HeartBeat thread
        private void RadioHeartBeat(object state)
        {
            while (true)
            {
                hama.RadioStatus();
            }
        }

        //It checks on what plugin you have
        private void OnGetReceiver(IServerManager server, uint id, string type, ISettingsCollection configuration, ref IReceiver receiver)
        {
            if (type == "SDRSharp")
            {
                int PIN = 1234; //Our PIN
                HamaParse hama = new HamaParse("192.168.88.11", PIN);
                receiver = new GTRadio(this, hama);
            }
        }
        
        //When Closing
        public void Close()
        {
            this.IsClosing = true;
            if (this.server != null)
            {
                this.server.CompleteShutdown();
            }
        }

        //To check Status
        public string StatusString
        {
            get
            {
                if (this.server == null)
                {
                    return "Not running";
                }
                string text;
                if (((IServerManager)this.server).HubConnected)
                {
                    text = "Connected to hub, ";
                }
                else
                {
                    text = "Not connected to hub, ";
                }
                int status = ((IServerManager)this.server).Status;
                if (status != 0)
                {
                    if (status != 1)
                    {
                        switch (status)
                        {
                            case 10:
                                text += "starting...";
                                break;
                            case 11:
                                text += "stopped";
                                break;
                            case 12:
                                text += "error";
                                break;
                            case 13:
                                text += "receiver offline";
                                break;
                            default:
                                text += "unknown";
                                break;
                        }
                    }
                    else
                    {
                        text += "online";
                    }
                }
                else
                {
                    text += "offline";
                }
                return text;
            }
        }

        //To EXIT
        void IServerFrontend.Exit()
        {
            //Application.Exit(); //Didn't implement EXIT yet :(
        }

        //To restart
        void IServerFrontend.Restart(bool emergency)
        {
            //Application.Restart(); //DIdn't implement Auto Restarting yet :(
            //Debug.WriteLine("Restart Requested");
        }
        void IServerFrontend.StatusChanged()
        {
           //We don't need this
        }

        //What kind of front-end we have (it can be service, console (opens window), or GUI (writes to Log.txt))
        ServerFrontendType IServerFrontend.Type
        {
            get
            {
                return ServerFrontendType.GUI;
            }
        }

        static void Main(string[] args)
        {
            Program prog = new Program();

            if (File.Exists("URadioServer.cfg"))
            {
                prog.StartPlugin();
            }
            else
            {
                Console.WriteLine("URadioServer.cfg not found, generate one, than put it in the same dir as DABControl");
            }
            Console.ReadLine();
        }
    }
}