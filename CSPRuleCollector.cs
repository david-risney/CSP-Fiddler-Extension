using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fiddler;

namespace FiddlerCSP
{
    public class CSPRuleCollector
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<string, Dictionary<string, HashSet<string>>> rules = new Dictionary<string, Dictionary<string, HashSet<string>>>();

        public delegate void RuleAddedOrModified(string uri, string rule);
        public event RuleAddedOrModified OnRuleAddedOrModified;

        public string Get(string documentUri)
        {
            string prefix = "Content-Security-Policy: default-src 'none'";
            string result = "";
            cacheLock.EnterReadLock();
            try
            {
                result = rules[documentUri].Select(entry => (
                    entry.Value.Aggregate(entry.Key, (total, next) => (total + " " + next))
                    )).Aggregate(prefix, (total, next) => (total + "; " + next));
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

            return result;
        }

        public override string ToString()
        {
            string result = "";
            string[] keys;

            cacheLock.EnterReadLock();
            try
            {
                keys = rules.Keys.ToArray<string>();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
            result = keys.Select(
                uri => (uri + ": " + Get(uri))
            ).Aggregate("",
                (total, next) => (total + "\n" + next)
            ) + "\n";

            return result;
        }

        private static string UriOrigin(string fullUri)
        {
            var uri = fullUri;
            try
            {
                UriBuilder uriBuilder = new UriBuilder(fullUri);
                uriBuilder.Password = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = null;
                uriBuilder.Query = null;
                uriBuilder.UserName = null;
                uri = uriBuilder.Uri.AbsoluteUri;
                if (uri.EndsWith("/"))
                {
                    uri = uri.Substring(0, uri.Length - 1);
                }
            }
            catch (Exception)
            {
                // Ignore failure and just return the original URI.
                // .NET's URI parser doesn't always match the web and we might get CSP values in 
                // here that aren't URIs.
            }
            return uri;
        }

        public static string UriWrtDocumentUri(string uri, string documentUri)
        {
            var result = uri;
            try
            {
                var uriParsed = new Uri(uri);
                var documentUriParsed = new Uri(documentUri);

                if (uriParsed.Equals(documentUriParsed))
                {
                    result = "'self'";
                }
                else if (uriParsed.Scheme == documentUriParsed.Scheme)
                {
                    result = uriParsed.Host;
                    if (!(uriParsed.Port == 0 || uriParsed.IsDefaultPort))
                    {
                        result += ":" + uriParsed.Port;
                    }
                }
            }
            catch (Exception)
            {
                // Again we can get exceptions if the URI can't parse as a .NET URI.
                // Pass through if we can't do anything better.
            }

            return result;
        }

        public void Add(CSPReport cspReport)
        {
            if (!(cspReport.cspReport.blockedUri == null ||
                cspReport.cspReport.documentUri == null ||
                cspReport.cspReport.effectiveDirective == null))
            {
                string documentUri = cspReport.cspReport.documentUri;
                string documentUriOrigin = UriOrigin(documentUri);
                string effectiveDirective = cspReport.cspReport.effectiveDirective;
                string blockedUri = cspReport.cspReport.blockedUri;
                if (blockedUri.Trim().Length == 0)
                {
                    // How to handle unsafe-eval? Might require a different report-uri and rule set.
                    blockedUri = "'unsafe-inline'";
                }
                else if (blockedUri.IndexOf(":") >= 0)
                {
                    blockedUri = UriWrtDocumentUri(UriOrigin(blockedUri), documentUriOrigin);
                }
                else
                {
                    // Report can give out schemes with no delimiters or anything else.
                    blockedUri = blockedUri + ":";
                }

                cacheLock.EnterWriteLock();
                try
                {
                    if (!rules.Keys.Contains(documentUri))
                    {
                        rules.Add(documentUri, new Dictionary<string, HashSet<string>>());
                    }
                    if (!rules[documentUri].Keys.Contains(effectiveDirective))
                    {
                        rules[documentUri].Add(effectiveDirective, new HashSet<string>());
                    }
                    rules[documentUri][effectiveDirective].Add(blockedUri);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }

                OnRuleAddedOrModified.Invoke(documentUri, Get(documentUri));
            }
            else
            {
                FiddlerExtension.Log("FiddlerCSP: Invalid cspreport: " + cspReport);
            }
        }
    }
}
