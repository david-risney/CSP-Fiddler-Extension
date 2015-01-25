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

        public void Add(CSPReport cspReport)
        {
            if (!(cspReport.cspReport.blockedUri == null ||
                cspReport.cspReport.documentUri == null ||
                cspReport.cspReport.effectiveDirective == null))
            {
                string documentUri = cspReport.cspReport.documentUri;
                string effectiveDirective = cspReport.cspReport.effectiveDirective;
                string blockedUri = cspReport.cspReport.blockedUri;
                if (blockedUri.Trim().Length == 0)
                {
                    blockedUri = "'unsafe-inline'";
                }
                else if (blockedUri.IndexOf(":") >= 0)
                {
                    try
                    {
                        var uri = new Uri(blockedUri);
                        blockedUri = uri.Scheme + "://" + uri.Host;
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
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
