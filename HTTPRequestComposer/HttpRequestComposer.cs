using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTTPRequestComposer
{
    public class HttpRequestComposer
    {
        private HashSet<string> _validMethods;
        private HttpWebRequest _request;
        private HttpWebResponse _response;
        private string _errorMessage;
        private string _responseHtml;

        public HttpRequestComposer()
        {
            _validMethods = new HashSet<string>(Enum.GetNames(typeof(HttpRequestMethod)));
            Headers = new Dictionary<string, string>();
        }

        public string Url { get; set; }
        public string Method { get; set; }
        public string HttpVersion { get; set; }
        public string RequestBody { get; set; }

        public HttpWebResponse Response { get { return _response; } }
        public string ResponseHtml { get { return _responseHtml; } }
        public string ErrorMessage { get { return _errorMessage; } }
        
        public IDictionary<string, string> Headers { get; set; }

        public bool ParseRawRequest(string rawRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(rawRequest))
                {
                    _errorMessage = "Request string can not be empty.";
                    return false;
                }
                var lines = rawRequest.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                if (!ValidateRawRequest(rawRequest))
                {
                    return false;
                }


                string[] parts;
                foreach (var line in lines.Skip(1).TakeWhile(x => x != ""))
                {
                    parts = line.Split(':');
                    if(parts.Length> 2)
                    {
                        Headers.Add(parts[0], string.Join(":",parts.Skip(1)));
                    }
                    else
                    {
                        Headers.Add(parts[0], parts[1].Trim());
                    }
                }

                var body = lines.SkipWhile(x => x != "").ToList();
                if(body.Count > 0)
                {
                    RequestBody = string.Join(Environment.NewLine, body);
                }

                parts = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                Method = parts[0].ToUpper();

                Uri uri = new Uri(parts[1], UriKind.RelativeOrAbsolute);

                if (uri.IsAbsoluteUri)
                {
                    Url = parts[1];
                }
                else
                {
                    Url = Headers["Host"].StartsWith("http") ? Headers["Host"] + parts[1] : "http://" + Headers["Host"] + parts[1];
                }
                HttpVersion = parts[2];

                if(!InitializeHttpRequest())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
            
        }
        

        private bool InitializeHttpRequest()
        {
            try
            {
                _request = (HttpWebRequest) WebRequest.Create(Url);
                _request.Method = Method;
                _request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
               
                if(HttpVersion == "HTTP/1.0")
                {
                    _request.ProtocolVersion = System.Net.HttpVersion.Version10;
                }
                else
                {
                    _request.ProtocolVersion = System.Net.HttpVersion.Version11;
                }

                if (!_request.RequestUri.IsAbsoluteUri)
                {
                    _request.Host = Headers["Host"];
                }
                Headers.Remove("Host");
                if (Headers.ContainsKey("Connection"))
                {
                    if(string.Compare(Headers["Connection"],"keep-alive",true) == 0)
                    {
                        _request.KeepAlive = true;
                    }
                    else if(string.Compare(Headers["Connection"], "close", true) != 0)
                    {
                        _request.Connection = Headers["Connection"];
                    }
                    Headers.Remove("Connection");
                }
                if (Headers.ContainsKey("Accept"))
                {
                    _request.Accept = Headers["Accept"];
                    Headers.Remove("Accept");
                }
                if (Headers.ContainsKey("User-Agent"))
                {
                    _request.UserAgent = Headers["User-Agent"];
                    Headers.Remove("User-Agent");
                }
                if (Headers.ContainsKey("Content-Length"))
                {
                    _request.ContentLength = long.Parse(Headers["Content-Length"]);
                    Headers.Remove("Content-Length");
                }
                if (Headers.ContainsKey("Content-Type"))
                {
                    _request.ContentType = Headers["Content-Type"];
                    Headers.Remove("Content-Type");
                }
                if (Headers.ContainsKey("Expect"))
                {
                    _request.Expect = Headers["Expect"];
                    Headers.Remove("Expect");
                }
                if (Headers.ContainsKey("Date"))
                {
                    _request.Date = DateTime.Parse(Headers["Date"]);
                    Headers.Remove("Date");
                }
                if (Headers.ContainsKey("If-Modified-Since"))
                {
                    _request.IfModifiedSince = DateTime.Parse(Headers["If-Modified-Since"]);
                    Headers.Remove("If-Modified-Since");
                }
                if (Headers.ContainsKey("Referer"))
                {
                    _request.Referer = Headers["Referer"];
                    Headers.Remove("Referer");
                }

                if (Headers.ContainsKey("Transfer-Encoding"))
                {
                    _request.SendChunked = true;
                }

                foreach (var header in Headers)
                {
                    _request.Headers.Add(header.Key, header.Value);
                }

                if(RequestBody != null)
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    var bytes = encoding.GetBytes(RequestBody);
                    _request.ContentLength = bytes.Length;
                    using (var requestStream = _request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
        }

        public void InitializeHttpRequest(string url, 
            string host,
            string method,
            string userAgent = "", 
            string accept = "",
            string acceptEncoding = "",
            string acceptLanguage = "")
        {
            try
            {
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    url = "http://" + host + url;
                }
                _request = (HttpWebRequest)WebRequest.Create(url);
                _request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                if (!_request.RequestUri.IsAbsoluteUri)
                {
                    _request.Host = host;
                }

                _request.Method = method;

                if (!string.IsNullOrEmpty(userAgent))
                {
                    _request.UserAgent = userAgent;
                }
                if (!string.IsNullOrEmpty(accept))
                {
                    _request.Accept = accept;
                }
                if (!string.IsNullOrEmpty(acceptEncoding))
                {
                    _request.Headers.Add("Accept-Encoding", acceptEncoding);
                }
                if (!string.IsNullOrEmpty(acceptLanguage))
                {
                    _request.Headers.Add("Accept-Language", acceptLanguage);
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            
            
        }

        public async Task<bool> SendRequest()
        {
            try
            {

                _response = (HttpWebResponse)(await _request.GetResponseAsync());
                using (var reader = new StreamReader(_response.GetResponseStream(),Encoding.UTF8))
                {
                    _responseHtml = await reader.ReadToEndAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
        }

        private bool ValidateRawRequest(string rawRequest)
        {
            try
            {
                var lines = rawRequest.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var requestLine = lines[0];
                var parts = requestLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    _errorMessage = "Raw request format is invalid. Error on line 1";
                    return false;
                }
                var method = parts[0].ToUpper();
                if (!_validMethods.Contains(method))
                {
                    _errorMessage = "Request method is not valid.";
                    return false;
                }

                var url = parts[1];
                bool result = ValidateUrl(url);


                if (!result)
                {
                    _errorMessage = "URL is not valid.";
                }
                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri && !rawRequest.Contains("Host:"))
                {
                    _errorMessage = "Host parameter is expected.Either type an absolute URI or provide a valid host parameter.";
                }

                var httpVersion = parts[2];
                if(httpVersion != "HTTP/1.0" && httpVersion != "HTTP/1.1")
                {
                    _errorMessage = "HTTP Version is not valid.";
                    return false;
                }

                int counter = 2;
                foreach (var line in lines.Skip(1).TakeWhile(x => x != ""))
                {
                    parts = line.Split(':');
                    if (parts.Length < 2)
                    {
                        _errorMessage = string.Format("Raw request format is invalid. Error on line {0}", counter);
                        return false;
                    }
                    counter++;
                }
                _errorMessage = "";
                return true;

            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
            
        }

        private bool ValidateUrl(string url)
        {
            try
            {
                Uri uriResult;
                bool result = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uriResult);
                if (uriResult.IsAbsoluteUri)
                {
                    result &= (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public bool ValidateForm(string url,string host, string method)
        {
            try
            {
                bool result = true;

                if(string.IsNullOrEmpty(url) && string.IsNullOrEmpty(host))
                {
                    _errorMessage = "Please enter a valid URL or provide a valid Host";
                    return false;
                }

                result &= ValidateUrl(url);

                if (!result)
                {
                    _errorMessage = "Please enter a valid URL";
                    return false;
                }

                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri && (string.IsNullOrEmpty(host) || !ValidateUrl(host)))
                {
                    _errorMessage = "Please enter a valid Host";
                    return false;
                }

                result &= !string.IsNullOrEmpty(method);

                if (!result)
                {
                    _errorMessage = "Please select a request method.";
                    return false;
                }

                _errorMessage = "";
                return result;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
            
        }

        

    }
}
