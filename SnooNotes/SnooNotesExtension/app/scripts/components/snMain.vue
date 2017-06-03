<template>
    <div id="sn-main">
        <!--<note-display></note-display>-->
        <div style="display:none;">
            <!--<note-thing v-for="thingid in thingIDs" :thingid="thingid"></note-thing>-->
            <media-analysis v-for="thingid in thingIDs" :thingid="thingid" :key="thingid"></media-analysis>
        </div>
    </div>
</template>
<script>
//import { getNotesForUsers } from './redux/actions/notes';
import mediaAnalysis from './mediaAnalysis.vue';
import _ from 'lodash';

export default {
    components: { 'media-analysis': mediaAnalysis },
    data() {
        return {
            thingIDs: []
        }
    },
    mounted: function () {
        this.$on('AddThings', (e) => {
            this.thingIDs = this.thingIDs.concat(e.thingIDs);
        });
        this.$on('RemoveThings', (e) => {
            this.thingIDs = _.without(this.thingIDs, e.thingIDs);
        })
    }
}
</script>
