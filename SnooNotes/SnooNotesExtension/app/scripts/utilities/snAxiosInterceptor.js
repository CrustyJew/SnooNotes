import {reduxStore} from '../redux/contentScriptStore';

class SNAxiosInterceptor{
    accessToken = "";
    
    constructor(){
        reduxStore.subscribe(()=>{
            let user = reduxStore.getState().user;
            if(user && user.access_token){
                this.accessToken = user.access_token;
            }
        });
    }

    interceptRequest(config){
        config.headers.common['Authorization'] = 'Bearer ' + this.accessToken;
        return config;
    }
}

export let snInterceptor = new SNAxiosInterceptor();