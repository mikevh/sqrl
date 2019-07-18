'use strict';
var newSync, lastSync, encodedSqrlUrl = false;
var syncQuery = window.XMLHttpRequest ? new window.XMLHttpRequest() : new ActiveXObject('MSXML2.XMLHTTP.3.0');
var gifProbe = new Image(); 					// create an instance of a memory-based probe image
var localhostRoot = 'http://localhost:25519/';	// the SQRL client listener
Date.now = Date.now || function () { return (+new Date()) };	// add old browser Date.now() support

// Linux/WINE desktop environments lack the uniform means for registering scheme handlers.
// So when we detect that we're running under Linux we disable the invocation of SQRL with
// the "sqrl://" scheme and rely upon upon the localhost server --- UNLESS we detect 'sqrl'
// present in the user-agent header which gives us permission to invoke with the sqrl:// scheme.
window.onload = function () {
    if ((navigator.userAgent.match(/linux/i)) && !(navigator.userAgent.match(/sqrl/i)) && !(navigator.userAgent.match(/android/i))) {		// if we're on Linux, suppress the sqrl:// href completely
        document.getElementById("sqrl").onclick = function () { sqrlLinkClick(this); return false; };
    }
}


// =============================================================================
// checkForChange forms a polling loop by continually re-calling itself after a
// 500ms (half second) delay, until the 'stopPolling' boolean is set when the
// script initiates an HREF jump to the local SQRL localhost:2519 server. While
// the loop is running, it periodically fetches a text file 'sync.txt' from the
// website's server to check whether anything has changed (typically that the
// user has logged on with SQRL) which would require this page to be refreshed
// to reflect the new status. This supports both cross-device logon and any
// situation where localhost:25519 CPS support is unavailable for any reason.
function checkForChange() {
    if (document.hidden) {					// before probing for any page change, we check to 
        setTimeout(checkForChange, 5000);   // see whether the page is visible. If the user is 
        return;								// not viewing the page, check again in 5 seconds.
    }
    syncQuery.open('GET', '/sqrl/sync.txt');	// the page is visible, so let's check for any update
    syncQuery.onreadystatechange = function () {
        if (syncQuery.readyState === 4) {
            if (syncQuery.status === 200) {
                newSync = syncQuery.responseText;
                if (lastSync && lastSync !== newSync) {
                    document.location.href = document.location.pathname.substring(location.pathname.lastIndexOf("/") + 1);
                } else {
                    lastSync = newSync;
                }
            }
            setTimeout(checkForChange, 500); // after every query, successful or not, retrigger after 500msc.
        }
    };
    syncQuery.send(); // initiate the query to the 'sync.txt' object.
}
checkForChange();	// launch our background change checking


// =============================================================================
// this defines the "onload" (load success) function for our in-memory test GIF
// image. IF, and only if, it succeeds, we know the localhost server is up and
// listening and that it's safe to execute an HREF jump to the localhost for CPS.
gifProbe.onload = function () {  // define our load-success function
    document.location.href = localhostRoot + encodedSqrlUrl;
};

// =============================================================================
// this defines the "onerror" (GIF probe failure) function for our in-memory
// test GIF. If no SQRL localhost:25519 server replies to and returns a
// GIF, this function queues a retry of the load after a 200msec delay.
gifProbe.onerror = function () { // define our load-failure function
    setTimeout(function () { gifProbe.src = localhostRoot + Date.now() + '.gif'; }, 250);
}

// =============================================================================
// sqrlLinkClick is invoked by the SQRL URL's href upon the user's mouse-click
// or touch. The function verifies that the current page's SQRL HREF link has
// defined a custom property named "encoded-sqrl-url" which is a base64-encoded
// version of the SQRL URL, provided by the server.  When this function is
// triggered by the user's SQRL authentication action with the "encoded-sqrl-url"
// link property present, it initiates a one-second delay before initiating a
// page-change to http://localhost:25519/{encoded-sqrl-url}. When authentication
// succeeds, the authenticating SQRL client will return an "HTTP 302 Found" to
// redirect the user's browser to a logged-in page.
//
// Note that the page could use JavaScript to generate this base64url-encoded
// URL locally from the link's HREF value. But that would require guaranteed
// JavaScript execution on the page, which no webserver can force. Therefore,
// having the webserver explicitly provide the base64url-encoded link allows
// CPS login to be used successfully without any requirement for JavaScript.
function sqrlLinkClick(e) {
    encodedSqrlUrl = e.getAttribute('encoded-sqrl-url');
    // if we have an encoded URL to jump to, initiate our GIF probing before jumping
    if (encodedSqrlUrl) { gifProbe.onerror(); };	// trigger the initial image probe query
}