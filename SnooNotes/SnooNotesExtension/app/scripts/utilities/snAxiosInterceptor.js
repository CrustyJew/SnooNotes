
export class SNAxiosInterceptor{
    accessToken = "";
    
    constructor(store){
        store.subscribe(()=>{
            let user = store.getState().user;
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
