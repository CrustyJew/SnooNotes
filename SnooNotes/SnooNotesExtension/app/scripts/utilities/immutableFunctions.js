
export const removeProperty = (obj, prop) => {
    return Object.keys(obj)
        .filter(key => key !== prop)
        .reduce((result,current)=>{
            result[current] = obj[current];
            return result;
        },{});
}