import {reduxStore} from '../redux/contentScriptStore';
import {redirectSuccess,silentRenewSuccess} from '../redux/actions/user';

const unsub = reduxStore.subscribe(()=>{
    unsub();
    if(window.location.href.match(/silent/i)){
        reduxStore.dispatch(silentRenewSuccess(window.location.href));
    }else{
        reduxStore.dispatch(redirectSuccess(window.location.href));
    }
})