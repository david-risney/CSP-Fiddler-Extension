using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiddler;

[assembly: Fiddler.RequiredVersion("4.4.9.3")]
namespace FiddlerCSP
{
    public class FiddlerExtension : IAutoTamper3, IDisposable
    {
        public static class Settings
        {
            private const string prefix = "FiddlerCSPExtension.";
            public static bool verboseLogging
            {
                get { return Fiddler.FiddlerApplication.Prefs.GetBoolPref(prefix + "verboseLogging", false); }
                set { Fiddler.FiddlerApplication.Prefs.SetBoolPref(prefix + "verboseLogging", value); }
            }
            public static bool enabled
            {
                get { return Fiddler.FiddlerApplication.Prefs.GetBoolPref(prefix + "enabled", false); }
                set { Fiddler.FiddlerApplication.Prefs.SetBoolPref(prefix + "enabled", value); }
            }
        }

        public class FiddlerAppLogger : ILogger
        {
            public void Log(string message)
            {
                if (Settings.verboseLogging)
                {
                    FiddlerApplication.Log.LogString("FiddlerCSP: " + message);
                }
            }
        }

        public static string reportHost = "fiddlercsp.deletethis.net";
        private ILogger logger;
        private CSPRuleCollector collector;

        public FiddlerExtension()
        {
            logger = new FiddlerAppLogger();
            collector = new CSPRuleCollector(logger);
        }

        public void OnPeekAtRequestHeaders(Session oSession) { }
        public void OnPeekAtResponseHeaders(Session oSession) { }
        public void AutoTamperRequestAfter(Session oSession) { }

        public void AutoTamperRequestBefore(Session session)
        {
            if (!session.isTunnel && !session.isFTP)
            {
                bool handled = false;
                if (session.oRequest.host == reportHost && Settings.enabled)
                {
                    // Not sure the best way to handle these report URI requests. They are real requests from the browser but
                    // only generated because of this extension. Not sure if should be hidden from Fiddler's view or marked specially.
                    session.utilCreateResponseAndBypassServer();
                    session.oResponse.headers.Add("Content-Type", "text/html");
                    session.ResponseBody = Encoding.UTF8.GetBytes("<!doctype html><HTML><BODY><H1>Report received. Thanks. You're the best.</H1></BODY></HTML>");

                    string requestBody = session.GetRequestBodyAsString();
                    if (requestBody != null && requestBody.Length > 0)
                    {
                        try 
                        {
                            CSPReport cspReport = CSPReport.Parse(requestBody);
                            if (cspReport.cspReport != null && cspReport.cspReport.documentUri != null)
                            {
                                logger.Log("Got report for " + cspReport.cspReport.documentUri + " via " + session.fullUrl);
                            }

                            logger.Log("Adding " + cspReport.ToString());
                            collector.Add(cspReport, session.PathAndQuery == "/unsafe-eval" ?
                                CSPRuleCollector.InterpretBlank.UnsafeEval : CSPRuleCollector.InterpretBlank.UnsafeInline);
                            logger.Log("Total " + collector.ToString());

                            handled = true;
                        }
                        catch (Exception exception)
                        {
                            logger.Log("Invalid CSP - " + exception);
                        }
                    }
                }

                if (!handled)
                {
                    session.bBufferResponse = true;
                }
            }
        }

        public void AutoTamperResponseAfter(Session oSession) { }

        public void AutoTamperResponseBefore(Session session)
        {
            if (!session.isTunnel && !session.isFTP && Settings.enabled)
            {
                // Use https report URI for https sites because otherwise Chrome won't report.
                // Use http report URI for http sites because Fiddler might not be configured to MitM https.
                string reportUri = (session.isHTTPS ? "https" : "http") + "://" + reportHost;
                // child-src generates a complaint in FireFox as apparently it isn't implemented.
                string CSPROCommon = "child-src 'none'; connect-src 'none'; font-src 'none'; frame-src 'none'; img-src 'none'; media-src 'none'; object-src 'none'; style-src 'none'; ";
                // Two different CSP-Report-Only headers with different report URIs so that we can tell the difference between unsafe-eval and unsafe-inline since they're
                // both reported as empty string blocked-uri properties. Sort of a CSP spec problem.
                session.oResponse.headers.Add("Content-Security-Policy-Report-Only", CSPROCommon + "script-src 'unsafe-eval'; report-uri " + reportUri + "/unsafe-inline");
                session.oResponse.headers.Add("Content-Security-Policy-Report-Only", "script-src 'unsafe-inline'; report-uri " + reportUri + "/unsafe-eval");
                session.oResponse.headers.Add("X-Fiddled-With-By", "FiddlerCSP");

                // Set cache headers to not cache response since we're modifying the headers and don't want browsers to remember this.
                session.oResponse.headers.Remove("Cache-Control");
                session.oResponse.headers.Add("Cache-Control", "private, max-age=0, no-cache");
                session.oResponse.headers.Remove("Expires");
                session.oResponse.headers.Add("Expires", "Thu, 01 Dec 1983 20:00:00 GMT");

                logger.Log("Adding report-only to " + session.fullUrl);
            }
        }

        public void OnBeforeReturningError(Session oSession) { }

        public void OnBeforeUnload() { }

        public void OnLoad()
        {
            FiddlerApplication.OnValidateServerCertificate += OnValidateServerCertificate;
            AddTab();
        }

        private void OnValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
        {
            // Ignore cert errors for the made up report host.
            e.ValidityState = (e.ExpectedCN == reportHost) ? CertificateValidity.ForceValid : e.ValidityState;
        }

        private void AddTab()
        {
            TabPage page = new TabPage("CSP Rule Collector");
            var ruleCollectionView = new RuleCollectionView(collector);
            ruleCollectionView.Dock = DockStyle.Fill;
            page.Controls.Add(ruleCollectionView);
            FiddlerApplication.UI.tabsViews.TabPages.Add(page);
        }

        private void Dispose(bool managedAndNative)
        {
            collector.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
