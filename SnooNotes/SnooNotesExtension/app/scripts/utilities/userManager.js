import { userManagerConfig } from '../config';
import moment from 'moment';

class UserManager{

    USER_KEY = "snoonotes-user";
    constructor(options){
        this.signin_url = options.signin_url;
    }
    

    getAccessToken(silent = true) {
        let authUrl = "" + this.signin_url + chrome.idenitity.getRedirectUrl();
        chrome.identity.launchWebAuthFlow({ url: authUrl, interactive: silent }, (url) => {
            promise.resolve(url.split('#')[1].split('=')[1]);
        });
    }

    getUser() {
        var user = chrome.storage.sync.get(USER_KEY, (u) => {
            Promise.resolve(u)
        })
    }

    saveUser(user) {
        chrome.storage.sync.set({ USER_KEY: user }, () => {
            Promise.resolve();
        });
    }
}

export let userManager = new UserManager(userManagerConfig);