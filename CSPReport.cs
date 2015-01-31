using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Json;
using Fiddler;

namespace FiddlerCSP
{
    [DataContract]
    public class CSPReport
    {
        public static CSPReport Parse(Stream postData)
        {
            try
            {
                return (CSPReport)new DataContractJsonSerializer(typeof(CSPReport)).ReadObject(postData);
            }
            catch (Exception exception)
            {
                postData.Seek(0, SeekOrigin.Begin);
                string postDataAsString = new StreamReader(postData).ReadToEnd();
                throw new Exception("Invalid CSP Report - JSON: " + postDataAsString + " exception: " + exception, exception);
            }
        }

        public static CSPReport Parse(string postData)
        {
            return Parse(new MemoryStream(Encoding.UTF8.GetBytes(postData)));
        }

        public override string ToString() 
        {
            return "{ csp-report: {\n" +
                (cspReport.blockedUri != null ? "blocked-uri: \"" + cspReport.blockedUri + "\",\n" : "") +
                (cspReport.documentUri != null ? "document-uri: \"" + cspReport.documentUri + "\",\n" : "") +
                (cspReport.effectiveDirective != null ? "effective-directive: \"" + cspReport.effectiveDirective + "\",\n" : "") +
                (cspReport.originalPolicy != null ? "original-policy: \"" + cspReport.originalPolicy + "\",\n" : "") +
                (cspReport.referrer != null ? "referrer: \"" + cspReport.referrer + "\",\n" : "") +
                (cspReport.statusCode != 0 ? "status-code: " + cspReport.statusCode + ",\n" : "") +
                (cspReport.violatedDirective != null ? "violated-directive: \"" + cspReport.violatedDirective + "\",\n" : "") +
                "} }";
        }

        [DataMember(Name = "csp-report")]
        public CSPReportData cspReport;

        [DataContract]
        public class CSPReportData
        {
            [DataMember(Name = "blocked-uri")]
            public string blockedUri;
            [DataMember(Name = "document-uri")]
            public string documentUri;
            [DataMember(Name = "effective-directive")]
            public string effectiveDirective;
            [DataMember(Name = "original-policy")]
            public string originalPolicy;
            [DataMember(Name = "referrer")]
            public string referrer;
            [DataMember(Name = "status-code")]
            public int statusCode;
            [DataMember(Name = "violated-directive")]
            public string violatedDirective;
            [DataMember(Name = "source-file")]
            public string sourceFile;
            [DataMember(Name = "line-number")]
            public int lineNumber;
            [DataMember(Name = "column-number")]
            public int columnNumber;
        };

    }
}
