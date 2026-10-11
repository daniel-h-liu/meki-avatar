// Assets/Plugins/WebGL/UnityBridge.jslib
//
// Only compiled into WebGL builds (Unity strips .jslib files from every
// other build target automatically — nothing to guard here).
//
// This handles the OUTBOUND direction only: Unity -> parent page.
// The INBOUND direction (parent page -> Unity, i.e. receiving 'init' and
// 'submit-rejected') is NOT handled here — that lives in the WebGL
// template's own JS, which calls unityInstance.SendMessage(...) into
// SessionManager.cs directly. See the Stage 7 README for exactly what to
// add there and why it can't safely be handed to you as a fabricated
// whole-file replacement.
//
// ORIGIN SECURITY: postMessage's second argument is the target origin.
// Both functions below try to read it from document.referrer — inside an
// iframe, this is set by the browser to the parent page's URL, so its
// origin IS the parent's origin, with no manual configuration needed per
// deployment. This is a genuine convenience, not a substitute for the
// parent page's OWN origin check on incoming messages (which the website
// side of this project already does via originIsTrusted() / DEV_MODE) —
// document.referrer can be empty or spoofed in some edge cases (e.g. a
// strict Referrer-Policy header), so treat this as defense-in-depth, not
// the only check. If document.referrer is unavailable for any reason,
// this falls back to '*' — which is intentionally permissive during
// local/DEV_MODE testing, matching the same DEV_MODE philosophy used on
// the website side, but must never ship that way. If you want a hard
// guarantee instead of the document.referrer convenience, replace
// targetOrigin's fallback below with your production origin as a literal
// string.

mergeInto(LibraryManager.library, {

  Bridge_SendUnityReady: function () {
    var targetOrigin = '*';
    try {
      if (document.referrer) {
        targetOrigin = new URL(document.referrer).origin;
      }
    } catch (e) {
      console.warn('[UnityBridge] could not parse document.referrer, falling back to *:', e);
    }
    window.parent.postMessage({ type: 'unity-ready' }, targetOrigin);
  },

  Bridge_SendAvatarResponse: function (jsonPtr) {
    var jsonString = UTF8ToString(jsonPtr);
    var payload;
    try {
      payload = JSON.parse(jsonString);
    } catch (e) {
      console.error('[UnityBridge] failed to parse outgoing JSON, not sending:', e, jsonString);
      return;
    }

    var targetOrigin = '*';
    try {
      if (document.referrer) {
        targetOrigin = new URL(document.referrer).origin;
      }
    } catch (e) {
      console.warn('[UnityBridge] could not parse document.referrer, falling back to *:', e);
    }

    window.parent.postMessage({ type: 'avatar-response', payload: payload }, targetOrigin);
  }

});
