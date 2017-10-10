import {TOGGLE_SORT} from '../actions/options';

const initialState = {ascending:true};

export const optionsReducer = (state = initialState, action) =>{
    switch (action.type){
        case TOGGLE_SORT:{
            return Object.assign({},state,{ascending:!state.ascending});
        }
        default:{
            return state;
        }
    }
}