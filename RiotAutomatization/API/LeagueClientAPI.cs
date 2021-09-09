using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace RiotAutomatization.API
{

    public class LeagueClient
    {
        private HttpClient httpClient;

        public string Token { get; set; }
        public ushort Port { get; set; }

        public LeagueClient()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            httpClient = new HttpClient(handler);
        }

        private string GetRiotCommandLineArgs()
        {
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c wmic PROCESS WHERE name='RiotClientUx.exe' GET commandline";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            //p.WaitForExit();

            return output;
        }

        private string[] GetPortAndAuthKey()
        {
            string[] retvalues = new string[3];

            string wholeCommandLine = GetRiotCommandLineArgs();

            string[] splitSpaces = wholeCommandLine.Split(' ');

            foreach (string s in splitSpaces)
            {
                if (s.Contains("="))
                {

                    string key = s.Split('=')[0];
                    string value = s.Split('=')[1];

                    if (key == "--app-port")
                    {
                        retvalues[0] = value;
                    }
                    else if (key == "--remoting-auth-token")
                    {
                        string Token = value;
                        retvalues[1] = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{Token}"));
                    }
                    else if (key == "--app-pid")
                    {
                        retvalues[2] = value;
                    }
                }
            }

            return retvalues;
        }

        public bool Connect()
        {
            string[] items = GetPortAndAuthKey();

            foreach(string s in items)
            {
                if (s == null || s == "")
                {
                    return false;
                }
            }

            Token = items[1];
            Port = ushort.Parse(items[0]);
            string ApiUri = "https://127.0.0.1:" + Port.ToString() + "/";

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Token);
            httpClient.BaseAddress = new Uri(ApiUri);

            return true;
        }

        public async Task<string> MakeHttpRequest(string method, string endpoint, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (method == "put")
            {
                var result = await httpClient.PutAsync(endpoint, content);
                string resultContent = await result.Content.ReadAsStringAsync();

                return resultContent;
            }

            return "";
        }

    }
}
