# TIL
Things learned while making the Content Security Policy rule collector extension for Fiddler.

## Content Security Policy
### Distinguishing between unsafe-inline and unsafe-eval
[Content Security Policy violation reports](http://www.w3.org/TR/CSP2/#violation-reports) convey CSP rule violations in a manner somewhat unlike how CSP rules are described to the browser.  The 'blocked-uri' property of the report contains the URI of the resource that is blocked by CSP unless there is no URI as can happen for blocked inline script or blocked eval.  In these cases 'blocked-uri' is the empty string and no other property indicates if the violation report was generated because of blocked inline script or blocked eval.

To resolve this I send two CSP Report-Only headers with two different report URIs. One allows unsafe-inline and the other unsafe-eval so I can distinguish an empty string blocked-uri based on which report URI received the report.

### Firefox CSP oddities
Amongst Internet Explorer Win10 preview, Chrome, Firefox and the CSP standard, Firefox has some unique aspects

The blocked-uri can be the string self if the origin of the blocked URI matches that of the document-uri. This is not something the spec describes or the other browsers do.

The child-src directive described in the spec is not implemented and results in an error in the Firefox developer console. The rest of the CSP header is still respected.

When sending two CSP Report-Only headers and Firefox is sending a report for unsafe-inline and unsafe-eval to the report-uris respectively, the second report has two violated directives: `"violated-directive":"script-src 'unsafe-eval'script-src 'unsafe-inline'"`

## Fiddler Extension
### Faking an HTTP server in Fiddler extension
Fiddler makes it easy to fake being an HTTP server:

    session.utilCreateResponseAndBypassServer();
    session.oResponse.headers.Add("Content-Type", "text/html");
    session.ResponseBody = Encoding.UTF8.GetBytes("<!doctype html><HTML><BODY><H1>Report received.</H1></BODY></HTML>");


## .NET
### Parsing JSON in .NET
Parsing JSON in .NET is relatively simple. Create a .NET class with appropriate attributes to connect class members to JSON properties like so:

    [DataContract]
    public class CSPReportData
    {
        [DataMember(Name = "blocked-uri")]
        public string blockedUri;
        [DataMember(Name = "document-uri")]
        public string documentUri;

Specifying the Name paramter is only necessary if the member name isn't the same as what appears in JSON. In my case I can't put the '-' in the .NET member name so I must specify a Name paramter to the DataMember attribute.
To parse a Stream containing the JSON string it is again pretty easy:

    public static CSPReport Parse(Stream postData)
    {
        CSPReport cspReport = (CSPReport)new DataContractJsonSerializer(typeof(CSPReport)).ReadObject(postData);


