
export class SNAxiosInterceptor{
    
    
    constructor(store){
        this.accessToken = "";
        store.subscribe(()=>{
            let user = store.getState().user;
            if(user && user.access_token != this.accessToken){
                this.accessToken = user.access_token;
            }
        });
    }

    interceptRequest(config){
        config.headers.common['Authorization'] = 'Bearer ' + this.accessToken;
        return config;
    }
}
