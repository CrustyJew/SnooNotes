export default class CustomRedirectNavigator {
  prepare() {
    return Promise.resolve(this);
  }

  navigate(params) {
    //Log.debug("RedirectNavigator.navigate");

    if (!params || !params.url) {
      //Log.error("No url provided");
      return Promise.reject(new Error("No url provided"));
    }

    chrome.tabs.create({ 'url': params.url, active: true });

    return Promise.resolve();
  }

  get url() {
    //Log.debug("RedirectNavigator.url");
    return window.location.href;
  }
}