using System;
using System.Collections.Generic;

using URadioServer; //Our GT Interface
using URadioServer.Radio; //Our GT Interface

using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;

namespace DABControl.GlobalTuners
{
    partial class GTRadio : IReceiver, IHasAudioRecorder
    {
        /* Variable declarations go here */
        private Program HB;
        private HamaParse hama;
        public string RadioText { get; private set; }
        public event EventHandler<RadioTextChangedEventArgs> RadioTextChanged;
        private byte rmode = 1; //for Our Tune Buttons
        private byte rfilter = 15;
        private byte rSignal = 0xFF; //For our initial signal

        //Display
        int count = 0;
        protected void RDS_Click()
        {
            count++;
            if (count > 1)
            {
                count = 0;
            }
        }
        //Display

        /* Variable declarations go here */
        public GTRadio(Program program, HamaParse _hama)
        {
            this.HB = program;
            this.hama = _hama;
        }

        /* We have to declare those */
        AudioRecorder IHasAudioRecorder.AudioRecorder //To force Default Audio Source from Recorder.NAudio.dll
        {
            get
            {
                return null;
            }
        }

        byte[] IReceiver.AvailableFilters //This is for filters (25khz, 50khz, 120khz, 180khz, etc)
        {
            get
            {
                return GTRadio.availableFilters;
            }
        }
        byte[] IReceiver.AvailableModes //This is for modes (we will use this for our Tune UP/Tune Down button since Interface is not designed for DAB just yet)
        {
            get
            {
                return GTRadio.availableModes;
            }
        }
        event EventHandler<AvailableModesChangedEventArgs> IReceiver.AvailableModesChanged //Do we even need this (it probably handles when you change mode)?
        {
            add
            {
            }
            remove
            {
            }
        }
        ReceiverCapabilities IReceiver.Capabilities //This is for what your reciever can do (AGC, Stereo, NoiseBlanker) we only need DISPlay functionality
        {
            get
            {
                return this.receiverCapabilities;
            }
        }
        void IReceiver.Configure(ISettingsCollection Config) //Configuration
        {
        }

        /* We have to declare those */
        Result IReceiver.ChangeRadioText() //Display
        {
            return Result.OK;
        }
        Result IReceiver.GetStatus(ref bool Online, ref bool SquelchOpen, ref byte Signal)
        {
            SquelchOpen = false;
            Online = this.HB.RadioStatus;
            Signal = getSignal();
            getRDS(count); //Display
            return Result.OK;
        }
        void IReceiver.Open() //Start the radio
        {
            this.hama.StartRadio();
        }
        void IReceiver.Close() //When we stop our service
        {
            if (!this.HB.IsClosing)
            {
                this.hama.StopRadio();
            }
        }

        public Result SetOption(ReceiverOptions Option, ref byte Value)
        {
            //Debug.WriteLine(Option);
            switch (Option)
            {
                case ReceiverOptions.Volume:
                    //this.audiosource.SetVolume((int)Value);
                    return Result.OK;
                case ReceiverOptions.RadioText: //Display
                    RDS_Click(); //so we know that DSP Button was clicked
                    return Result.OK;
                //to get RadioText to work
                //to get SignalMeter working
                case ReceiverOptions.Signal:
                    this.rSignal = (byte)Value;
                    return Result.OK;
                default:
                    if (Option != ReceiverOptions.StereoMono)
                    {
                        return Result.ERR_Func;
                    }

                    if (Value > 1)
                    {
                        Value = 1;
                    }
                    return Result.OK;
            }
        }
        Result IReceiver.SetTuning(ref ulong frequency, ref byte mode, ref byte filter) //for tunning
        {
            if (frequency != 0UL)
            {
                say("You cannot tune like this :(", 120);
                //Tune(frequency);
                //Debug.WriteLine(frequency); //3000000

            }
            if (mode != 0 || filter != 0)
            {
                switch (mode)
                {
                    case 1:
                        say("Home", 90);
                        say("Home sweat", 90);
                        say("Home sweat home", 90);
                        count = 0;
                        //this.rmode = mode; //To keep button off
                        break;
                    case 2:
                        this.hama.TuneUP();
                        //this.rmode = mode; //To keep button off
                        break;
                    case 3:
                        this.hama.TuneDOWN();
                        //this.rmode = mode; //To keep button off
                        break;
                }
                if (mode != 0)
                {
                    //this.SetOption(ReceiverOptions.Squelch, ref this.rsquelch);
                    //this.SetOption(ReceiverOptions.Signal, ref this.rSignal);
                }
                mode = this.rmode;
                /*
                if (filter == 0)
                {
                    filter = this.rfilter;
                }
                if (filter < 10)
                {
                    filter = 10;
                }
                if (this.rmode == 2 && filter > 250)
                {
                    filter = 250;
                }
                if (this.rmode == 2 && filter == 230)
                {
                    filter = 180;
                }
                if ((this.rmode == 4 || this.rmode == 5) && filter > 16)
                {
                    filter = 15;
                }
                if ((this.rmode == 1 || this.rmode == 3 || this.rmode == 6 || this.rmode == 7 || this.rmode == 8) && filter > 32)
                {
                    filter = 32;
                }
                //this.control.FilterBandwidth = (int)filter * 1000;
                //this.rfilter = filter;
                */
            }
            return 0;
        }

        private static readonly byte[] availableModes = new byte[] //Our Tune UP/DOWN button
        {
            1, //HOME
            2, //Tune UP
            3 //Tune Down
        };

        private static readonly byte[] availableFilters = new byte[]
        {
            //3,
            //6,
            //15,
            //50,
            //180,
        };
        private ReceiverCapabilities receiverCapabilities = new ReceiverCapabilities
        {

            Options = new ReceiverOptions[]
            {
                //ReceiverOptions.Filter, //Not sure what I will use buttons below Tune buttons for
                //ReceiverOptions.Frequency, 
                ReceiverOptions.Mode, //TUNE Up / TUNE DOWN buttons
                ReceiverOptions.RadioText, //Display 
                //ReceiverOptions.StereoMono,
                //ReceiverOptions.AGC,
                //ReceiverOptions.Volume, //Not needed
                ReceiverOptions.Signal,
            },

            Modes = new Dictionary<byte, string>
            {
                {
                    1,
                    "HOME"
                },
                {
                    2,
                    "TUNE UP"
                },
                {
                    3,
                    "TUNE DOWN"
                }
            },
            /*
            Filters = new Dictionary<byte, string>
            {
                {
                    3,
                    "3kHz"
                },
                {
                    6,
                    "6kHz"
                },
                {
                    15,
                    "15kHz"
                },
                {
                    50,
                    "50kHz"
                },
                {
                    180,
                    "180kHz"
                }
            },
            */
            Bands = new FrequencyRange[]
            {
                new FrequencyRange(0UL, 24000000000UL)
            }
        };
        private byte getSignal() //Signal
        {
            byte Signal = 0x00;
            //Signal = Math.Min(Convert.ToByte(this.control.VisualSNR * 2.55), (byte)0xFF); //Float is 4 bytes
            Signal = hama.GetSignal();
            return Signal;
        }
        private void getRDS(int value)//Display
        {
            if (value == 0)
            {
                string text = string.Format("{0}", this.hama.GetStationName());
                this.RadioTextChanged(this, new RadioTextChangedEventArgs(text));
                if (text != this.RadioText)
                {
                    this.RadioText = text;
                    if (this.RadioTextChanged != null)
                    {
                        this.RadioTextChanged(this, new RadioTextChangedEventArgs(text));
                    }
                }
            }
            else if (value == 1)
            {
                string text = this.hama.GetStationDLS();
                this.RadioTextChanged(this, new RadioTextChangedEventArgs(text));
                if (text != this.RadioText)
                {
                    this.RadioText = text;
                    if (this.RadioTextChanged != null)
                    {
                        this.RadioTextChanged(this, new RadioTextChangedEventArgs(text));
                    }
                }
            }
        }
        private void Tune(ulong value)
        {
            string service = Convert.ToString(value);
            Debug.WriteLine(service.Substring(0,1));
            this.hama.Tune(service.Substring(0,1));
        }

    }
}
