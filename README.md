# CSP-Fiddler-Extension
Content Security Policy rule collector extension for Fiddler.

## What's this?
This is an extension for [Fiddler4](http://www.telerik.com/fiddler) that helps you produce the minimal required set of [Content-Security-Policy](https://docs.webplatform.org/wiki/tutorials/content-security-policy) rules for web pages. Install the extension, turn it on, navigate to web pages, and view the CSP rules that the extension generates.

## Install
Win+R, powershell.exe and enter the following into the PowerShell prompt:

    wget -uri http://david-risney.github.io/CSP-Fiddler-Extension/fiddlercsp.dll -OutFile "$env:userprofile\My Documents\Fiddler2\Scripts\FiddlerCSP.dll"

Or if you want to put in more effort, clone this repo, build it, and copy the built FiddlerCSP.dll to your ~\My Documents\Fiddler2\Scripts directory.

## Run
After installing:
 * Start Fiddler4.
 * Click on the 'CSP Rule Collector' tab.
 * Ensure the 'Enable Rule Collection' checkbox is checked.
 * In your web browser navigate to the page for which you want to generate CSP rules.
 * Go back to the 'CSP Rule Collector' tab.
 * Select the URI of the document you visited to see its CSP rules.

Do not leave the 'Enable Rule Collection' checkbox checked. While checked the extension will make web responses non-cachable in your browser and injects CSP HTTP headers that will result in possibly many developer console errors.

To get accurate results be sure to clear your browsers cache of any site for which you want to collect CSP rules. If resources are cached the extension won't be able to inject CSP HTTP headers and collect CSP information.
