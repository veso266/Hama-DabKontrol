using System;
using System.IO;
using System.Net;

namespace DABControl.HTTP
{
    /// <summary>
    /// A simple basic class for HTTP Requests.
    /// </summary>
    class Connect
    {
        //Inspiration from here: https://gist.github.com/ruel/865237

        
        /// <summary>
        /// UserAgent to be used on the requests
        /// </summary>
        public string UserAgent = @"GlobalTuners/DAB";

        /// <summary>
        /// Performs a basic HTTP GET request.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <returns>HTML Content of the response.</returns>
        public string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = UserAgent;
            request.KeepAlive = false;
            request.Proxy = null; //If this is true then it takes too much time because it wants to detect proxy but we don't have a proxy
            request.Method = "GET";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();   
            }
            catch
            {
                return "403";
            }   
        }
    }
}