import { userManagerConfig, baseUrl } from '../config';
import moment from 'moment';
import { UserManager, OidcClient } from 'oidc-client';

// class UserManager{

//     USER_KEY = "snoonotes-user";
//     accessToken = "";
//     constructor(options){
//         this.signin_url = options.signin_url;
//     }
    

//     getAccessToken(silent = true) {
//         return new Promise((resolve,reject)=>{
//         chrome.identity.launchWebAuthFlow({ url: this.signin_url, interactive: !silent }, (url) => {
//             console.log(url);
//             if(!url){
//                 reject("Not Logged In");
//             }
//             else{
//                 resolve( url.split('#')[1].split('=')[1]);
//             }
//             });
//         })
//     }

//     login() {
//        return this.getAccessToken(false).then((token) => {
//             fetch(baseUrl + 'Account', {
//                 method: 'get',
//                 headers: {
//                     'Authorization': 'Bearer ' + token
//                 }
//             })
//                 .then(json)
//                 .then((data) => {
//                     saveUser(data);
//                     Promise.resolve(data);
//                 });
//         });
//     }

//     getUser() {
//         return chrome.storage.sync.get(USER_KEY, (u) => {
//             Promise.resolve(u)
//         })
//     }

//     saveUser(user) {
//         return chrome.storage.sync.set({ USER_KEY: user }, () => {
//             Promise.resolve();
//         });
//     }
// }
export let userManager = new UserManager(userManagerConfig);
