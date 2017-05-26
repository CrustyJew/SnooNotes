<template>
    <div id="sn-main">
        <note-display></note-display>
        <div style="display:none;">
            <note-thing v-for="thingid in thingids" :thingid="thingid"></note-thing>
        </div>
    </div>
</template>
<script>
import { getNotesForUsers } from './redux/actions/notes';
export default {
  props:['thingids'],
  data(){
      return{
        
      }
  }
}
</script>
