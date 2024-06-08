using System.Net;
using System.Web;

namespace Example3
{
    class Program
    {
        // Properties
        private static bool Infinite { get; set; } = false;
        private static bool Detailed { get; set; } = false;
        private static bool Timestamp { get; set; } = false;
        private static bool NoColor { get; set; } = false;
        private static bool UseCommonLogFormat { get; set; } = false;
        private static bool ForceHttps { get; set; } = false;
        private static bool ShowOnlyHostName { get; set; } = false;
        private static int Interval { get; set; } = 1000;
        private static int Requests { get; set; } = 5;
        private static int Redirects { get; set; } = 4;
        // SO: https://stackoverflow.com/questions/27108264/c-sharp-how-to-properly-make-a-http-web-get-request
        private static void HttpRequestLoop(string query)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
            request.MaximumAutomaticRedirections = Redirects;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Credentials = CredentialCache.DefaultCredentials;

            int sentRequests = 0;
            Console.WriteLine("Sending HTTP{1} requests to [{0}]:", query, query.Contains("https://") ? "S" : "");

            while (true)
            {
                var response = HttpRequest(request);

                if (response != null)
                    DisplayResponse(response, response.ResponseUri);

                sentRequests++;

                if (!Infinite && sentRequests == Requests)
                {
                    break;
                }

                Thread.Sleep(Interval);

                if (response != null)
                {
                    response.Close();
                }
            }
        }


        private static HttpWebResponse HttpRequest(HttpWebRequest req)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                return response;
            }
            catch (WebException e)
            {
                Error(e.Message + (Timestamp ? " @ " + DateTime.Now.ToString("HH:mm:ss") : ""));
            }
            catch (Exception e)
            {
                Error(e.GetType() + ":" + e.Message + (Timestamp ? " @ " + DateTime.Now.ToString("HH:mm:ss") : ""));
            }

            return null;
        }

        private static void DisplayResponse(HttpWebResponse response, Uri responseAddress)
        {
            if (!UseCommonLogFormat)
            {
                Console.Write("Response from {0}: Code=", ShowOnlyHostName ? responseAddress.Host.ToString() : responseAddress.ToString());
            }

            if (!NoColor)
            {
                // Apply colour rules to http status code
                Console.ForegroundColor = ConsoleColor.Black;
                int statusCode = (int)response.StatusCode;
                if (statusCode >= 100 && statusCode <= 199)
                    // Informative
                    Console.BackgroundColor = ConsoleColor.Blue;
                else if (statusCode >= 200 && statusCode <= 299)
                    // Success
                    Console.BackgroundColor = ConsoleColor.Green;
                else if (statusCode >= 300 && statusCode <= 399)
                    // Redirection
                    Console.BackgroundColor = ConsoleColor.Cyan;
                else if (statusCode >= 400 && statusCode <= 499)
                    // Client errors
                    Console.BackgroundColor = ConsoleColor.Yellow;
                else if (statusCode >= 500 && statusCode <= 599)
                    // Server errors
                    Console.BackgroundColor = ConsoleColor.Red;
                else
                    // Other
                    Console.BackgroundColor = ConsoleColor.Magenta;
            }

            if (UseCommonLogFormat)
            {
                Console.WriteLine("{6} {0} [{1:dd/MMM/yyyy HH:mm:ss zzz}] \"GET {2} {3}/1.0\" {4} {5}",
                                  responseAddress.Host,
                                  DateTime.Now,
                                  responseAddress.AbsolutePath,
                                  responseAddress.Scheme.ToString().ToUpper(),
                                  ((int)response.StatusCode).ToString(),
                                  response.ContentLength,
                                  "resolvedAddress");
                if (!NoColor)
                    ResetConsoleColors();

                return;
            }

            // Display response content
            Console.Write("{0}:{1}",
                (int)response.StatusCode,
                response.StatusCode);
            if (!NoColor)
                ResetConsoleColors();
            Console.Write(" Size={0}", response.ContentLength);

            if (Detailed)
                Console.Write(" Server={0} Cached={1}", response.Server, response.IsFromCache);

            if (Timestamp)
                Console.Write(" @ {0}", DateTime.Now.ToString("HH:mm:ss"));

            Console.WriteLine();
        }

        private static string LookupAddress(string address)
        {
            IPAddress ipAddr = null;

            // Only resolve address if not already in IP address format
            if (IPAddress.TryParse(address, out ipAddr))
                return ipAddr.ToString();

            try
            {
                // Query DNS for host address
                foreach (IPAddress a in Dns.GetHostEntry(address).AddressList)
                {
                    // Run through addresses until we find one that matches the family we are forcing
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddr = a;
                        break;
                    }
                }
            }
            catch (Exception) { }

            // If no address resolved then exit
            if (ipAddr == null)
                return "";

            return ipAddr.ToString();
        }

        private static void Error(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(msg);
            ResetConsoleColors();
        }

        private static void ResetConsoleColors()
        {
            Console.ResetColor();
        }

    }
}