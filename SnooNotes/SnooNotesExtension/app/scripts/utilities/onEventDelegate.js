// http://stackoverflow.com/a/14677089/395414
// updated for modern browsers
window.addEvent = function(elem,type,callback) {
    var evt = function(e) {
        e = e || window.event;
        return callback.call(elem,e);
    }; 
    var cb = function(e) { return evt(e); };
    elem.addEventListener(type,cb,false);
    return elem;
};
window.findParent = function(child,filter,root) {
    do {
        if( filter(child)) return child;
        if( root && child == root) return false;
    } while(child = child.parentNode);
    return false;
};
// removed window.hasClass in favor of native classList.contains() 



export const on = (bindTo, type, targetSelector, callback) => {
  window.addEvent(bindTo, type, function(e) {
    var s = window.findParent(e.srcElement || e.target, function(elm) {
        return elm.matches(targetSelector);
    },this);
    
    if( s && callback ) {
        callback(e)
    }
	});
}