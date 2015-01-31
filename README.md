# CSP-Fiddler-Extension
Content Security Policy rule collector extension for Fiddler helps you produce a strong CSP for a web page.

## What's this?
Use this extension to add a CSP header to your page, or tighten up your existing CSP header.

This is an extension for [Fiddler](http://www.telerik.com/fiddler) that gives you the most restrictive  [Content-Security-Policy](https://docs.webplatform.org/wiki/tutorials/content-security-policy) for a web page but that won't generate any errors for that web page. Install the extension, turn it on, navigate to web pages using a browser that supports CSP, and view the CSP rules that the extension generates.

## Install
Win+R, powershell.exe and enter the following into the PowerShell prompt:

    wget -uri http://david-risney.github.io/CSP-Fiddler-Extension/fiddlercsp.dll -OutFile (Join-Path (mkdir -Force ~\Documents\Fiddler2\Scripts) FiddlerCSP.dll)

Or if you want to put in more effort, clone this repo, build it, and copy the built FiddlerCSP.dll to your ~\Documents\Fiddler2\Scripts directory.

## Run
After installing:
 * Start Fiddler.
 * Click on the 'CSP Rule Collector' tab.
 * Ensure the 'Enable Rule Collection' checkbox is checked.
 * In your web browser navigate to the page for which you want to generate CSP rules.
 * Go back to the 'CSP Rule Collector' tab.
 * Select the URI of the document you visited to see its CSP rules.

For best results:
 * Clear your browser's cache of any site for which you want to collect CSP rules. If resources are cached the extension won't be able to inject CSP HTTP headers and collect CSP information.
 * Visit your site in all the browsers that support CSP that you care about in the same Fiddler session. All browsers' results will be incorporated into one CSP rule for a particluar web page. Different browsers may violate different CSP rules due to different feature support resulting in different HTTP requests so be sure to check each browser.
 * Do not leave the 'Enable Rule Collection' checkbox checked when not intending to gather CSP rules. While enabled the extension will make web responses non-cachable in your browser and injects CSP HTTP headers that will result in possibly many developer console errors.

## How does it work?
The extension adds mock `Content-Security-Policy-Report-Only` headers to servers' responses. For instance:

    Content-Security-Policy-Report-Only: child-src 'none'; connect-src 'none'; font-src 'none'; frame-src 'none'; img-src 'none'; media-src 'none'; object-src 'none'; style-src 'none'; script-src 'unsafe-eval'; report-uri https://fiddlercsp.deletethis.net/unsafe-inline

It then watches for the browser to report errors to the specified `report-uri` and uses those reports to generate the most restrictive CSP that allows through all issues described in the reports.
