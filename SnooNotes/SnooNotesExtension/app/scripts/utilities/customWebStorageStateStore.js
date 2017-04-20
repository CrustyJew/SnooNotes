export default class CustomWebStorageStateStore {
    constructor({ prefix = "snoonotes-user:" } = {}) {
        this.PREFIX = prefix;
    }

    set(key, value) {
        return new Promise((resolve) => {
            return chrome.storage.local.set({ [this.PREFIX + key]: value }, () => {
                resolve();
            });
        });
    }

    get(key) {
        return new Promise((resolve) => {
            return chrome.storage.local.get(this.PREFIX + key, (v) => {
                resolve(v[this.PREFIX + key]);
            });
        });
    }

    remove(key) {
        return new Promise((resolve) => {
            return chrome.storage.local.remove(this.PREFIX + key, () => {
                resolve();
            });
        });
    }

    getAllKeys() {
        return new Promise((resolve) => {
            return chrome.storage.local.get(null, (v) => {
                var keys = [];
                for (var prop in v) {
                    if (prop.indexOf(this.PREFIX) == 0) {
                        keys.push(prop.substr(this.PREFIX.length));
                    }
                }
                resolve(keys);
            });
        });
    }
}