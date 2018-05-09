
export class ModLogModule {
    constructor() {
        this.modSubs = [];
    }
    refreshModule(subs) {
        this.modSubs = subs.map((s) => { return s.SubName; });
    }
    initModule(){

    }
}