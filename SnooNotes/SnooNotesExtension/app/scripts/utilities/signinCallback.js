import {reduxStore} from '../redux/contentScriptStore';
import {redirectSuccess,silentRenewSuccess} from '../redux/actions/user';

const unsub = reduxStore.subscribe(()=>{
    unsub();
    
        reduxStore.dispatch(redirectSuccess(window.location.href));
    
})