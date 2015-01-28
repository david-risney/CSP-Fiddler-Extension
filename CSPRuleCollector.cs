using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fiddler;

namespace FiddlerCSP
{
    public class CSPRuleCollector : IDisposable
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<string, Dictionary<string, HashSet<string>>> rules = new Dictionary<string, Dictionary<string, HashSet<string>>>();

        public delegate void RuleAddedOrModified(string uri, string rule);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event RuleAddedOrModified OnRuleAddedOrModified;
        private ILogger logger;

        public CSPRuleCollector(ILogger logger)
        {
            this.logger = logger;
        }

        public string Get(string documentUri)
        {
            string prefix = "Content-Security-Policy: default-src 'none'";
            string result = "";
            cacheLock.EnterReadLock();
            try
            {
                result = rules[documentUri].OrderBy(x => x.Key).Select(entry => (
                    entry.Value.OrderBy(x => x).Aggregate(entry.Key, (total, next) => (total + " " + next))
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

        public enum InterpretBlank
        {
            UnsafeInline,
            UnsafeEval
        };

        public void Add(CSPReport cspReport, InterpretBlank blankIs)
        {
            if (cspReport.cspReport.blockedUri == null)
            {
                logger.Log("Invalid CSP Report - missing blocked-uri property.");
            }
            else if (cspReport.cspReport.documentUri == null)
            {
                logger.Log("Invalid CSP Report - missing document-uri property.");
            }
            else if (cspReport.cspReport.violatedDirective == null && cspReport.cspReport.effectiveDirective == null)
            {
                logger.Log("Invalid CSP Report - missing violated-directive and effective-directive properties.");
            }
            else
            {
                string documentUri = cspReport.cspReport.documentUri;
                string documentUriOrigin = UriOrigin(documentUri);
                string directive = cspReport.cspReport.effectiveDirective == null ? cspReport.cspReport.violatedDirective : cspReport.cspReport.effectiveDirective;
                string blockedUri = cspReport.cspReport.blockedUri;

                if (blockedUri.Trim().Length == 0)
                {
                    // A blank blocked-uri indicates either unsafe-inline or unsafe-eval. The caller tells us
                    // which it is.
                    blockedUri = blankIs == InterpretBlank.UnsafeInline ? "'unsafe-inline'" : "'unsafe-eval'";
                }
                else if (blockedUri.IndexOf(":") >= 0) // If there's a colon, assume its a URI.
                {
                    blockedUri = UriWrtDocumentUri(UriOrigin(blockedUri), documentUriOrigin);
                }
                else if (blockedUri == "self") // Firefox can return self as the blocked-uri.
                {
                    blockedUri = "'self'";
                }
                else // Lastly CSP reports may contain schemes with no delimiters just the scheme name.
                {
                    blockedUri = blockedUri + ":";
                }

                // Directive might be something like script-src or script-src none. We want just the first part.
                directive = directive.Split(' ')[0];

                cacheLock.EnterWriteLock();
                try
                {
                    if (!rules.Keys.Contains(documentUri))
                    {
                        rules.Add(documentUri, new Dictionary<string, HashSet<string>>());
                    }
                    if (!rules[documentUri].Keys.Contains(directive))
                    {
                        rules[documentUri].Add(directive, new HashSet<string>());
                    }
                    rules[documentUri][directive].Add(blockedUri);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
                if (OnRuleAddedOrModified != null)
                {
                    OnRuleAddedOrModified.Invoke(documentUri, Get(documentUri));
                }
            }
        }

        private void Dispose(bool manageAndNativeResources)
        {
            cacheLock.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
