<template>
    <transition name="fade">
        <div class="sn-notes-display" v-if="showNotes" :style="displayStyle" v-draggable="'.sn-header'">
            <div class="sn-header">
                <a class="sn-close-note sn-close" @click="close">X</a>
            </div>
            <table v-if="!notes.noNotes && !notes.loading">
                <thead>
                    <tr>
                        <td>Subreddit</td>
                        <td class="md-sortable" @click="toggleSort">Submitter / Date <i class="md-icon md-sortable-icon material-icons" :class="{'sorted-descending':!ascending}">arrow_upward</i></td>
                        <td>Note</td>
                    </tr>
                </thead>
                <tbody is="transition-group" name="fade">
                    <tr v-for="note in notes" transition="fade" :key="note.NoteID">
                        <td class="sn-sub-name">
                            <a :href="'https://reddit.com/r/'+note.SubName">{{note.SubName}}</a>
                            <span v-if="note.ParentSubreddit">
                                <br>via
                                <br>
                                <a :href="'https://reddit.com/r/'+note.ParentSubreddit">{{note.ParentSubreddit}}</a>
                            </span>
                            <span v-else-if="isCabal">
                                <br>
                                <cabalify :noteID="note.NoteID"></cabalify>
                            </span>
                        </td>
                        <td class="sn-submitter">
                            <span>{{note.Submitter}}</span>
                            <br>
                            <a :href="note.Url" style="white-space:pre;">{{new Date(note.Timestamp).toLocaleString().replace(', ', '\n')}}</a>
                        </td>
                        <td class="sn-message" :style="noteTypeStyle(note.SubName, note.NoteTypeID)">
                            <p>{{note.Message}}</p>
                            <a class="sn-delete-note" @click="deleteNote(note.NoteID)" v-if="!note.ParentSubreddit || modSubs.findIndex(s=>s.name == note.ParentSubreddit) > -1">
                                <i class="material-icons">delete_forever</i>
                            </a>
                        </td>
                    </tr>
                </tbody>
            </table>
            <div class="sn-new-note-container">
                <div class="sn-new-note">
                    <select class="sn-new-note-sub" v-model="newNote.newNoteSubIndex" :class="{'sn-error':$v.newNote.newNoteSubIndex.$error }">
                        <option value="-1" disabled>--Select a Sub--</option>
                        <option :value="modSub.index" v-if="isModdedSub">{{modSub.name}}</option>
                        <option value="-2" v-if="isModdedSub" disabled>---------</option>
                        <option v-for="sub in otherSubs" v-if="otherSubs.length >0" :value="sub.index" :key="sub.index">{{sub.name}}</option>
                    </select>
                    <textarea placeholder="Add a new note for user..." class="sn-new-message" v-model="newNote.message"></textarea>
                    <button type="button" class="sn-btn-submit sn-new-note-submit" @click="submit" :disabled="submitting">Submit</button>
                </div>
                <div class="sn-note-type" :class="{'sn-error':$v.newNote.newNoteTypeID.$error }">
                    <label v-for="nt in noteTypes" :style="noteTypeStyle(newNote.newNoteSubIndex,nt.NoteTypeID)" :key="nt.NoteTypeID">
                        <input type="radio" :value="nt.NoteTypeID" v-model="newNote.newNoteTypeID">{{nt.DisplayName}}</label>
                </div>
                <div class="sn-new-error">
                    <p v-if="$v.newNote.newNoteTypeID.$error">Shucks! You forgot the note type...</p>
                    <p v-if="$v.newNote.newNoteSubIndex.$error">Select a subby you fool!</p>
                </div>
            </div>

        </div>
    </transition>
</template>
<script>
import { store } from '../redux/contentScriptStore';
import { toggleSort } from '../redux/actions/options';
import { draggable } from './directives/draggable';
import { validationMixin } from 'vuelidate'
import { required, between } from 'vuelidate/lib/validators'
import axios from 'axios';
import cabalify from './cabalify.vue';
import UserNotes from './userNotes.vue';
import Vue from 'vue';
export default {
    name: 'user-notes-display',
    //props: ['username', 'subreddit', 'url', 'showNotes'],
    directives: { 'draggable': draggable },
    components: { 'cabalify': cabalify, 'user-notes': UserNotes },
    mixins: [validationMixin],
    data() {
        return {
            snInfo: this.$select('snoonotes_info as snInfo'),
            username: "",
            subreddit: "",
            url: "",
            showNotes: false,
            allNotes: this.$select('notes as allNotes'),
            isCabal: this.$select('user.isCabal as isCabal'),
            ascending: this.$select('options.ascending as ascending'),
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
    validations: {
        newNote: {
            newNoteSubIndex: {
                required,
                'notNegative': function(value) {
                    return value > -1;
                }
            },
            newNoteTypeID: {
                required
            }
        }
    },
    computed: {

        userNotes: function() {
            return this.allNotes[this.username.toLowerCase()];
        },
        hasNotes: function() {
            return this.usersWithNotes.indexOf(this.username.toLowerCase()) > -1;
        },
        usersWithNotes: function() {
            return this.snInfo.users_with_notes;
        },
        modSubs: function() {
            return this.snInfo.modded_subs.map((s, i) => { return { name: s.SubName, id: s.SubredditID, index: i } });
        },
        modSub: function() {
            return this.modSubs.filter(sub => sub.name == this.subreddit)[0];
        },
        otherSubs: function() {
            return this.modSubs.filter(sub => sub.name != this.subreddit && sub.name != "SpamCabal");
        },
        notes: function() {
            if(this.userNotes){
                return this.userNotes.sort((a,b)=>{
                    var da = new Date(a.Timestamp);
                    var db = new Date(b.Timestamp);
                    if((da < db && this.ascending)|| (da > db && !this.ascending)){
                        return -1
                    }
                    if((da > db && this.ascending) || (da < db && !this.ascending)){
                        return 1
                    }
                    return 0;
                })
            }
            return { noNotes: true }
        },
        noteTypes: function() {
            if (this.newNote.newNoteSubIndex == -1) return {}
            return this.snInfo.modded_subs[this.newNote.newNoteSubIndex].Settings.NoteTypes.filter(nt => !nt.Disabled);
        },
        isModdedSub: function() {
            if (this.subreddit && this.modSubs.findIndex(sub => sub.name == this.subreddit) > -1) {
                return true;
            }
            return false;
        }
    },
    methods: {
        close: function() {
            this.showNotes = false;
            this.newNote.message = "";
            this.newNoteTypeID = -1;
            document.removeEventListener('click', this.clickWatch, true);
        },
        noteTypeStyle: function(sub, ntID) {
            let subIndex = isNaN(sub) ? this.snInfo.modded_subs.findIndex(subreddit => subreddit.SubName == sub) : sub;
            let nt = this.snInfo.modded_subs[subIndex].Settings.NoteTypes.filter(nt => nt.NoteTypeID == ntID)[0]
            let style = {
                color: '#' + nt.ColorCode
            }
            if (nt.Bold) style.fontWeight = "bold";
            if (nt.Italic) style.fontStyle = "italic";
            return style;
        },
        show: function(e) {
            this.username = e.username;
            this.url = e.url;
            this.subreddit = e.subreddit;
            //this.displayStyle.top = e.target.offsetTop + 15 + 'px';
            //this.displayStyle.left = e.target.offsetLeft + 20 + 'px';
            this.displayStyle.top = e.event.pageY + 5 + 'px';
            this.displayStyle.left = e.event.pageX + 15 + 'px'

            this.displayStyle.display = 'block';
            this.showNotes = true;
            if (this.subreddit) {
                this.newNote.newNoteSubIndex = this.modSubs.findIndex(sub => sub.name == this.subreddit)
            } else {
                this.newNote.newNoteSubIndex = -1
            }
            document.addEventListener('click', this.clickWatch, true);
        },
        clickWatch: function(e) {
            if (!this.$el.contains(e.target)) this.close();
        },
        toggleSort: function(){
            store.dispatch(toggleSort());
        },
        submit: function() {
            this.$v.newNote.$touch();
            if (this.$v.newNote.$invalid) { return; }
            this.submitting = true;
            axios.post('Note', { NoteTypeID: this.newNote.newNoteTypeID, SubName: this.modSubs[this.newNote.newNoteSubIndex].name, Message: this.newNote.message, AppliesToUsername: this.username, Url: this.url })
                .then(() => {
                    this.submitting = false;
                    this.newNote.message = "";
                }, (e) => {
                    this.submitting = false;
                    this.$toasted.error("Failed to submit new note.");
                })
        },
        deleteNote: function(id) {
            axios.delete('Note?id=' + id);
        },
        processNewThing: function(author, subreddit, url, node){
            let noteElem = document.createElement('user-notes');
            noteElem.setAttribute('username', author);
            noteElem.setAttribute('subreddit', subreddit);
            noteElem.setAttribute('url', url);
            //noteElem.setAttribute('v-on:shownotes','show');
       
            node.appendChild(noteElem);
            new Vue({ components: { 'user-notes': UserNotes }, parent: this }).$mount(noteElem);
        }
    },
    watch: {
        'newNote.newNoteSubIndex': function(newIndex) {
            this.newNote.newNoteTypeID = null;
            this.$v.newNote.$reset();
        }
    },
    mounted: function() {
        this.$on('showNotes', (e) => {

            this.show(e.event)
        });
    },
    beforeDestroy: function() {
        document.removeEventListener('click', this.clickWatch, true);
    }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

.sn-notes-display {
    .sn-header {
        margin-left: -10px;
        margin-right: -10px;
        margin-bottom: 10px;
        .sn-close {
            display: inline-block;
            cursor: pointer;
            color: white;
            background-color: $accent;
            font-weight: bold;
            padding: 2px 10px;
            margin-top: -1px;
            &:hover {
                background-image: linear-gradient(to bottom, darken($accent, 10%), darken($accent, 25%));
            }
        }
    }
    a {
        color: $secondary;
    }
    p {
        display: inline;
    }
    a.sn-delete-note {
        color: $accent;
        font-size: 18px;
    }
    table {
        border-collapse: separate;
        border-spacing: 0;
    }
    td {
        border: solid 1px $gray;
        border-style: none solid solid none;
        padding: 10px;
    }
    thead{
        .md-sortable{
            cursor: pointer;
        }
    }
    thead td{
        text-align: center;
        font-weight: bold;
        i{
            font-size:18px;
        }
        i.sorted-descending{
            transform: translateY(-20%) rotate(180deg);
        }
    }
    thead tr:first-child td:first-child {
        border-top-left-radius: 10px;
    }
    thead tr:first-child td:last-child {
        border-top-right-radius: 10px;
    }
    tbody tr:last-child td:first-child {
        border-bottom-left-radius: 10px;
    }
    tbody tr:last-child td:last-child {
        border-bottom-right-radius: 10px;
    }
    thead tr:first-child td {
        border-top-style: solid;
    }
    tr td:first-child {
        border-left-style: solid;
    }
}

.sn-delete-note {
    position: absolute;
    top: 1px;
    right: 5px;
    cursor: pointer;
}
</style>

