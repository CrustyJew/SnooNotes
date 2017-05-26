import {reduxStore} from '../redux/contentScriptStore';
import {redirectSuccess} from '../redux/actions/user';

const unsub = reduxStore.subscribe(()=>{
    unsub();
    if((/silent/i).test(window.location.href)){
         chrome.runtime.sendMessage(window.location.href);
         window.parent.postMessage(window.location.href,"*");
    }else{
        reduxStore.dispatch(redirectSuccess(window.location.href));
    }
})