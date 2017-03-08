import userManagerconfig from './config';
import moment from 'moment';

class UserManager{

    USER_KEY = "snoonotes-user";
    tokenExpires = null;
    accessToken = "";
    constructor(options){
        this.signinUrl = options.signinUrl;
        this.tokenEndpoint = options.tokenEndpoint;
        this.cookieName = options.cookieName;
        this.tokenLifetime = options.tokenLifetime;
        this.expireThreshold = options.expireThreshold;
    }

    getUser(){
        var user = chrome.storage.sync.get(USER_KEY,(u)=>{
            Promise.resolve(u)
        })
    }

    saveUser(user){
        chrome.storage.sync.set({USER_KEY:user},()=>{
            Promise.resolve();
        });
    }

    getAccessToken(){
        if(!accessToken || (tokenExpires && moment().subtract(this.expireThreshold,'minutes') ))
    }
}

export const userManager = new UserManager(userManagerConfig);