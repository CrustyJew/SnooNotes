import {reduxStore} from '../redux/contentScriptStore';
import {redirectSuccess,silentRenewSuccess} from '../redux/actions/user';
import {UserManager} from 'oidc-client';

const unsub = reduxStore.subscribe(()=>{
    unsub();
    if((/silent/i).test(window.location.href)){
        console.warn("sending " + url);
         chrome.runtime.sendMessage(url);
         window.parent.postMessage(url,"*");
    }else{
        reduxStore.dispatch(redirectSuccess(window.location.href));
    }
})