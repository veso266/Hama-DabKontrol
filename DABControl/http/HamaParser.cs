using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DABControl.HTTP; //For connection to FSAPI
using System.Xml.Linq; //FOR XML
using System.Diagnostics; //For debugging purposes

namespace DABControl
{
    class HamaParse
    {
        int PIN; //Our PIN that we get when inisiating class
        int SID; //Our Seasson Key that we get when creating our Seasson
        string IP;
        public HamaParse (string _IP, int _PIN)
        {
            IP = _IP; //Our Radio IP
            PIN = _PIN; //Our PIN
            CreateSession(); //Creates our season
        }

        /// <summary>
        /// Creates HAMA Session.
        /// </summary>
        public void CreateSession()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/CREATE_SESSION?pin=" + PIN);
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList sessonKey = xmlDoc.GetElementsByTagName("sessionId"); //Gets our SeasonKey XML Object and returns its InnerXML Value
            SID = Convert.ToInt32(sessonKey[0].InnerXml);
        }

        /// <summary>
        /// Stats the radio if it off and swiches to DAB
        /// </summary>
        public void StartRadio()
        {
            /* Power on */
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.sys.power?pin=" + PIN + "&sid=" + SID);
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            if (!(xmlDoc.GetElementsByTagName("u8")[0].InnerXml == "1")) //If Radio is OFF
            {
                //0 if Radio is OFF or is sleaping
                //1 if Radio is ON
                Debug.WriteLine("Starting UP");
                connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.sys.power?pin=" + PIN + "&sid=" + SID + "&value=1"); //Turn it on
            }
            /* Power on */

            /* Switch to DAB */
            //Since radio doesn't tell us which modes it is in we will switch to DAB anyway
            //http://192.168.88.11:80/fsapi/SET/netRemote.sys.mode?pin=1234&value=0
            // 0 = Internet Radio
            // 1 = Spotify
            // 2 = Air Play?? (my Hama does not have it so I am guessing :)) 
            // 3 = Musik Spieler (Music Player)
            // 4 = DAB
            // 5 = FM
            // 6 = AUX

            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.sys.mode?pin=" + PIN + "&value=4"); //Switch it to DAB
 
            /* Switch to DAB */
        }

        /// <summary>
        /// Stats the radio if it off and swiches to DAB
        /// </summary>
        public void StopRadio()
        {
            /* Power on */
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            Debug.WriteLine("Stopping Radio");
           connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.sys.power?pin=" + PIN + "&sid=" + SID + "&value=0");
        }

        /// <summary>
        /// Checks if Radio is ON/OFF
        /// </summary>
        public bool RadioStatus()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.sys.power?pin=" + PIN + "&sid=" + SID);
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            if (xmlDoc.GetElementsByTagName("u8")[0].InnerXml == "1") //If Radio is ON
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns status of the player.
        /// </summary>
        /// <returns>1=buffering/loading, 2=playing, 3=paused</returns>
        public int GetStatus()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.status?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _Status = xmlDoc.GetElementsByTagName("u8"); //Gets our u8 (station name inside) XML Object and returns its InnerXML Value
            return Convert.ToInt16(_Status[0].InnerXml);
        }

        /// <summary>
        /// Gets all available DAB Stations
        /// </summary>
        /// <returns>Array containing DAB stations</returns>
        public List<string> GetStationList()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=1"); //Set NAV Menu to 1 so we can request station List
            string data = connect.HttpGet("http://" + IP + "/fsapi/LIST_GET_NEXT/netRemote.nav.list/-1?pin=" + PIN + "&maxItems=-1"); //It also works on FM and Internet mode (-1 is just to get all the stations)
            List <string> StationList = new List<string>(); //Creates list to hold stations in
            XDocument document = XDocument.Parse(data); //Parses XML

            foreach (var Node in document.Root.Elements("item"))  //List of stations
            {
                var Station = Node.Element("field").Element("c8_array").Value; //selects c8_array node
                StationList.Add(Station); //adds it to the list
            }
            //BTW: ID for tunning that we need is StationList[0+1]
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=0"); //Finished so put it back
            return StationList;
        }
        /// <summary>
        /// Gets all available DAB Stations ordered by its service ID
        /// </summary>
        /// <returns>Array containing DAB stations</returns>
        public Dictionary<int, string> GetStationList_IDs()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=1"); //Set NAV Menu to 1 so we can request station List
            string data = connect.HttpGet("http://" + IP + "/fsapi/LIST_GET_NEXT/netRemote.nav.list/-1?pin=" + PIN + "&maxItems=-1"); //It also works on FM and Internet mode
            Dictionary<int, string> StationList = new Dictionary<int, string>(); //Creates Dictionary to hold stations and their IDs (we need those) in
            XDocument document = XDocument.Parse(data); //Parses XML

            foreach (var Node in document.Root.Elements("item").OrderBy(g => (int)g.Attribute("key")))  //Orders by item key
            {
                var Station = Node.Element("field").Element("c8_array").Value; //selects c8_array node
                StationList.Add((int)Node.Attribute("key"), Station); //adds it to the list
            }
            //BTW: ID for tunning that we need is StationList[0+1]
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=0"); //Finished so put it back
            return StationList;
        }

        public string Tune(string StationID)
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request: https://gist.github.com/ruel/865237
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=1"); //Set NAV Menu to 1 so we can tune
            string data = connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.action.selectItem?pin=1234&value=" + StationID); //if StationID = 3 TUNE UP
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            string _Status = xmlDoc.GetElementsByTagName("status")[0].InnerXml; //Gets our u8 (station name inside) XML Object and returns its InnerXML Value

            /*******************************************************************************************************/
            /*                                      * STATUS CODES *                                               */
            /*******************************************************************************************************/
            /* FS_OK = The command has been executed                                                               */
            /* FS_FAIL = The command hasn't been executed, because your value does not match the validation rules. */
            /* FS_PACKET_BAD = You tried to set the value of an read only node.                                    */
            /* FS_NODE_BLOCKED = You tried to SET a node of an operation Mode which is not active.                 */
            /* FS_NODE_DOES_NOT_EXIST = You tried to access an not existing node.                                  */
            /* FS_TIMEOUT = Your Request took to long.                                                             */
            /* FS_LIST_END = There is no list-entry left.                                                          */
            /*******************************************************************************************************/
            /*                                      * STATUS CODES *                                               */
            /*******************************************************************************************************/

            //http://192.168.88.11:80/fsapi/SET/netRemote.nav.action.selectItem?pin=1234&value=10 //Radio Antena
            //http://192.168.88.11:80/fsapi/SET/netRemote.nav.action.selectItem?pin=1234&value=3 //Net FM
            //http://192.168.88.11:80/fsapi/SET/netRemote.nav.action.selectItem?pin=1234&value=5 //A-Radio


            if (_Status == "FS_OK")
            {
                return "";
            }
            connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.nav.state?pin=" + PIN + "&sid=" + SID + "&value=0"); //put NAV Menu the way it was (0)
            return _Status;
        }

        /* TUNING
             value=3 = Tune UP; 
             value=4 = Tune Down
        */
        public string TuneUP()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request

            string data = connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.play.control?pin=" + PIN + "&value=3"); //if StationID = 3 TUNE UP
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            string _Status = xmlDoc.GetElementsByTagName("status")[0].InnerXml; //Gets our u8 (station name inside) XML Object and returns its InnerXML Value

            if (_Status == "FS_OK")
            {
                return "";
            }
            return _Status;
        }

        public string TuneDOWN()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/SET/netRemote.play.control?pin=" + PIN + "&value=4"); //if StationID = 4 TUNE UP
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            string _Status = xmlDoc.GetElementsByTagName("status")[0].InnerXml; //Gets our u8 (station name inside) XML Object and returns its InnerXML Value

            if (_Status == "FS_OK")
            {
                return "";
            }
            return _Status;
        }

        /// <summary>
        /// Gets Station Signal.
        /// </summary>
        /// <returns>Station Signal</returns>
        public byte GetSignal()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.signalStrength?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _Signal = xmlDoc.GetElementsByTagName("u8"); //Gets our u8 (station name inside) XML Object and returns its InnerXML Value
            return Convert.ToByte(_Signal[0].InnerXml);
        }

        /* ONLY FOR FM
        public string GetFrequency()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.frequency?pin=" + PIN + "&sid=" + SID); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _Freq = xmlDoc.GetElementsByTagName("u32"); //Gets our u8 (station name inside) XML Object and returns its InnerXML Value
            return (_Freq[0].InnerXml);
            //http://192.168.88.44/fsapi/GET/netRemote.play.frequency?pin=$PIN&sid=$SID
        }
        */

        /// <summary>
        /// Gets Station Name.
        /// </summary>
        /// <returns>Station Name</returns>
        public string GetStationName()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.info.name?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _STName = xmlDoc.GetElementsByTagName("c8_array"); //Gets our c8_array (station name inside) XML Object and returns its InnerXML Value
            string Station_Name = _STName[0].InnerXml;
            return Station_Name;
        }

        /// <summary>
        /// Gets DLS.
        /// </summary>
        /// <returns>DLS</returns>
        public string GetStationDLS()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.info.text?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _STDLS = xmlDoc.GetElementsByTagName("c8_array"); //Gets our c8_array (DLS inside) XML Object and returns its InnerXML Value
            string DLS = _STDLS[0].InnerXml;
            return DLS;
        }

        //DAB Stuff go here

        /// <summary>
        /// Gets DAB Essemble ID (Decimal).
        /// </summary>
        /// <returns>int</returns>
        public int DABEssembleID()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.serviceIds.dabEnsembleId?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _DABEssemble = xmlDoc.GetElementsByTagName("u16");
            int DABEssembleID = Convert.ToInt32(_DABEssemble[0].InnerXml);
            return DABEssembleID;
        }

        /// <summary>
        /// Gets DAB Service Component Identifier for current station(Decimal).
        /// </summary>
        /// <returns>Returns the DAB Service Component Identifier (decimal notation) Note: Nearly always 0 for audio services - Secondary Component services will have a different value</returns>
        public int DABService_Component_ID()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.serviceIds.dabScids?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _DABSCSID = xmlDoc.GetElementsByTagName("u8");
            int DABSCSID = Convert.ToInt16(_DABSCSID[0].InnerXml);
            return DABSCSID;
        }
        /// <summary>
        /// Gets DAB Service ID for current station(Decimal).
        /// </summary>
        /// <returns>Returns DAB Service Identifier (decimal notation) Note: commonly used in Hex notation</returns>
        public long DABServiceID()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.serviceIds.dabServiceId?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _DABSCSID = xmlDoc.GetElementsByTagName("u32");
            long DABSCSID = Convert.ToInt64(_DABSCSID[0].InnerXml);
            return DABSCSID;
        }
        /// <summary>
        /// Gets DAB Extended Country Code for current MUX(Decimal).
        /// </summary>
        /// <returns>
        /// Returns Extended Country Code (decimal notation) as defined in ETSI TS 101 756 
        /// https://www.etsi.org/deliver/etsi_ts/101700_101799/101756/01.06.01_60/ts_101756v010601p.pdf 5.4 Country ID (pages 10-13)
        /// Note: commonly used in Hex notation. 
        /// Defined in ETSI TS 101 756, used in conjunction with the first character (in Hex notation) of the dabServiceId to identify the country. 
        /// e.g. 
        /// dabServiceId 37952 (9440); ecc 228 (E4); Global Country Code of 9E4 = Slovenia
        /// dabServiceId 52951 (CED7); ecc 225 (E1); Global Country Code of CE1 = United Kingdom
        /// dabServiceId 57233 (DF91); ecc 224 (E0); Global Country Code of DE0 = Germany
        /// </returns>
        public int DAB_ECC()
        {
            Connect connect = new Connect(); //Performs BASIC HTTP GET Request
            string data = connect.HttpGet("http://" + IP + "/fsapi/GET/netRemote.play.serviceIds.ecc?pin=" + PIN); //It also works on FM and Internet mode
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(data); //Load XML Document from our string
            XmlNodeList _DAB_ECC = xmlDoc.GetElementsByTagName("u8");
            int DAB_ECC = Convert.ToInt16(_DAB_ECC[0].InnerXml);
            return DAB_ECC;
        }
        /* 
        Don't know what theese do 
        netRemote.nav.action.dabPrune //maybe it can tune DAB via MUX or frequency?
        /fsapi/GET/netRemote.nav.action.dabScan?pin=1337&sid=1983995656 //Does DAB Scan but IDK how do you know when scan is finished and how many station it found?? (maybe <value><u8>(numberOfStation | 1 when scan is finished)</u8></value>)
        netRemote.nav.dabScanUpdate //hmm you call it when you want to update stations?? why??

        Didn't implement them yet

        Name: netRemote.sys.caps.dabFreqList
        Method: LIST_GET_NEXT

        Lists available dab-frequencies

        Didn't implement them yet
        */
    }
}
