<template>
    <span v-if="Object.keys(snInfo.modded_subs).length > 0" class="sn-user-notes" @click.stop>
        <span @click="show">
            <i class="material-icons" :style="hasNotesStyle()">comment</i>
            <md-tooltip md-direction="right" md-delay="400">
                <span v-if="hasNotes">
                    <div><span :style="noteTypeStyle(getToolTip.mostRecent.SubName,getToolTip.mostRecent.NoteTypeID)">{{getToolTip.mostRecent.Message}}</span></div>
                    <div v-for="(sub, subName) in getToolTip.subs" :key="subName">
                        <h3>{{subName}}</h3>
                        <span v-for="(count, ntid) in sub" :style="noteTypeStyle(subName, ntid)" :key="ntid">
                            <i class="material-icons">{{getIcon(subName, ntid)}}</i> {{count}}</span>
                    </div>
                </span>
                <span v-else>No notes for this user</span>
            </md-tooltip>
        </span>
    </span>
    <span v-else class="sn-not-logged-in"></span>
</template>
<script>
import { store } from '../redux/contentScriptStore';
import { draggable } from './directives/draggable';
import { validationMixin } from 'vuelidate'
import { required, between } from 'vuelidate/lib/validators'
import axios from 'axios';
import { showNotesHub } from '../showNotesHub';

export default {
    name: 'user-notes',
    props: ['username', 'subreddit', 'url'],
    directives: { 'draggable': draggable },
    mixins: [validationMixin],
    data() {
        return {
            snInfo: this.$select('snoonotes_info as snInfo'),
            userNotes: this.$select('notes.' + this.username.toLowerCase() + ' as userNotes'),
            showNotes: false,
            displayStyle: {
                display: 'none',
                position: 'absolute',
                top: '0px',
                right: '0px',

            },
            newNote: {
                message: "",
                newNoteTypeID: null,
                newNoteSubIndex: -1,
            },
            submitting: false
        }
    },
    computed: {
        hasNotes: function () {
            return this.userNotes && this.userNotes.length > 0;
        },
        getToolTip: function () {
            let toolTipData = {subs:{}};
            this.userNotes.forEach(function (note) {
                if(!toolTipData.mostRecent || toolTipData.mostRecent.Timestamp < note.Timestamp) toolTipData.mostRecent = note;
                toolTipData.subs[note.SubName]= toolTipData.subs[note.SubName] || {};
                if (!toolTipData.subs[note.SubName].hasOwnProperty(note.NoteTypeID)) toolTipData.subs[note.SubName][note.NoteTypeID] = 0;
                toolTipData.subs[note.SubName][note.NoteTypeID] += 1;
            });
            return toolTipData;
        }
    },
    methods: {
        show: function (e) {
            //hacky but fuckoff it works
            this.$parent.$parent.show({ event: e, subreddit: this.subreddit, username: this.username, url: this.url });
        },
        noteTypeStyle: function (sub, ntID) {
            //let subIndex = isNaN(sub) ? this.snInfo.modded_subs.findIndex(subreddit => subreddit.SubName == sub) : sub;
            let nt = this.snInfo.modded_subs[sub.toLowerCase()].Settings.NoteTypes.filter(nt => nt.NoteTypeID == ntID)[0]
            let style = {
                color: '#' + nt.ColorCode
            }
            if (nt.Bold) style.fontWeight = "bold";
            if (nt.Italic) style.fontStyle = "italic";
            return style;
        },
        hasNotesStyle: function(){
            let style = {
                color: '#888'
            }
            if(this.hasNotes){
                let noteStyle = this.noteTypeStyle(this.getToolTip.mostRecent.SubName,this.getToolTip.mostRecent.NoteTypeID);
                style.color = noteStyle.color;
            }
            return style;
        },
        getIcon: function (sub, ntID) {
            //let subIndex = isNaN(sub) ? this.snInfo.modded_subs.findIndex(subreddit => subreddit.SubName == sub) : sub;
            let nt = this.snInfo.modded_subs[sub.toLowerCase()].Settings.NoteTypes.filter(nt => nt.NoteTypeID == ntID)[0]

            return nt.IconString || "comment";
        }
    }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";


.sn-user-notes {
    cursor: pointer;

    i.material-icons {
        font-size: 18px;
        vertical-align: middle;
    }
}
</style>