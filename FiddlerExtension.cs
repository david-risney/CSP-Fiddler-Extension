using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiddler;

// Install requirements:
// - copy FiddlerCSP.dll to "%userprofile%\My Documents\Fiddler2\Scripts\$(TargetFilename)"

// Todo:
// - change verboseLogging to default to false

[assembly: Fiddler.RequiredVersion("4.4.9.3")]
namespace FiddlerCSP
{
    public class FiddlerExtension : IAutoTamper3
    {
        public static class Settings
        {
            private const string prefix = "FiddlerCSPExtension.";
            public static bool verboseLogging
            {
                get { return Fiddler.FiddlerApplication.Prefs.GetBoolPref(prefix + "verboseLogging", true); }
                set { Fiddler.FiddlerApplication.Prefs.SetBoolPref(prefix + "verboseLogging", value); }
            }
            public static bool enabled
            {
                get { return Fiddler.FiddlerApplication.Prefs.GetBoolPref(prefix + "enabled", false); }
                set { Fiddler.FiddlerApplication.Prefs.SetBoolPref(prefix + "enabled", value); }
            }
        }

        public static string reportHost = "fiddlercsp.deletethis.net";
        private CSPRuleCollector collector = new CSPRuleCollector();

        public static void Log(string message)
        {
            if (Settings.verboseLogging)
            {
                FiddlerApplication.Log.LogString("FiddlerCSP: " + message);
            }
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
                        CSPReport cspReport = CSPReport.TryParse(requestBody);
                        if (cspReport != null && cspReport.cspReport != null && cspReport.cspReport.documentUri != null)
                        {
                            Log("Got report for " + cspReport.cspReport.documentUri);
                        }
                        Log("Adding " + cspReport.ToString());
                        collector.Add(cspReport);
                        Log("Total " + collector.ToString());

                        handled = true;
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
                string reportUri = (session.isHTTPS ? "https" : "http") + "://" + reportHost + "/";
                session.oResponse.headers.Add("Content-Security-Policy-Report-Only", "connect-src 'none'; font-src 'none'; frame-src 'none'; img-src 'none'; media-src 'none'; object-src 'none'; style-src 'none'; report-uri " + reportUri);
                session.oResponse.headers.Add("X-Fiddled-With-By", "FiddlerCSP");

                // Set cache headers to not cache response since we're modifying the headers and don't want browsers to remember this.
                session.oResponse.headers.Remove("Cache-Control");
                session.oResponse.headers.Add("Cache-Control", "private, max-age=0, no-cache");
                session.oResponse.headers.Remove("Expires");
                session.oResponse.headers.Add("Expires", "Thu, 01 Dec 1983 20:00:00 GMT");

                Log("Adding report-only to " + session.fullUrl);
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
    }
}
